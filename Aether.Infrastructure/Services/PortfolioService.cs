using Aether.Application.DTOs;
using Aether.Application.Services;
using Aether.Domain.Entities;
using Aether.Domain.Interfaces;
using Aether.Domain.ValueObjects;

namespace Aether.Infrastructure.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _repository;

    public PortfolioService(IPortfolioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(Guid userId)
    {
        var portfolios = await _repository.GetAllByUserIdAsync(userId);
        return portfolios.Select(MapToDto);
    }

    public async Task<PortfolioDto?> GetPortfolioByIdAsync(Guid id, Guid userId)
    {
        var portfolio = await _repository.GetByIdAsync(id);
        if (portfolio == null || portfolio.UserId != userId) return null;
        return MapToDto(portfolio);
    }

    public async Task<PortfolioDto> CreatePortfolioAsync(CreatePortfolioRequest request, Guid userId)
    {
        var portfolio = new Portfolio(request.Name, userId);
        await _repository.AddAsync(portfolio);
        return MapToDto(portfolio);
    }

    public async Task<AssetDto> AddCryptoAssetAsync(Guid portfolioId, AddCryptoAssetRequest request, Guid userId)
    {
        var portfolio = await GetOwnedPortfolioAsync(portfolioId, userId);
        var asset = new CryptoAsset(request.Name, DateTime.UtcNow, new Money(request.AcquisitionPrice, request.Currency), request.Symbol, request.Quantity);
        portfolio.AddAsset(asset);
        await _repository.UpdateAsync(portfolio);
        return MapAssetToDto(asset, "Crypto");
    }

    public async Task<AssetDto> AddSteamSkinAsync(Guid portfolioId, AddSteamSkinRequest request, Guid userId)
    {
        var portfolio = await GetOwnedPortfolioAsync(portfolioId, userId);
        var asset = new SteamSkinAsset(request.Name, DateTime.UtcNow, new Money(request.AcquisitionPrice, request.Currency), request.MarketHashName);
        portfolio.AddAsset(asset);
        await _repository.UpdateAsync(portfolio);
        return MapAssetToDto(asset, "SteamSkin");
    }

    public async Task<AssetDto> AddPhysicalAssetAsync(Guid portfolioId, AddPhysicalAssetRequest request, Guid userId)
    {
        var portfolio = await GetOwnedPortfolioAsync(portfolioId, userId);
        var asset = new PhysicalAsset(request.Name, DateTime.UtcNow, new Money(request.AcquisitionPrice, request.Currency), request.Category, request.Brand, request.Condition);
        portfolio.AddAsset(asset);
        await _repository.UpdateAsync(portfolio);
        return MapAssetToDto(asset, "Physical");
    }

    public async Task RemoveAssetAsync(Guid portfolioId, Guid assetId, Guid userId)
    {
        var portfolio = await GetOwnedPortfolioAsync(portfolioId, userId);
        var removed = portfolio.RemoveAsset(assetId);
        if (!removed) throw new KeyNotFoundException($"Asset {assetId} not found in portfolio.");
        await _repository.UpdateAsync(portfolio);
    }

    private async Task<Portfolio> GetOwnedPortfolioAsync(Guid portfolioId, Guid userId)
    {
        var portfolio = await _repository.GetByIdAsync(portfolioId);
        if (portfolio == null || portfolio.UserId != userId)
            throw new KeyNotFoundException($"Portfolio {portfolioId} not found.");
        return portfolio;
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
