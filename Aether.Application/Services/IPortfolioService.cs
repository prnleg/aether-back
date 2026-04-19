using Aether.Application.DTOs;

namespace Aether.Application.Services;

public interface IPortfolioService
{
    Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(Guid userId);
    Task<PortfolioDto?> GetPortfolioByIdAsync(Guid id, Guid userId);
    Task<PortfolioDto> CreatePortfolioAsync(CreatePortfolioRequest request, Guid userId);
    Task<AssetDto> AddCryptoAssetAsync(Guid portfolioId, AddCryptoAssetRequest request, Guid userId);
    Task<AssetDto> AddSteamSkinAsync(Guid portfolioId, AddSteamSkinRequest request, Guid userId);
    Task<AssetDto> AddPhysicalAssetAsync(Guid portfolioId, AddPhysicalAssetRequest request, Guid userId);
    Task RemoveAssetAsync(Guid portfolioId, Guid assetId, Guid userId);
}
