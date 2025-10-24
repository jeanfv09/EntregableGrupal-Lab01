using System.Text.Json;

namespace Lab01_Grupo1.Services
{
    public class OpenFDAService
    {
        private readonly HttpClient _httpClient;

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

                // 🔄 DIFERENTES ESTRATEGIAS DE BÚSQUEDA
                var searchQueries = new[]
                {
                    $"openfda.brand_name:{drugName}",
                    $"openfda.generic_name:{drugName}", 
                    $"openfda.substance_name:{drugName}",
                    $"{drugName}"
                };

                foreach (var searchQuery in searchQueries)
                {
                    var url = $"drug/label.json?search={Uri.EscapeDataString(searchQuery)}&limit=1";
                    var response = await _httpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        
                        // Verificar si realmente encontró resultados
                        using var document = JsonDocument.Parse(json);
                        if (document.RootElement.TryGetProperty("results", out var results) && 
                            results.GetArrayLength() > 0)
                        {
                            return ParseDrugInfo(json, drugName);
                        }
                    }
                }
                
                return $"No se encontró información para: {drugName}. Prueba con el nombre en inglés.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
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
                    
                    // Intentar diferentes campos de nombre
                    var brandName = drug.TryGetProperty("openfda", out var openfda) && 
                                   openfda.TryGetProperty("brand_name", out var brand) 
                        ? brand.EnumerateArray().FirstOrDefault().GetString() 
                        : null;
                        
                    var genericName = drug.TryGetProperty("openfda", out var openfda2) && 
                                     openfda2.TryGetProperty("generic_name", out var generic) 
                        ? generic.EnumerateArray().FirstOrDefault().GetString() 
                        : drugName;

                    var purposes = new List<string>();
                    if (drug.TryGetProperty("purpose", out var purposeArray))
                    {
                        purposes.AddRange(purposeArray.EnumerateArray()
                            .Select(p => p.GetString())
                            .Where(p => !string.IsNullOrEmpty(p))!);
                    }

                    var displayName = brandName ?? genericName ?? drugName;
                    var info = $"💊 **{displayName}**\n\n";
                    
                    if (purposes.Any())
                        info += $"**Usos:** {string.Join(", ", purposes.Take(2))}";
                    else
                        info += "**Usos:** Información disponible en la base de datos FDA";
                    
                    return info;
                }
                
                return $"No se encontró información para: {drugName}";
            }
            catch (Exception)
            {
                return $"Información obtenida para {drugName}";
            }
        }
    }
}