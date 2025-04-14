using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace demo_api_rest
{
    public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string? ApiKey { get; set; }
    }
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        //TODO Change to whatever name you want to use
        private const string ApiKeyHeaderName = "x-api-key";

        public ApiKeyAuthenticationHandler(
           IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
           ILoggerFactory logger,
           UrlEncoder encoder,
           ISystemClock clock,
           IConfiguration configuration)
           : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(ApiKeyHeaderName))
            {
                return Task.FromResult(AuthenticateResult.Fail("Header was not found"));
            }

            string token = Request.Headers[ApiKeyHeaderName].ToString();


            if (string.IsNullOrEmpty(token) || !IsApiKeyValid(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Token is invalid"));
            }
            else
            {
                Claim[] claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, "jason p"),
                    new Claim(ClaimTypes.Email, "jasonp***@gmail.com"),
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, nameof(ApiKeyAuthenticationHandler));
                AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

        }
        private bool IsApiKeyValid(string apiKey)
        {
            var allowedApiKeys = _configuration.GetSection("AllowedApiKeys").Get<string[]>();
            return allowedApiKeys.Contains(apiKey);
        }
    }
}
