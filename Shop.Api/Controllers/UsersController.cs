using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts.Users;
using Shop.Application.Services;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.RegisterAsync(request.ToDto(), cancellationToken);
        var response = user.ToResponse();
        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }
}
