using Aether.Domain.Common;

namespace Aether.Application.Features.Portfolio;

public interface IPortfolioService
{
    Task<Result<IEnumerable<PortfolioDto>>> GetUserPortfoliosAsync(Guid userId);
    Task<Result<Guid>> GetOrCreateDefaultPortfolioIdAsync(Guid userId);
    Task<Result<PortfolioDto>> GetPortfolioByIdAsync(Guid id, Guid userId);
    Task<Result<PortfolioDto>> CreatePortfolioAsync(CreatePortfolioRequest request, Guid userId);
    Task<Result<AssetDto>> AddCryptoAssetAsync(Guid portfolioId, AddCryptoAssetRequest request, Guid userId);
    Task<Result<AssetDto>> AddSteamSkinAsync(Guid portfolioId, AddSteamSkinRequest request, Guid userId);
    Task<Result<AssetDto>> AddPhysicalAssetAsync(Guid portfolioId, AddPhysicalAssetRequest request, Guid userId);
    Task<Result> RemoveAssetAsync(Guid portfolioId, Guid assetId, Guid userId);
}
