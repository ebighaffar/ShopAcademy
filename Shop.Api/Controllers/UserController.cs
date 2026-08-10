using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts.User;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers;

public class UserController(UserService userService) : BaseController
{
    [HttpPost(UserUriConstants.Register)]
    public async Task<ApiResult<UserResponse>> Register(RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userService.RegisterAsync(request.ToDto(), cancellationToken);
        var response = user.ToResponse();
        return ApiResult<UserResponse>.Created(response, null);
    }
}