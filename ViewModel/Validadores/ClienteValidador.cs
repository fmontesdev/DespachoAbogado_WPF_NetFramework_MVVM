using System;
using System.Text.RegularExpressions;
using Model;

namespace ViewModel.Validadores
{
    /// <summary>
    /// Clase que encapsula las validaciones de negocio para la entidad Cliente.
    /// Proporciona métodos estáticos para validar cada campo de un cliente
    /// </summary>
    public static class ClienteValidador
    {
        // Patrones de validación
        private static readonly string PatronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private static readonly string PatronDni = @"^\d{8}[A-Za-z]$";
        private static readonly string PatronTelefono = @"^[6789]\d{8}$";

        /// <summary>
        /// Valida todos los campos de un cliente y devuelve el primer error encontrado
        /// </summary>
        /// <param name="nombre">Nombre del cliente</param>
        /// <param name="apellidos">Apellidos del cliente</param>
        /// <param name="dni">DNI del cliente</param>
        /// <param name="telefono">Teléfono del cliente (opcional)</param>
        /// <param name="email">Email del cliente</param>
        /// <param name="poblacion">Población del cliente</param>
        /// <param name="direccion">Dirección del cliente (opcional)</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si todos los campos son válidos, false en caso contrario</returns>
        public static bool ValidarCliente(
            string nombre,
            string apellidos,
            string dni,
            string telefono,
            string email,
            string poblacion,
            string direccion,
            out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validación del Nombre
            if (!ValidarNombre(nombre, out mensajeError))
                return false;

            // Validación de los Apellidos
            if (!ValidarApellidos(apellidos, out mensajeError))
                return false;

            // Validación del DNI
            if (!ValidarDni(dni, out mensajeError))
                return false;

            // Validación del Teléfono (opcional)
            if (!ValidarTelefono(telefono, out mensajeError))
                return false;

            // Validación del Email
            if (!ValidarEmail(email, out mensajeError))
                return false;

            // Validación de la Población
            if (!ValidarPoblacion(poblacion, out mensajeError))
                return false;

            // Validación de la Dirección (opcional)
            if (!ValidarDireccion(direccion, out mensajeError))
                return false;

            return true;
        }

        /// <summary>
        /// Valida el nombre del cliente
        /// </summary>
        /// <param name="nombre">Nombre a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si el nombre es válido</returns>
        private static bool ValidarNombre(string nombre, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre del cliente es obligatorio";
                return false;
            }

            if (nombre.Trim().Length < 2)
            {
                mensajeError = "El nombre debe tener al menos 2 caracteres";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida los apellidos del cliente
        /// </summary>
        /// <param name="apellidos">Apellidos a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si los apellidos son válidos</returns>
        private static bool ValidarApellidos(string apellidos, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(apellidos))
            {
                mensajeError = "Los apellidos del cliente son obligatorios";
                return false;
            }

            if (apellidos.Trim().Length < 2)
            {
                mensajeError = "Los apellidos deben tener al menos 2 caracteres";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el DNI del cliente (8 dígitos seguidos de una letra)
        /// </summary>
        /// <param name="dni">DNI a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si el DNI es válido</returns>
        private static bool ValidarDni(string dni, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(dni))
            {
                mensajeError = "El DNI del cliente es obligatorio";
                return false;
            }

            if (!Regex.IsMatch(dni.Trim(), PatronDni))
            {
                mensajeError = "El DNI debe tener 8 números seguidos de una letra (ej: 12345678A)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el teléfono del cliente (opcional, 9 dígitos empezando por 6, 7, 8 o 9)
        /// </summary>
        /// <param name="telefono">Teléfono a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si el teléfono es válido o está vacío</returns>
        private static bool ValidarTelefono(string telefono, out string mensajeError)
        {
            mensajeError = string.Empty;

            // El teléfono es opcional
            if (string.IsNullOrWhiteSpace(telefono))
                return true;

            if (!Regex.IsMatch(telefono.Trim(), PatronTelefono))
            {
                mensajeError = "El teléfono debe tener 9 dígitos y empezar por 6, 7, 8 o 9 (ej: 612345678)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el email del cliente
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si el email es válido</returns>
        public static bool ValidarEmail(string email, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                mensajeError = "El email del cliente es obligatorio";
                return false;
            }

            if (!Regex.IsMatch(email, PatronEmail))
            {
                mensajeError = "El email debe tener un formato válido (ejemplo@email.com)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida la población del cliente
        /// </summary>
        /// <param name="poblacion">Población a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si la población es válida</returns>
        private static bool ValidarPoblacion(string poblacion, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(poblacion))
            {
                mensajeError = "La población del cliente es obligatoria";
                return false;
            }

            if (poblacion.Trim().Length < 2)
            {
                mensajeError = "La población debe tener al menos 2 caracteres";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida la dirección del cliente (opcional, no puede ser solo números)
        /// </summary>
        /// <param name="direccion">Dirección a validar</param>
        /// <param name="mensajeError">Mensaje de error si la validación falla</param>
        /// <returns>True si la dirección es válida o está vacía</returns>
        private static bool ValidarDireccion(string direccion, out string mensajeError)
        {
            mensajeError = string.Empty;

            // La dirección es opcional
            if (string.IsNullOrWhiteSpace(direccion))
                return true;

            if (int.TryParse(direccion.Trim(), out _))
            {
                mensajeError = "La dirección no puede ser solo números";
                return false;
            }

            if (direccion.Trim().Length < 3)
            {
                mensajeError = "La dirección debe tener al menos 3 caracteres";
                return false;
            }

            return true;
        }
    }
}
