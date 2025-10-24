using System.Text.Json;

namespace Lab01_Grupo1.Services
{
    public class OpenFDAService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.fda.gov/drug/label.json";

        public OpenFDAService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetDrugInformation(string drugName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(drugName))
                    return "Por favor ingrese un nombre de medicamento válido.";

                var url = $"{BaseUrl}?search=generic_name:{Uri.EscapeDataString(drugName)}&limit=1";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return ParseDrugInfo(json, drugName);
                }
                
                return "No se pudo obtener información del medicamento en este momento.";
            }
            catch (Exception ex)
            {
                return $"Error al consultar la base de datos de medicamentos: {ex.Message}";
            }
        }

        private string ParseDrugInfo(string json, string drugName)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var results = document.RootElement.GetProperty("results");
                
                if (results.GetArrayLength() > 0)
                {
                    var drug = results[0];
                    
                    // Extraer información del medicamento
                    var genericName = drug.TryGetProperty("generic_name", out var genName) 
                        ? genName.GetString() ?? drugName 
                        : drugName;
                    
                    var purposes = new List<string>();
                    if (drug.TryGetProperty("purpose", out var purposeArray))
                    {
                        purposes.AddRange(purposeArray.EnumerateArray()
                            .Select(p => p.GetString())
                            .Where(p => !string.IsNullOrEmpty(p))!);
                    }
                    
                    var warnings = new List<string>();
                    if (drug.TryGetProperty("warnings", out var warningsArray))
                    {
                        warnings.AddRange(warningsArray.EnumerateArray()
                            .Select(w => w.GetString())
                            .Where(w => !string.IsNullOrEmpty(w))!);
                    }

                    // Construir respuesta amigable
                    var info = $"💊 **{genericName}**\n\n";
                    
                    if (purposes.Any())
                        info += $"**Usos:** {string.Join(", ", purposes.Take(2))}\n\n";
                    
                    if (warnings.Any())
                        info += $"**Advertencias:** {string.Join(" ", warnings.Take(1))}";
                    
                    return info;
                }
                
                return $"No se encontró información para el medicamento: {drugName}";
            }
            catch (Exception)
            {
                return $"Se encontró información para {drugName}, pero hubo un error al procesarla.";
            }
        }
    }
}

