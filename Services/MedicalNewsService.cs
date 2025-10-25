using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NoticiasMedicas.Services
{
    public class MedicalArticle
    {
        public string title { get; set; }
        public string link { get; set; }
        public string source { get; set; }
        public string published { get; set; }
    }

    // Clases para GNews
    public class GNewsResponse
    {
        public int totalArticles { get; set; }
        public List<GNewsArticle> articles { get; set; }
    }

    public class GNewsArticle
    {
        public string title { get; set; }
        public string url { get; set; }
        public GNewsSource source { get; set; }
        public string publishedAt { get; set; }
    }

    public class GNewsSource
    {
        public string name { get; set; }
    }

    public class MedicalNewsService
    {
        private readonly HttpClient _httpClient;

        public MedicalNewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<MedicalArticle>> GetMedicalNewsAsync()
        {
            try
            {
                // GNews API con tu API key real
                string apiKey = "5098ff602e4a66ced16cf98b187fe6ac";
                string url = $"https://gnews.io/api/v4/search?q=medicina%20salud%20hospital&lang=es&max=10&apikey={apiKey}";
                
                var response = await _httpClient.GetFromJsonAsync<GNewsResponse>(url);
                
                if (response?.articles != null && response.articles.Any())
                {
                    return response.articles.Select(article => new MedicalArticle
                    {
                        title = article.title ?? "Sin título",
                        link = article.url ?? "#",
                        source = article.source?.name ?? "Fuente desconocida",
                        published = FormatDate(article.publishedAt)
                    }).ToList();
                }
                
                return await GetSampleMedicalNews();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return await GetSampleMedicalNews();
            }
        }

        private string FormatDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date.ToString("yyyy-MM-dd");
            }
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        private async Task<List<MedicalArticle>> GetSampleMedicalNews()
        {
            // Noticias de ejemplo como fallback
            return new List<MedicalArticle>
            {
                new MedicalArticle
                {
                    title = "Descubren nuevo tratamiento para la artritis reumatoide",
                    link = "https://www.who.int/news-room/fact-sheets/detail/arthritis",
                    source = "Organización Mundial de la Salud",
                    published = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")
                },
                new MedicalArticle
                {
                    title = "Avances en la inteligencia artificial para diagnóstico médico",
                    link = "https://www.ncbi.nlm.nih.gov/pmc/articles/PMC7322364/",
                    source = "National Institutes of Health",
                    published = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd")
                },
                new MedicalArticle
                {
                    title = "Importancia del ejercicio cardiovascular en la salud mental",
                    link = "https://www.heart.org/en/healthy-living/fitness/fitness-basics/aha-recs-for-physical-activity-in-adults",
                    source = "American Heart Association",
                    published = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd")
                },
                new MedicalArticle
                {
                    title = "Nuevas guías para el manejo de la diabetes tipo 2",
                    link = "https://www.diabetes.org/resources/statistics/statistics-about-diabetes",
                    source = "American Diabetes Association",
                    published = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd")
                },
                new MedicalArticle
                {
                    title = "Estudio revela beneficios de la meditación en la presión arterial",
                    link = "https://www.nccih.nih.gov/health/meditation-in-depth",
                    source = "National Center for Complementary Health",
                    published = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd")
                }
            };
        }
    }
}