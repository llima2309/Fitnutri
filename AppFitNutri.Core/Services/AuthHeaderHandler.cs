using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFitNutri.Core.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ITokenStore _store;
        public AuthHeaderHandler(ITokenStore store) => _store = store;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var token = await _store.GetTokenAsync(ct);
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(req, ct);
        }
    }
}
