using AppFitNutri.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFitNutri
{

    public class MauiAuthTokenStore : ITokenStore
    {
        private const string Key = "auth_token";

        public async Task<string?> GetTokenAsync(CancellationToken ct = default)
            => await SecureStorage.GetAsync(Key);

        public Task SetTokenAsync(string token, CancellationToken ct = default)
            => SecureStorage.SetAsync(Key, token);

        public Task ClearAsync(CancellationToken ct = default)
        {
            SecureStorage.Remove(Key);
            return Task.CompletedTask;
        }
    }
}
