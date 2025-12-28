using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using AssetTrackerPro.Domain.Entities;
using AssetTrackerPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AssetTrackerPro.Api.Auth;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AssetTrackerDbContext _dbContext;
    private readonly PasswordHasher<Users> _passwordHasher = new();

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AssetTrackerDbContext dbContext)
        : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var encodedCredentials = authHeader["Basic ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(encodedCredentials))
        {
            return AuthenticateResult.Fail("Kimlik bilgileri eksik.");
        }

        string decoded;
        try
        {
            var bytes = Convert.FromBase64String(encodedCredentials);
            decoded = Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Geçersiz Base64 kimlik bilgisi.");
        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex <= 0)
        {
            return AuthenticateResult.Fail("Geçersiz kimlik bilgisi formatı.");
        }

        var username = decoded[..separatorIndex];
        var password = decoded[(separatorIndex + 1)..];

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return AuthenticateResult.Fail("Kullanıcı adı veya şifre hatalı.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return AuthenticateResult.Fail("Kullanıcı adı veya şifre hatalı.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "Basic";
        return base.HandleChallengeAsync(properties);
    }
}
