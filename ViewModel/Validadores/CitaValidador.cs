using System;
using Model;

namespace ViewModel.Validadores
{
    /// <summary>
    /// Clase estática que contiene métodos de validación para citas
    /// </summary>
    public static class CitaValidador
    {
        /// <summary>
        /// Valida todos los campos para crear una nueva cita
        /// </summary>
        /// <param name="cliente">Cliente asociado a la cita</param>
        /// <param name="fecha">Fecha de la cita</param>
        /// <param name="horario">Horario de la cita</param>
        /// <param name="modalidad">Modalidad de la cita</param>
        /// <param name="motivo">Motivo de la cita (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si es válido, false en caso contrario</returns>
        public static bool ValidarNuevaCita(
            Cliente cliente,
            DateTime? fecha,
            string horario,
            string modalidad,
            string motivo,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar cliente (obligatorio para nueva cita)
            if (cliente == null)
            {
                mensajeError = "Debe seleccionar un cliente";
                return false;
            }

            // Validar campos comunes
            return ValidarCamposComunesCita(fecha, horario, modalidad, motivo, out mensajeError);
        }

        /// <summary>
        /// Valida todos los campos de una cita existente para edición
        /// </summary>
        public static bool ValidarCita(
            DateTime? fecha,
            string horario,
            string modalidad,
            string motivo,
            string estado,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar campos comunes
            if (!ValidarCamposComunesCita(fecha, horario, modalidad, motivo, out mensajeError))
                return false;

            // Validar estado (obligatorio para edición)
            if (string.IsNullOrWhiteSpace(estado))
            {
                mensajeError = "El estado es obligatorio";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida los campos comunes de una cita (fecha, horario, modalidad y motivo)
        /// </summary>
        /// <param name="fecha">Fecha de la cita</param>
        /// <param name="horario">Horario de la cita</param>
        /// <param name="modalidad">Modalidad de la cita</param>
        /// <param name="motivo">Motivo de la cita (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si todos los campos comunes son válidos</returns>
        private static bool ValidarCamposComunesCita(
            DateTime? fecha,
            string horario,
            string modalidad,
            string motivo,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar fecha
            if (!ValidarFecha(fecha, out mensajeError))
                return false;

            // Validar horario (obligatorio)
            if (string.IsNullOrWhiteSpace(horario))
            {
                mensajeError = "El horario es obligatorio";
                return false;
            }

            // Validar modalidad (obligatoria)
            if (string.IsNullOrWhiteSpace(modalidad))
            {
                mensajeError = "La modalidad es obligatoria";
                return false;
            }

            // Validar motivo (opcional)
            if (!string.IsNullOrWhiteSpace(motivo) && motivo.Trim().Length > 500)
            {
                mensajeError = "El motivo no puede exceder los 500 caracteres";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida la fecha de la cita
        /// </summary>
        /// <param name="fecha">Fecha a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si la fecha es válida</returns>
        public static bool ValidarFecha(DateTime? fecha, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar que la fecha no sea null (obligatoria)
            if (!fecha.HasValue)
            {
                mensajeError = "La fecha es obligatoria";
                return false;
            }

            // Validar que la fecha no sea anterior a hoy
            if (fecha.Value.Date < DateTime.Now.Date)
            {
                mensajeError = "La fecha no puede ser anterior al día actual";
                return false;
            }

            return true;
        }
    }
}
