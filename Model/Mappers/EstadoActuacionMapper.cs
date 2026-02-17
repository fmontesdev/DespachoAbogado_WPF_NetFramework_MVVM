using System.Collections.Generic;

namespace Model.Mappers
{
    /// <summary>
    /// Mapper para convertir estados de actuación entre BD y UI
    /// </summary>
    public static class EstadoActuacionMapper
    {
        /// <summary>
        /// Diccionario de mapeo entre valores de estado de actuación en la BD y su representación formateada para la UI
        /// </summary>
        private static readonly Dictionary<string, string> _estados = new Dictionary<string, string>
        {
            { "pendiente", "Pendiente" },
            { "realizada", "Realizada" },
            { "cancelada", "Cancelada" }
        };

        /// <summary>
        /// Obtiene todos los estados de actuación disponibles para UI
        /// </summary>
        /// <returns>Diccionario con claves de BD (lowercase) y valores formateados para UI</returns>
        public static Dictionary<string, string> ObtenerTodos() => _estados;

        /// <summary>
        /// Convierte un valor de estado de actuación de la BD a su representación formateada para la UI
        /// </summary>
        /// <param name="valorBD">Valor de estado desde la base de datos (ej: "pendiente", "realizada")</param>
        /// <returns>Texto formateado para mostrar en la UI (ej: "Pendiente", "Realizada") o el valor original si no se encuentra</returns>
        public static string DeBDaUI(string valorBD)
        {
            if (string.IsNullOrEmpty(valorBD)) return string.Empty;
            
            string key = valorBD.ToLower();
            return _estados.ContainsKey(key) ? _estados[key] : valorBD;
        }
    }
}

