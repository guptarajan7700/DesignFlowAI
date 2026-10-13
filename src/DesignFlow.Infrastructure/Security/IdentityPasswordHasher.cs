using DesignFlow.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace DesignFlow.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private readonly object _user = new();

    public string Hash(string password) => _hasher.HashPassword(_user, password);

    public bool Verify(string hash, string password) =>
        _hasher.VerifyHashedPassword(_user, hash, password) is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
}
