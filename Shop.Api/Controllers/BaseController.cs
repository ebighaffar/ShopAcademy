using Microsoft.AspNetCore.Mvc;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}")]
public abstract class BaseController : ControllerBase
{
}
