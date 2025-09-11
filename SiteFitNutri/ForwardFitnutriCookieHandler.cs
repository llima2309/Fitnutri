namespace SiteFitNutri
{
    public sealed class ForwardFitnutriCookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;

        public ForwardFitnutriCookieHandler(IHttpContextAccessor http) => _http = http;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var ctx = _http.HttpContext;
            if (ctx?.Request.Cookies.TryGetValue("fitnutri_auth", out var jwt) == true)
            {
                // Encaminha como COOKIE (a API lê via OnMessageReceived do JwtBearer)
                request.Headers.Remove("Cookie");
                request.Headers.Add("Cookie", $"fitnutri_auth={jwt}");

                // Alternativa: repassar como Authorization (se preferir)
                // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
            return base.SendAsync(request, ct);
        }
    }
}
