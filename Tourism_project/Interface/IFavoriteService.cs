using Microsoft.AspNetCore.Mvc;
using Tourism_project.Dtos.Home;

namespace Tourism_project.Interface
{
    public interface IFavoriteService
    {
        Task<IActionResult> AddFavoriteAsync(AddFavoriteDto dto);
        Task<IActionResult> RemoveFavoriteAsync(AddFavoriteDto dto);
        Task<IActionResult> GetUserFavoritesAsync(int userId);
    }
}
