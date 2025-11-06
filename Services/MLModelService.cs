using System;
using System.IO;
using System.Linq;
using Lab01_Grupo1.Models;
using Microsoft.ML;

namespace Lab01_Grupo1.Services
{
    public class MLModelService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer? _modelo;
        private readonly PredictionEngine<ModelInput, ModelOutput>? _prediccionEngine;

        public MLModelService()
        {
            _mlContext = new MLContext();

            // ⚙️ Ruta del modelo entrenado (ajústala si tu estructura cambia)
            string modeloRuta = Path.Combine(
                AppContext.BaseDirectory,
                "ML", "Data", "config", "Training data", "Prueba", "MLModel.mlmodel"
            );

            if (File.Exists(modeloRuta))
            {
                try
                {
                    _modelo = _mlContext.Model.Load(modeloRuta, out _);
                    _prediccionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(_modelo);
                    Console.WriteLine($"✅ Modelo cargado correctamente desde: {modeloRuta}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al cargar modelo ML: {ex.Message}. Se usará modo simulado.");
                    _modelo = null;
                    _prediccionEngine = null;
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No se encontró el modelo ML en: {modeloRuta}. Se usará modo simulado.");
                _modelo = null;
                _prediccionEngine = null;
            }
        }

        /// <summary>
        /// Analiza el sentimiento del comentario (usa ML real o simulación si no hay modelo)
        /// </summary>
        public (string Prediccion, float Confianza) AnalizarSentimiento(string comentario)
        {
            if (string.IsNullOrWhiteSpace(comentario))
                return ("Desconocido", 0f);

            // Si el modelo no existe, usamos un análisis básico por palabras clave
            if (_prediccionEngine == null)
            {
                return AnalisisFallback(comentario);
            }

            try
            {
                var input = new ModelInput { Comentario = comentario };
                var resultado = _prediccionEngine.Predict(input);

                string etiqueta = MapLabelString(resultado.PredictedLabel);
                float score = resultado.Score?.Max() ?? 0f;

                Console.WriteLine($"🧠 [{comentario}] → {etiqueta} ({score:F2})");

                return (etiqueta, score);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al analizar sentimiento: {ex.Message}");
                return AnalisisFallback(comentario);
            }
        }

        /// <summary>
        /// Simulación de análisis de sentimiento (modo fallback)
        /// </summary>
        private static (string, float) AnalisisFallback(string comentario)
        {
            string texto = comentario.ToLower();

            if (texto.Contains("gracias") || texto.Contains("excelente") || texto.Contains("bueno") || texto.Contains("feliz"))
                return ("Positivo", 0.9f);

            if (texto.Contains("mal") || texto.Contains("terrible") || texto.Contains("pésimo") || texto.Contains("malo"))
                return ("Negativo", 0.9f);

            return ("Neutro", 0.6f);
        }

        /// <summary>
        /// Convierte etiquetas del modelo a texto legible
        /// </summary>
        private static string MapLabelString(string rawKey)
        {
            var k = (rawKey ?? string.Empty).Trim().ToLower();

            if (k == "1" || k == "positivo" || k == "true" || k == "pos")
                return "Positivo";

            if (k == "0" || k == "negativo" || k == "false" || k == "neg")
                return "Negativo";

            if (k == "2" || k == "neutro")
                return "Neutro";

            return "Desconocido";
        }
    }

    // ✅ Clases base para el modelo
    public class ModelInput
    {
        public string Comentario { get; set; } = string.Empty;
    }

    public class ModelOutput
    {
        public string PredictedLabel { get; set; } = string.Empty;
        public float[]? Score { get; set; }
    }
}
