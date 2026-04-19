using Aether.Application.Features.Portfolio;
using Aether.Domain.Common;
using Aether.Domain.Entities;
using Aether.Domain.Interfaces;
using Aether.Domain.Specifications;
using Aether.Domain.ValueObjects;

namespace Aether.Infrastructure.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _repository;

    public PortfolioService(IPortfolioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<PortfolioDto>>> GetUserPortfoliosAsync(Guid userId)
    {
        var portfolios = (await _repository.ListAsync(new PortfoliosByUserIdSpec(userId))).ToList();

        if (portfolios.Count == 0)
        {
            var defaultPortfolio = new Portfolio("My Portfolio", userId);
            await _repository.AddAsync(defaultPortfolio);
            portfolios.Add(defaultPortfolio);
        }

        return Result.Success<IEnumerable<PortfolioDto>>(portfolios.Select(MapToDto));
    }

    public async Task<Result<Guid>> GetOrCreateDefaultPortfolioIdAsync(Guid userId)
    {
        var portfolios = (await _repository.ListAsync(new PortfoliosByUserIdSpec(userId))).ToList();
        if (portfolios.Count > 0) return Result.Success(portfolios[0].Id);

        var portfolio = new Portfolio("My Portfolio", userId);
        await _repository.AddAsync(portfolio);
        return Result.Success(portfolio.Id);
    }

    public async Task<Result<PortfolioDto>> GetPortfolioByIdAsync(Guid id, Guid userId)
    {
        var portfolio = await _repository.FindAsync(new PortfolioByIdAndUserIdSpec(id, userId));
        if (portfolio == null)
            return Result.Failure<PortfolioDto>(Error.NotFound($"Portfolio {id} not found."));

        return Result.Success(MapToDto(portfolio));
    }

    public async Task<Result<PortfolioDto>> CreatePortfolioAsync(CreatePortfolioRequest request, Guid userId)
    {
        var portfolio = new Portfolio(request.Name, userId);
        await _repository.AddAsync(portfolio);
        return Result.Success(MapToDto(portfolio));
    }

    public async Task<Result<AssetDto>> AddCryptoAssetAsync(Guid portfolioId, AddCryptoAssetRequest request, Guid userId)
    {
        var result = await GetOwnedPortfolioAsync(portfolioId, userId);
        if (result.IsFailure) return Result.Failure<AssetDto>(result.Error);

        var asset = new CryptoAsset(request.Name, DateTime.UtcNow,
            new Money(request.AcquisitionPrice, request.Currency),
            request.Symbol, request.Quantity);

        result.Value.AddAsset(asset);
        await _repository.UpdateAsync(result.Value);
        return Result.Success(MapAssetToDto(asset, "Crypto"));
    }

    public async Task<Result<AssetDto>> AddSteamSkinAsync(Guid portfolioId, AddSteamSkinRequest request, Guid userId)
    {
        var result = await GetOwnedPortfolioAsync(portfolioId, userId);
        if (result.IsFailure) return Result.Failure<AssetDto>(result.Error);

        var asset = new SteamSkinAsset(request.Name, DateTime.UtcNow,
            new Money(request.AcquisitionPrice, request.Currency),
            request.MarketHashName);

        result.Value.AddAsset(asset);
        await _repository.UpdateAsync(result.Value);
        return Result.Success(MapAssetToDto(asset, "SteamSkin"));
    }

    public async Task<Result<AssetDto>> AddPhysicalAssetAsync(Guid portfolioId, AddPhysicalAssetRequest request, Guid userId)
    {
        var result = await GetOwnedPortfolioAsync(portfolioId, userId);
        if (result.IsFailure) return Result.Failure<AssetDto>(result.Error);

        var asset = new PhysicalAsset(request.Name, DateTime.UtcNow,
            new Money(request.AcquisitionPrice, request.Currency),
            request.Category, request.Brand, request.Condition);

        result.Value.AddAsset(asset);
        await _repository.UpdateAsync(result.Value);
        return Result.Success(MapAssetToDto(asset, "Physical"));
    }

    public async Task<Result> RemoveAssetAsync(Guid portfolioId, Guid assetId, Guid userId)
    {
        var result = await GetOwnedPortfolioAsync(portfolioId, userId);
        if (result.IsFailure) return Result.Failure(result.Error);

        if (!result.Value.RemoveAsset(assetId))
            return Result.Failure(Error.NotFound($"Asset {assetId} not found in portfolio."));

        await _repository.UpdateAsync(result.Value);
        return Result.Success();
    }

    private async Task<Result<Portfolio>> GetOwnedPortfolioAsync(Guid portfolioId, Guid userId)
    {
        var portfolio = await _repository.FindAsync(new PortfolioByIdAndUserIdSpec(portfolioId, userId));
        if (portfolio == null)
            return Result.Failure<Portfolio>(Error.NotFound($"Portfolio {portfolioId} not found."));

        return Result.Success(portfolio);
    }

    private static PortfolioDto MapToDto(Portfolio portfolio)
    {
        var assets = portfolio.Assets.Select(a => MapAssetToDto(a, GetDiscriminatorValue(a))).ToList();
        return new PortfolioDto
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Assets = assets,
            TotalValue = portfolio.CalculateTotalValue("USD").Amount,
            Currency = "USD"
        };
    }

    private static AssetDto MapAssetToDto(Asset asset, string assetType)
    {
        var dto = new AssetDto
        {
            Id = asset.Id,
            Name = asset.Name,
            AssetType = assetType,
            AcquisitionDate = asset.AcquisitionDate,
            AcquisitionPrice = asset.AcquisitionPrice.Amount,
            AcquisitionCurrency = asset.AcquisitionPrice.Currency,
            CurrentFloorPrice = asset.CurrentFloorPrice.Amount,
            CurrentFloorCurrency = asset.CurrentFloorPrice.Currency,
            LastUpdated = asset.LastUpdated
        };

        if (asset is CryptoAsset crypto)
        {
            dto.Symbol = crypto.Symbol;
            dto.Quantity = crypto.Quantity;
        }
        else if (asset is SteamSkinAsset skin)
        {
            dto.MarketHashName = skin.MarketHashName;
            dto.AppId = skin.AppId;
        }
        else if (asset is PhysicalAsset physical)
        {
            dto.Category = physical.Category;
            dto.Brand = physical.Brand;
            dto.Condition = physical.Condition;
        }

        return dto;
    }

    private static string GetDiscriminatorValue(Asset asset) => asset switch
    {
        CryptoAsset => "Crypto",
        SteamSkinAsset => "SteamSkin",
        PhysicalAsset => "Physical",
        _ => "Unknown"
    };
}
