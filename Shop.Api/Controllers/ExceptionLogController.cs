using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts.ExceptionLogs;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExceptionLogController(ExceptionLogService exceptionLogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExceptionLogResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var logs = await exceptionLogService.GetAllAsync(cancellationToken);
        return Ok(logs.Select(l => l.ToResponse()));
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<ExceptionLogResponse>>> GetRecent(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var logs = await exceptionLogService.GetRecentAsync(count, cancellationToken);
        return Ok(logs.Select(l => l.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExceptionLogResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var log = await exceptionLogService.GetByIdAsync(id, cancellationToken);
        return Ok(log.ToResponse());
    }
}
