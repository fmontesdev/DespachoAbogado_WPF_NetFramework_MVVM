using System.Collections.Generic;

namespace Model.Mappers
{
    /// <summary>
    /// Mapper para convertir jurisdicciones entre BD y UI
    /// </summary>
    public static class JurisdiccionMapper
    {
        /// <summary>
        /// Diccionario de mapeo entre valores de jurisdicción en la BD y su representación formateada para la UI
        /// </summary>
        private static readonly Dictionary<string, string> _jurisdicciones = new Dictionary<string, string>
        {
            { "civil", "Civil" },
            { "penal", "Penal" },
            { "social", "Social" },
            { "mercantil", "Mercantil" },
            { "contencioso", "Contencioso" }
        };

        /// <summary>
        /// Obtiene todas las jurisdicciones disponibles para UI
        /// </summary>
        /// <returns>Diccionario con claves de BD (lowercase) y valores formateados para UI</returns>
        public static Dictionary<string, string> ObtenerTodas() => _jurisdicciones;

        /// <summary>
        /// Convierte un valor de jurisdicción de la BD a su representación formateada para la UI
        /// </summary>
        /// <param name="valorBD">Valor de jurisdicción desde la base de datos (ej: "civil", "penal")</param>
        /// <returns>Texto formateado para mostrar en la UI (ej: "Civil", "Penal") o el valor original si no se encuentra</returns>
        public static string DeBDaUI(string valorBD)
        {
            if (string.IsNullOrEmpty(valorBD)) return string.Empty;
            
            string key = valorBD.ToLower();
            return _jurisdicciones.ContainsKey(key) ? _jurisdicciones[key] : valorBD;
        }
    }
}
