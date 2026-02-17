using ViewModel.Services;
using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    /// <summary>
    /// Clase de pruebas de integración para validar el control de horarios ocupados en citas.
    /// Verifica que el sistema rechace citas cuando un horario ya está ocupado en la misma fecha
    /// </summary>
    [TestClass]
    public sealed class TestHorarioOcupado
    {
        /// <summary>
        /// Cliente de prueba para el primer test (horario ocupado)
        /// </summary>
        private Cliente cliente1;

        /// <summary>
        /// Cliente de prueba para el segundo test (diferentes horarios)
        /// </summary>
        private Cliente cliente2;

        /// <summary>
        /// Método de inicialización que se ejecuta antes de cada prueba.
        /// Configura los datos de prueba con dos clientes diferentes para evitar conflictos
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            cliente1 = new Cliente
            {
                Nombre = "Pedro",
                Apellidos = "Sánchez López",
                Dni = "12345678A",
                Email = "pedro.sanchez.test1@test.com",
                Poblacion = "Madrid"
            };

            cliente2 = new Cliente
            {
                Nombre = "Ana",
                Apellidos = "García Ruiz",
                Dni = "87654321B",
                Email = "ana.garcia.test2@test.com",
                Poblacion = "Barcelona"
            };
        }

        /// <summary>
        /// Prueba de integración que verifica el control de horarios ocupados.
        /// Crea una primera cita en un horario específico (debe tener éxito),
        /// e intenta crear una segunda cita en el mismo horario y fecha (debe ser rechazada con excepción).
        /// Al finalizar, limpia todos los datos de prueba de la base de datos
        /// </summary>
        [TestMethod]
        public async Task TestControlHorario_HorarioOcupado_SegundaCitaDenegada()
        {
            // Arrange
            CitaService citaService = new CitaService();
            ClienteService clienteService = new ClienteService();
            Cliente clienteCreado = null;
            Cita cita1Creada = null;
            Cita cita2Creada = null;

            try
            {
                // Crear el cliente1 de prueba
                await clienteService.CrearClienteAsync(cliente1);
                var clientes = await clienteService.ObtenerClientesAsync();
                clienteCreado = clientes.FirstOrDefault(c =>
                    c.Dni == cliente1.Dni && c.Email == cliente1.Email);
                Assert.IsNotNull(clienteCreado, "No se pudo recuperar el cliente creado de la BD");

                // Act 1: Crear la primera cita
                var cita1 = new Cita
                {
                    IdCliente = clienteCreado.IdCliente,
                    Fecha = DateTime.Today.AddDays(1),
                    Horario = "09:00-10:00",
                    Modalidad = "presencial",
                    Estado = "programada"
                };

                await citaService.CrearCitaAsync(cita1);
                var citas = await citaService.ObtenerCitasAsync();
                cita1Creada = citas.FirstOrDefault(c =>
                    c.IdCliente == clienteCreado.IdCliente &&
                    c.Fecha.Date == cita1.Fecha.Date &&
                    c.Horario == cita1.Horario);

                // Assert 1: La primera cita debe haberse creado exitosamente
                Assert.IsNotNull(cita1Creada, "La primera cita debería haberse creado");
                Assert.IsTrue(cita1Creada.IdCita > 0, "La primera cita debería tener un ID válido");

                // Act 2: Intentar crear la segunda cita (mismo horario, misma fecha)
                var cita2 = new Cita
                {
                    IdCliente = clienteCreado.IdCliente,
                    Fecha = DateTime.Today.AddDays(1),
                    Horario = "09:00-10:00",
                    Modalidad = "videollamada",
                    Estado = "programada"
                };

                ArgumentException excepcionCapturada = null;
                try
                {
                    await citaService.CrearCitaAsync(cita2);

                    // Si llegamos aquí, verificar que NO se creó en la BD
                    citas = await citaService.ObtenerCitasAsync();
                    cita2Creada = citas.FirstOrDefault(c =>
                        c.IdCliente == clienteCreado.IdCliente &&
                        c.Fecha.Date == cita2.Fecha.Date &&
                        c.Horario == cita2.Horario &&
                        c.Modalidad == cita2.Modalidad);
                }
                catch (ArgumentException ex)
                {
                    excepcionCapturada = ex;
                }

                // Assert 2: La segunda cita debe haber sido rechazada
                Assert.IsNotNull(excepcionCapturada, "Debería lanzarse una ArgumentException al intentar crear la segunda cita");
                Assert.IsTrue(excepcionCapturada.Message.Contains("horario"),
                    "El mensaje de error debería mencionar el horario");
                Assert.IsTrue(excepcionCapturada.Message.Contains("disponible"),
                    "El mensaje de error debería indicar que no está disponible");
                Assert.IsNull(cita2Creada, "La segunda cita NO debería haberse creado en la BD");
            }
            finally
            {
                // Eliminar datos de prueba en orden inverso
                await EliminarDatos(cita2Creada, citaService.EliminarCitaAsync);
                await EliminarDatos(cita1Creada, citaService.EliminarCitaAsync);
                await EliminarDatos(clienteCreado, clienteService.EliminarClienteAsync);
            }
        }

        /// <summary>
        /// Prueba que verifica que se pueden crear múltiples citas en diferentes horarios.
        /// Crea dos citas para el mismo día pero en horarios diferentes (ambas deben tener éxito).
        /// Al finalizar, limpia todos los datos de prueba de la base de datos
        /// </summary>
        [TestMethod]
        public async Task TestControlHorario_DiferentesHorarios_AmbasCitasCreadas()
        {
            // Arrange
            CitaService citaService = new CitaService();
            ClienteService clienteService = new ClienteService();
            Cliente clienteCreado = null;
            Cita cita1Creada = null;
            Cita cita2Creada = null;

            try
            {
                // Crear el cliente2 de prueba
                await clienteService.CrearClienteAsync(cliente2);
                var clientes = await clienteService.ObtenerClientesAsync();
                clienteCreado = clientes.FirstOrDefault(c =>
                    c.Dni == cliente2.Dni && c.Email == cliente2.Email);
                Assert.IsNotNull(clienteCreado, "No se pudo recuperar el cliente creado de la BD");

                // Act 1: Crear la primera cita
                var cita1 = new Cita
                {
                    IdCliente = clienteCreado.IdCliente,
                    Fecha = DateTime.Today.AddDays(2),
                    Horario = "09:00-10:00",
                    Modalidad = "presencial",
                    Estado = "programada"
                };

                await citaService.CrearCitaAsync(cita1);

                // Act 2: Crear la segunda cita en diferente horario
                var cita2 = new Cita
                {
                    IdCliente = clienteCreado.IdCliente,
                    Fecha = DateTime.Today.AddDays(2),
                    Horario = "10:00-11:00",
                    Modalidad = "videollamada",
                    Estado = "programada"
                };

                await citaService.CrearCitaAsync(cita2);

                // Verificar que ambas citas se crearon
                var citas = await citaService.ObtenerCitasAsync();
                cita1Creada = citas.FirstOrDefault(c =>
                    c.IdCliente == clienteCreado.IdCliente &&
                    c.Fecha.Date == cita1.Fecha.Date &&
                    c.Horario == cita1.Horario);

                cita2Creada = citas.FirstOrDefault(c =>
                    c.IdCliente == clienteCreado.IdCliente &&
                    c.Fecha.Date == cita2.Fecha.Date &&
                    c.Horario == cita2.Horario);

                // Assert: Ambas citas deben haberse creado exitosamente
                Assert.IsNotNull(cita1Creada, "La primera cita debería haberse creado");
                Assert.IsNotNull(cita2Creada, "La segunda cita debería haberse creado");
                Assert.AreNotEqual(cita1Creada.Horario, cita2Creada.Horario, "Las citas deberían tener horarios diferentes");
            }
            finally
            {
                // Eliminar datos de prueba en orden inverso
                await EliminarDatos(cita2Creada, citaService.EliminarCitaAsync);
                await EliminarDatos(cita1Creada, citaService.EliminarCitaAsync);
                await EliminarDatos(clienteCreado, clienteService.EliminarClienteAsync);
            }
        }

        /// <summary>
        /// Método auxiliar genérico para eliminar datos de prueba de la base de datos.
        /// Ignora errores durante la eliminación para no interferir con las pruebas
        /// </summary>
        /// <typeparam name="T">Tipo de entidad a eliminar</typeparam>
        /// <param name="entidad">Entidad a eliminar</param>
        /// <param name="eliminar">Función de eliminación del servicio</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        private async Task EliminarDatos<T>(T entidad, Func<T, Task> eliminar) where T : class
        {
            try
            {
                if (entidad != null)
                {
                    await eliminar(entidad);
                }
            }
            catch { /* Ignorar errores */ }
        }
    }
}
