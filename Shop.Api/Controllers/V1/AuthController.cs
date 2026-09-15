using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Contracts.Auth;
using Shop.Api.Contracts.User;
using Shop.Api.Extensions;
using Shop.Api.Mappers;
using Shop.Application.Services.Interfaces;
using Shop.Domain.Constants;

namespace Shop.Api.Controllers.V1;

[ApiVersion(1.0)]
public class AuthController(IAuthService authService) : BaseController
{
    [AllowAnonymous]
    [HttpPost(AuthUriConstants.Register)]
    public async Task<ApiResult<AuthenticationResponse>> Register(RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.ToDto(), cancellationToken);
        return ApiResult<AuthenticationResponse>.Created(result.ToResponse(), null);
    }

    [AllowAnonymous]
    [HttpPost(AuthUriConstants.Login)]
    public async Task<ApiResult<AuthenticationResponse>> Login(LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.ToDto(), cancellationToken);
        return result.ToResponse();
    }

    [AllowAnonymous]
    [HttpPost(AuthUriConstants.Refresh)]
    public async Task<ApiResult<AuthenticationResponse>> Refresh(RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return result.ToResponse();
    }

    [HttpPost(AuthUriConstants.Logout)]
    public async Task<ApiResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(
            User.GetUserId(), request.RefreshToken, cancellationToken);
        return ApiResult.NoContent();
    }

    [HttpPost(AuthUriConstants.LogoutAll)]
    public async Task<ApiResult> LogoutAll(CancellationToken cancellationToken)
    {
        await authService.LogoutAllAsync(User.GetUserId(), cancellationToken);
        return ApiResult.NoContent();
    }


    [HttpGet(AuthUriConstants.Me)]
    public ApiResult<CurrentUserResponse> Me()
    {
        var response = new CurrentUserResponse(
            User.GetUserId(),
            User.FindFirstValue("name")!,
            User.FindFirstValue("email")!,
            User.FindFirstValue(CustomClaimTypes.AccountStatus)!,
            User.FindAll("role").Select(claim => claim.Value).ToArray());

        return response;
    }
}
