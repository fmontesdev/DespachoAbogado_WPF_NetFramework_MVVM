using ViewModel.Validadores;
using System.Collections.Generic;

namespace Testing
{
    /// <summary>
    /// Clase de pruebas unitarias para validar el formato de emails de clientes.
    /// Verifica que la validación de emails acepte formatos válidos y rechace inválidos
    /// </summary>
    [TestClass]
    public sealed class TestEmailCliente
    {
        /// <summary>
        /// Lista de emails en formato válido para pruebas positivas
        /// </summary>
        private readonly List<string> emailsValidos = new List<string>
        {
            "juan.perez@gmail.com",
            "maria.lopez@despacho.es",
            "cliente123@dominio.com",
            "usuario@mail.dominio.com",
            "abogado@despacho.abogados.es"
        };
        
        /// <summary>
        /// Lista de emails en formato inválido para pruebas negativas
        /// </summary>
        private readonly List<string> emailsInvalidos = new List<string>
        {
            "cliente.com",              // Sin @
            "cliente@",                 // Sin dominio
            "cliente@dominio",          // Sin extensión
            "cliente @dominio.com",     // Con espacio
            "@dominio.com",             // Sin usuario
            "cliente@@dominio.com"      // Doble @
        };

        /// <summary>
        /// Prueba que verifica que emails con formato válido son aceptados por la validación.
        /// Recorre la lista de emails válidos y comprueba que ValidarEmail retorna true
        /// </summary>
        [TestMethod]
        public void TestEmailsValidos_RetornaTrue()
        {
            foreach (var email in emailsValidos)
            {
                // Arrange
                string mensajeError;

                // Act
                bool resultado = ClienteValidador.ValidarEmail(email, out mensajeError);

                // Assert
                Assert.IsTrue(resultado, $"El email '{email}' debería ser válido");
                Assert.AreEqual(string.Empty, mensajeError, $"No debería haber mensaje de error para '{email}'");
            }
        }

        /// <summary>
        /// Prueba que verifica que emails con formato inválido son rechazados por la validación.
        /// Recorre la lista de emails inválidos y comprueba que ValidarEmail retorna false
        /// </summary>
        [TestMethod]
        public void TestEmailsInvalidos_RetornaFalse()
        {
            foreach (var email in emailsInvalidos)
            {
                // Arrange
                string mensajeError;

                // Act
                bool resultado = ClienteValidador.ValidarEmail(email, out mensajeError);

                // Assert
                Assert.IsFalse(resultado, $"El email '{email}' debería ser inválido");
                Assert.IsFalse(string.IsNullOrEmpty(mensajeError), $"Debería haber un mensaje de error para '{email}'");
            }
        }

        /// <summary>
        /// Prueba que verifica que un email vacío es rechazado por la validación.
        /// Comprueba que ValidarEmail retorna false cuando el email está vacío
        /// </summary>
        [TestMethod]
        public void TestEmailVacio_RetornaFalse()
        {
            // Arrange
            string email = "";
            string mensajeError;

            // Act
            bool resultado = ClienteValidador.ValidarEmail(email, out mensajeError);

            // Assert
            Assert.IsFalse(resultado, "El email vacío debería ser inválido");
            Assert.AreEqual("El email del cliente es obligatorio", mensajeError);
        }

        /// <summary>
        /// Prueba que verifica que un email null es rechazado por la validación.
        /// Comprueba que ValidarEmail retorna false cuando el email es null
        /// </summary>
        [TestMethod]
        public void TestEmailNull_RetornaFalse()
        {
            // Arrange
            string email = null;
            string mensajeError;

            // Act
            bool resultado = ClienteValidador.ValidarEmail(email, out mensajeError);

            // Assert
            Assert.IsFalse(resultado, "El email null debería ser inválido");
            Assert.AreEqual("El email del cliente es obligatorio", mensajeError);
        }
    }
}
