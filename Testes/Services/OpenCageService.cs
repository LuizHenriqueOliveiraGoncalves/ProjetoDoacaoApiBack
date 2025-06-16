using System.Text.Json;

namespace Testes.Services
{
    public class OpenCageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenCageService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = configuration["OpenCage:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new ArgumentException("Chave da API do OpenCage não configurada.");
        }

        public async Task<(double? latitude, double? longitude)> GetCoordinatesAsync(string fullAddress)
        {
            try
            {
                var url = $"https://api.opencagedata.com/geocode/v1/json?q={Uri.EscapeDataString(fullAddress)}&key={_apiKey}&language=pt-BR";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return (null, null);

                var content = await response.Content.ReadAsStringAsync();
                using var json = JsonDocument.Parse(content);

                if (json.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    if (firstResult.TryGetProperty("geometry", out var geometry))
                    {
                        var lat = geometry.GetProperty("lat").GetDouble();
                        var lng = geometry.GetProperty("lng").GetDouble();
                        return (lat, lng);
                    }
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar OpenCage: {ex.Message}");
                return (null, null);
            }
        }
    }
}