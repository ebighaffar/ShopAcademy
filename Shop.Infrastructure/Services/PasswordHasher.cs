using Microsoft.AspNetCore.Identity;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(User user, string password)
        => _passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string password)
        => _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password)
           != PasswordVerificationResult.Failed;
}
