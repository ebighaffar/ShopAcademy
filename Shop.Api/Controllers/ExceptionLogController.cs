using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts.ExceptionLog;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers;

public class ExceptionLogController(ExceptionLogService exceptionLogService) : BaseController
{
    [HttpGet(ExceptionLogUriConstants.GetAll)]
    public async Task<ApiResult<IReadOnlyList<ExceptionLogResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var logs = await exceptionLogService.GetAllAsync(cancellationToken);
        return logs.Select(l => l.ToResponse()).ToList();
    }

    [HttpGet(ExceptionLogUriConstants.GetRecent)]
    public async Task<ApiResult<IReadOnlyList<ExceptionLogResponse>>> GetRecent(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var logs = await exceptionLogService.GetRecentAsync(count, cancellationToken);
        return logs.Select(l => l.ToResponse()).ToList();
    }

    [HttpGet(ExceptionLogUriConstants.GetById)]
    public async Task<ApiResult<ExceptionLogResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var log = await exceptionLogService.GetByIdAsync(id, cancellationToken);
        return log.ToResponse();
    }
}
