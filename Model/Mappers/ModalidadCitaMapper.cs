using System.Collections.Generic;

namespace Model.Mappers
{
    /// <summary>
    /// Mapper para convertir modalidades de cita entre BD y UI
    /// </summary>
    public static class ModalidadCitaMapper
    {
        /// <summary>
        /// Diccionario de mapeo entre valores de modalidad en la BD y su representación formateada para la UI
        /// </summary>
        private static readonly Dictionary<string, string> _modalidades = new Dictionary<string, string>
        {
            { "presencial", "Presencial" },
            { "telefonica", "Telefónica" },
            { "videollamada", "Videollamada" }
        };

        /// <summary>
        /// Obtiene todas las modalidades disponibles para UI
        /// </summary>
        /// <returns>Diccionario con claves de BD (lowercase) y valores formateados para UI</returns>
        public static Dictionary<string, string> ObtenerTodas() => _modalidades;

        /// <summary>
        /// Convierte un valor de modalidad de la BD a su representación formateada para la UI
        /// </summary>
        /// <param name="valorBD">Valor de modalidad desde la base de datos (ej: "presencial", "telefonica")</param>
        /// <returns>Texto formateado para mostrar en la UI (ej: "Presencial", "Telefónica") o el valor original si no se encuentra</returns>
        public static string DeBDaUI(string valorBD)
        {
            if (string.IsNullOrEmpty(valorBD)) return string.Empty;
            
            string key = valorBD.ToLower();
            return _modalidades.ContainsKey(key) ? _modalidades[key] : valorBD;
        }
    }
}
