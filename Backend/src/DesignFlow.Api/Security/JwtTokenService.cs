using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DesignFlow.Application.Abstractions;
using DesignFlow.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DesignFlow.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "DesignFlowAI";

    public string Audience { get; set; } = "DesignFlowAI";

    public string Key { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 120;
}

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IConfiguration configuration)
    {
        _options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user, IReadOnlyList<string> roles)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes <= 0 ? 120 : _options.ExpiresMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

public sealed class HttpCurrentUser : ICurrentUser
{
    public HttpCurrentUser(IHttpContextAccessor accessor)
    {
        var principal = accessor.HttpContext?.User;
        var rawId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        UserId = Guid.TryParse(rawId, out var userId) ? userId : Guid.Empty;
        Roles = principal?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray() ?? Array.Empty<string>();
    }

    public Guid UserId { get; }

    public IReadOnlyCollection<string> Roles { get; }
}
