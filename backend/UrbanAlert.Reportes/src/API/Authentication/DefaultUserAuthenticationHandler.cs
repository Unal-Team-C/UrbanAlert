using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace API.Authentication;

public class DefaultUserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DefaultUser";

    public DefaultUserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, "usuario-por-defecto"),
            new Claim(ClaimTypes.Role, "Administrador")
        ];

        ClaimsIdentity identity = new(claims, SchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
