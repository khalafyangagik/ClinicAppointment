using System.Net.Http.Json;

namespace ClinicClient.Services
{
    public class AuthApi
    {
        private readonly HttpClient _http;

        public AuthApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var request = new { email, password };

            var response = await _http.PostAsJsonAsync("api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<LoginResponse>();

            return json?.Token;
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
    }
}
