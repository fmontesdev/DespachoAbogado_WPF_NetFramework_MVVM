using System.Collections.Generic;

namespace Model.Configuracion
{
    /// <summary>
    /// Configuración de horarios disponibles en el despacho de abogados.
    /// Define los horarios válidos para las citas según constraint de base de datos.
    /// </summary>
    public static class HorariosDespacho
    {
        /// <summary>
        /// Lista de horarios disponibles para citas en el despacho.
        /// Estos valores corresponden al constraint CHECK en la columna Horario de la tabla Cita.
        /// </summary>
        public static readonly List<string> Horarios = new List<string>
        {
            "09:00-10:00",
            "10:00-11:00",
            "11:00-12:00",
            "12:00-13:00",
            "13:00-14:00",
            "17:00-18:00",
            "18:00-19:00",
            "19:00-20:00"
        };
    }
}
