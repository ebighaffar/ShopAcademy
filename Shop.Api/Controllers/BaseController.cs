using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}")]
public abstract class BaseController : ControllerBase
{
}