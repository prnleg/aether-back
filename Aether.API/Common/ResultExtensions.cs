using Aether.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Aether.API.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result.Value);

        return result.Error.Code switch
        {
            "NotFound" => controller.NotFound(new { error = result.Error.Code, message = result.Error.Message }),
            "Unauthorized" => controller.Unauthorized(new { error = result.Error.Code, message = result.Error.Message }),
            "Conflict" => controller.Conflict(new { error = result.Error.Code, message = result.Error.Message }),
            _ => controller.BadRequest(new { error = result.Error.Code, message = result.Error.Message })
        };
    }

    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.NoContent();

        return result.Error.Code switch
        {
            "NotFound" => controller.NotFound(new { error = result.Error.Code, message = result.Error.Message }),
            "Unauthorized" => controller.Unauthorized(new { error = result.Error.Code, message = result.Error.Message }),
            "Conflict" => controller.Conflict(new { error = result.Error.Code, message = result.Error.Message }),
            _ => controller.BadRequest(new { error = result.Error.Code, message = result.Error.Message })
        };
    }
}
