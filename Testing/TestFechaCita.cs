using ViewModel.Validadores;
using Model;
using System;
using System.Collections.Generic;

namespace Testing
{
    /// <summary>
    /// Clase de pruebas unitarias para validar las fechas de cita.
    /// Verifica que la validación de fechas acepte fechas futuras y hoy, y rechace fechas pasadas
    /// </summary>
    [TestClass]
    public sealed class TestFechaCita
    {
        /// <summary>
        /// Lista de fechas válidas para pruebas positivas (hoy y fechas futuras)
        /// </summary>
        private readonly List<DateTime?> fechasValidas = new List<DateTime?>
        {
            DateTime.Today,
            DateTime.Today.AddDays(1),
            DateTime.Today.AddDays(7),
            DateTime.Today.AddMonths(1),
            DateTime.Today.AddYears(1)
        };
        
        /// <summary>
        /// Lista de fechas inválidas para pruebas negativas (fechas pasadas)
        /// </summary>
        private readonly List<DateTime?> fechasInvalidas = new List<DateTime?>
        {
            DateTime.Today.AddDays(-1),
            DateTime.Today.AddDays(-7),
            DateTime.Today.AddMonths(-1),
            DateTime.Today.AddYears(-1)
        };

        /// <summary>
        /// Prueba que verifica que fechas válidas (hoy y futuras) son aceptadas por la validación.
        /// Recorre la lista de fechas válidas y comprueba que ValidarFecha retorna true
        /// </summary>
        [TestMethod]
        public void TestFechasValidas_RetornaTrue()
        {
            foreach (var fecha in fechasValidas)
            {
                // Arrange
                string mensajeError;

                // Act
                bool resultado = CitaValidador.ValidarFecha(fecha, out mensajeError);

                // Assert
                Assert.IsTrue(resultado, $"La fecha '{fecha:dd/MM/yyyy}' debería ser válida");
                Assert.AreEqual(string.Empty, mensajeError, $"No debería haber mensaje de error para la fecha '{fecha:dd/MM/yyyy}'");
            }
        }

        /// <summary>
        /// Prueba que verifica que fechas inválidas (pasadas) son rechazadas por la validación.
        /// Recorre la lista de fechas inválidas y comprueba que ValidarFecha retorna false
        /// </summary>
        [TestMethod]
        public void TestFechasInvalidas_RetornaFalse()
        {
            foreach (var fecha in fechasInvalidas)
            {
                // Arrange
                string mensajeError;

                // Act
                bool resultado = CitaValidador.ValidarFecha(fecha, out mensajeError);

                // Assert
                Assert.IsFalse(resultado, $"La fecha '{fecha:dd/MM/yyyy}' debería ser inválida (anterior a hoy)");
                Assert.IsFalse(string.IsNullOrEmpty(mensajeError), $"Debería haber un mensaje de error para la fecha '{fecha:dd/MM/yyyy}'");
            }
        }

        /// <summary>
        /// Prueba que verifica que una fecha null es rechazada por la validación.
        /// Comprueba que ValidarFecha retorna false cuando la fecha es null
        /// </summary>
        [TestMethod]
        public void TestFechaNull_RetornaFalse()
        {
            // Arrange
            DateTime? fecha = null;
            string mensajeError;

            // Act
            bool resultado = CitaValidador.ValidarFecha(fecha, out mensajeError);

            // Assert
            Assert.IsFalse(resultado, "La fecha null debería ser inválida");
            Assert.AreEqual("La fecha es obligatoria", mensajeError);
        }

        /// <summary>
        /// Prueba que verifica que se genera el mensaje de error correcto para fechas pasadas.
        /// Comprueba que el mensaje de error es específico para fechas anteriores a hoy
        /// </summary>
        [TestMethod]
        public void TestFechaAyer_MensajeError()
        {
            // Arrange
            DateTime? fecha = DateTime.Today.AddDays(-1);
            string mensajeError;

            // Act
            bool resultado = CitaValidador.ValidarFecha(fecha, out mensajeError);

            // Assert
            Assert.IsFalse(resultado);
            Assert.AreEqual("La fecha no puede ser anterior al día actual", mensajeError);
        }

        /// <summary>
        /// Prueba que verifica que la fecha de hoy es aceptada.
        /// Comprueba que ValidarFecha retorna true para DateTime.Today
        /// </summary>
        [TestMethod]
        public void TestFechaHoy_RetornaTrue()
        {
            // Arrange
            DateTime? fecha = DateTime.Today;
            string mensajeError;

            // Act
            bool resultado = CitaValidador.ValidarFecha(fecha, out mensajeError);

            // Assert
            Assert.IsTrue(resultado, "La fecha de hoy debería ser válida");
            Assert.AreEqual(string.Empty, mensajeError);
        }
    }
}

