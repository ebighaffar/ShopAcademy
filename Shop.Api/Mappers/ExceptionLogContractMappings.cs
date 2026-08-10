using Shop.Api.Contracts.ExceptionLog;
using Shop.Application.DTOs;

namespace Shop.Api.Mappers;

public static class ExceptionLogContractMappings
{
    public static ExceptionLogResponse ToResponse(this ExceptionLogDto dto) => new(
        dto.Id,
        dto.ExceptionType,
        dto.Message,
        dto.StackTrace,
        dto.Source,
        dto.RequestPath,
        dto.RequestMethod,
        dto.StatusCode,
        dto.CreatedAt);
}
