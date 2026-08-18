using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Contracts.User;
using Shop.Api.Controllers;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers.V1;

[ApiVersion(1.0)]
public class UserController(UserService userService) : ApiControllerBase
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
