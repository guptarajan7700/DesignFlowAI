using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using DesignFlow.Domain.Entities;

namespace DesignFlow.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwords;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens, IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || !_passwords.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var roles = await _users.GetRoleNamesAsync(user.UserId, cancellationToken);
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (token, expires) = _tokens.CreateToken(user, roles);
        return new LoginResponse(token, expires, ToProfile(user, roles));
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var roleName = string.IsNullOrWhiteSpace(request.Role) ? RoleNames.Client : request.Role.Trim();
        if (roleName is not (RoleNames.Client or RoleNames.Designer))
        {
            throw new ForbiddenException("Self-registration can create a Client or Designer account.");
        }

        if (await _users.FindByEmailAsync(email, cancellationToken) is not null)
        {
            throw new ConflictException("An account with that email already exists.");
        }

        var roleId = await _users.GetRoleIdAsync(roleName, cancellationToken)
            ?? throw new NotFoundException($"Role {roleName} is not configured.");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwords.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            IsActive = true
        };

        await _users.AddAsync(user, cancellationToken);
        await _users.AddRoleAsync(new UserRole
        {
            UserRoleId = Guid.NewGuid(),
            UserId = user.UserId,
            RoleId = roleId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToProfile(user, new[] { roleName });
    }

    internal static UserProfileDto ToProfile(User user, IReadOnlyList<string> roles) =>
        new(user.UserId, user.Email, user.FirstName, user.LastName, user.Phone, user.IsActive, roles);
}
