using System.Collections.Generic;

namespace Model.Mappers
{
    /// <summary>
    /// Mapper para convertir tipos de actuación entre BD y UI
    /// </summary>
    public static class TipoActuacionMapper
    {
        /// <summary>
        /// Diccionario de mapeo entre valores de tipo de actuación en la BD y su representación formateada para la UI
        /// </summary>
        private static readonly Dictionary<string, string> _tipos = new Dictionary<string, string>
        {
            { "llamada", "Llamada" },
            { "reunion", "Reunión" },
            { "escrito", "Escrito" },
            { "email", "Email" },
            { "notificacion", "Notificación" },
            { "tarea", "Tarea" },
            { "plazo", "Plazo" }
        };

        /// <summary>
        /// Obtiene todos los tipos de actuación disponibles para UI
        /// </summary>
        /// <returns>Diccionario con claves de BD (lowercase) y valores formateados para UI</returns>
        public static Dictionary<string, string> ObtenerTodos() => _tipos;

        /// <summary>
        /// Convierte un valor de tipo de actuación de la BD a su representación formateada para la UI
        /// </summary>
        /// <param name="valorBD">Valor de tipo desde la base de datos (ej: "llamada", "reunion")</param>
        /// <returns>Texto formateado para mostrar en la UI (ej: "Llamada", "Reunión") o el valor original si no se encuentra</returns>
        public static string DeBDaUI(string valorBD)
        {
            if (string.IsNullOrEmpty(valorBD)) return string.Empty;
            
            string key = valorBD.ToLower();
            return _tipos.ContainsKey(key) ? _tipos[key] : valorBD;
        }
    }
}

