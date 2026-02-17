using System;
using Model;

namespace ViewModel.Validadores
{
    /// <summary>
    /// Clase estática que contiene métodos de validación para actuaciones
    /// </summary>
    public static class ActuacionValidador
    {
        /// <summary>
        /// Valida todos los campos para crear una nueva actuación
        /// </summary>
        /// <param name="expediente">Expediente asociado a la actuación</param>
        /// <param name="tipo">Tipo de actuación</param>
        /// <param name="descripcion">Descripción de la actuación (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si es válido, false en caso contrario</returns>
        public static bool ValidarNuevaActuacion(
            Expediente expediente,
            string tipo,
            string descripcion,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar expediente (obligatorio para nueva actuación)
            if (expediente == null)
            {
                mensajeError = "Debe seleccionar un expediente";
                return false;
            }

            // Validar campos comunes
            return ValidarCamposComunesActuacion(tipo, descripcion, out mensajeError);
        }

        /// <summary>
        /// Valida todos los campos de una actuación existente
        /// </summary>
        public static bool ValidarActuacion(
            string tipo,
            string descripcion,
            string estado,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar campos comunes
            if (!ValidarCamposComunesActuacion(tipo, descripcion, out mensajeError))
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
        /// Valida los campos comunes de una actuación (tipo y descripción)
        /// </summary>
        /// <param name="tipo">Tipo de actuación</param>
        /// <param name="descripcion">Descripción de la actuación (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si todos los campos comunes son válidos</returns>
        private static bool ValidarCamposComunesActuacion(
            string tipo,
            string descripcion,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar tipo (obligatorio)
            if (string.IsNullOrWhiteSpace(tipo))
            {
                mensajeError = "El tipo de actuación es obligatorio";
                return false;
            }

            // Validar descripción (opcional)
            if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > 500)
            {
                mensajeError = "La descripción no puede exceder los 500 caracteres";
                return false;
            }

            return true;
        }
    }
}

