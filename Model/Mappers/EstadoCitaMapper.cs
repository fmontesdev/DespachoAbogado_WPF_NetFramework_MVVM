using System.Collections.Generic;

namespace Model.Mappers
{
    /// <summary>
    /// Mapper para convertir estados de cita entre BD y UI
    /// </summary>
    public static class EstadoCitaMapper
    {
        /// <summary>
        /// Diccionario de mapeo entre valores de estado de cita en la BD y su representación formateada para la UI
        /// </summary>
        private static readonly Dictionary<string, string> _estados = new Dictionary<string, string>
        {
            { "programada", "Programada" },
            { "realizada", "Realizada" },
            { "cancelada", "Cancelada" }
        };

        /// <summary>
        /// Obtiene todos los estados de cita disponibles para UI
        /// </summary>
        /// <returns>Diccionario con claves de BD (lowercase) y valores formateados para UI</returns>
        public static Dictionary<string, string> ObtenerTodos() => _estados;

        /// <summary>
        /// Convierte un valor de estado de cita de la BD a su representación formateada para la UI
        /// </summary>
        /// <param name="valorBD">Valor de estado desde la base de datos (ej: "programada", "realizada")</param>
        /// <returns>Texto formateado para mostrar en la UI (ej: "Programada", "Realizada") o el valor original si no se encuentra</returns>
        public static string DeBDaUI(string valorBD)
        {
            if (string.IsNullOrEmpty(valorBD)) return string.Empty;
            
            string key = valorBD.ToLower();
            return _estados.ContainsKey(key) ? _estados[key] : valorBD;
        }
    }
}
