using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ClinicClient.Services
{
    public class ClinicApi
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;

        public ClinicApi(HttpClient http, ILocalStorageService storage)
        {
            _http = http;
            _storage = storage;
        }

        private async Task AddToken()
        {
            var token = await _storage.GetItemAsStringAsync("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ClinicDto>> GetAllAsync()
        {
            await AddToken();
            return await _http.GetFromJsonAsync<List<ClinicDto>>("api/Clinics");
        }

        public async Task CreateAsync(CreateClinicDto dto)
        {
            await AddToken();
            await _http.PostAsJsonAsync("api/Clinics", dto);
        }
    }

    public class ClinicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class CreateClinicDto
    {
        public string Name { get; set; } = "";
    }
}
