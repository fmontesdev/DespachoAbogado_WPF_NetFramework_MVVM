using System;
using Model;

namespace ViewModel.Validadores
{
    /// <summary>
    /// Clase estática que contiene métodos de validación para expedientes
    /// </summary>
    public static class ExpedienteValidador
    {
        /// <summary>
        /// Valida todos los campos para crear un nuevo expediente
        /// </summary>
        /// <param name="cliente">Cliente asociado al expediente</param>
        /// <param name="titulo">Título del expediente</param>
        /// <param name="descripcion">Descripción del expediente (opcional)</param>
        /// <param name="jurisdiccion">Jurisdicción del expediente</param>
        /// <param name="organo">Órgano judicial (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si es válido, false en caso contrario</returns>
        public static bool ValidarNuevoExpediente(
            Cliente cliente,
            string titulo,
            string descripcion,
            string jurisdiccion,
            string organo,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar cliente (obligatorio para nuevo expediente)
            if (cliente == null)
            {
                mensajeError = "Debe seleccionar un cliente";
                return false;
            }

            // Validar campos comunes
            return ValidarCamposComunesExpediente(titulo, descripcion, jurisdiccion, organo, out mensajeError);
        }

        /// <summary>
        /// Valida todos los campos de un expediente existente
        /// </summary>
        public static bool ValidarExpediente(
            string titulo,
            string descripcion,
            string jurisdiccion,
            string organo,
            DateTime? cierre,
            string estado,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar campos comunes
            if (!ValidarCamposComunesExpediente(titulo, descripcion, jurisdiccion, organo, out mensajeError))
                return false;

            // Validar fecha de cierre (opcional)
            if (cierre.HasValue && cierre.Value.Date < DateTime.Now.Date)
            {
                mensajeError = "La fecha de cierre no puede ser anterior a la fecha actual";
                return false;
            }

            // Validar estado (obligatorio para edición)
            if (string.IsNullOrWhiteSpace(estado))
            {
                mensajeError = "El estado es obligatorio";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida los campos comunes de un expediente (título, descripción, jurisdicción y órgano)
        /// </summary>
        /// <param name="titulo">Título del expediente</param>
        /// <param name="descripcion">Descripción del expediente (opcional)</param>
        /// <param name="jurisdiccion">Jurisdicción del expediente</param>
        /// <param name="organo">Órgano judicial (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si todos los campos comunes son válidos</returns>
        private static bool ValidarCamposComunesExpediente(
            string titulo,
            string descripcion,
            string jurisdiccion,
            string organo,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar título
            if (!ValidarTitulo(titulo, out mensajeError))
                return false;

            // Validar descripción (opcional)
            if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > 500)
            {
                mensajeError = "La descripción no puede exceder los 500 caracteres";
                return false;
            }

            // Validar jurisdicción (obligatorio)
            if (string.IsNullOrWhiteSpace(jurisdiccion))
            {
                mensajeError = "La jurisdicción es obligatoria";
                return false;
            }

            // Validar órgano (opcional)
            if (!string.IsNullOrWhiteSpace(organo) && organo.Trim().Length > 200)
            {
                mensajeError = "El órgano no puede exceder los 200 caracteres";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el título del expediente
        /// </summary>
        /// <param name="titulo">Título a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si el título es válido</returns>
        private static bool ValidarTitulo(string titulo, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar que el título no sea null o vacío (obligatorio)
            if (string.IsNullOrWhiteSpace(titulo))
            {
                mensajeError = "El título es obligatorio";
                return false;
            }

            // Validar longitud mínima
            if (titulo.Trim().Length < 3)
            {
                mensajeError = "El título debe tener al menos 3 caracteres";
                return false;
            }

            // Validar longitud máxima
            if (titulo.Trim().Length > 200)
            {
                mensajeError = "El título no puede exceder los 200 caracteres";
                return false;
            }

            return true;
        }
    }
}

