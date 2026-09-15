using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts;
using Shop.Api.Contracts.Auth;
using Shop.Api.Contracts.User;
using Shop.Api.Extensions;
using Shop.Api.Mappers;
using Shop.Domain.Constants;

namespace Shop.Api.Controllers.V1;

[ApiVersion(1.0)]
public class UserController() : BaseController
{
}