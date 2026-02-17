using Model.DataSets;
using Model.Mappers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Repositorios
{
    /// <summary>
    /// Repositorio para gestionar las operaciones de acceso a datos de citas
    /// </summary>
    public class CitaRepositorio
    {
        /// <summary>
        /// Instancia del contexto de la base de datos
        /// </summary>
        private readonly DespachoAbogadoEntities _context = new DespachoAbogadoEntities();

        /// <summary>
        /// Obtiene todas las citas de la base de datos incluyendo sus relaciones
        /// </summary>
        /// <returns>Lista de todas las citas con sus relaciones asociadas</returns>
        public async Task<List<Cita>> SeleccionarAsync()
        {
            return await _context.Cita.Include("Cliente").Include("Expediente").ToListAsync();
        }

        /// <summary>
        /// Crea una nueva cita en la base de datos
        /// </summary>
        /// <param name="entity">Cita a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task CrearAsync(Cita entity)
        {
            _context.Cita.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Guarda los cambios realizados en una cita
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task GuardarAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina una cita de la base de datos
        /// </summary>
        /// <param name="entity">Cita a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task EliminarAsync(Cita entity)
        {
            _context.Cita.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Inicia una transacción en la base de datos
        /// </summary>
        /// <returns>Objeto que representa la transacción iniciada</returns>
        public DbContextTransaction IniciarTransaccion()
        {
            return _context.Database.BeginTransaction();
        }

        /// <summary>
        /// Obtiene el contexto actual de Entity Framework
        /// </summary>
        /// <returns>Contexto de Entity Framework para operaciones dentro de transacciones</returns>
        public DespachoAbogadoEntities ObtenerContexto()
        {
            return _context;
        }

        /// <summary>
        /// Genera el DataSet para el informe de Agenda de Citas por día
        /// </summary>
        /// <param name="fecha">Fecha para filtrar las citas</param>
        /// <returns>DataSet tipado con las citas del día especificado</returns>
        public async Task<dsAgendaCitas> ObtenerDataSetAgendaCitasAsync(DateTime fecha)
        {
            // Crear una instancia del DataSet tipado
            var dataSet = new dsAgendaCitas();
            
            // Definir el rango de fechas para el día completo
            var fechaInicio = fecha.Date;
            var fechaFin = fecha.Date.AddDays(1);

            // Obtener todas las citas del día especificado incluyendo relaciones
            var citas = await _context.Cita
                .Include("Cliente")
                .Include("Expediente")
                .Where(c => c.Fecha >= fechaInicio && c.Fecha < fechaFin)
                .OrderBy(c => c.Horario)
                .ToListAsync();

            // Mapear entidades al DataSet
            foreach (var cita in citas)
            {
                dataSet.Citas.AddCitasRow(
                    cita.Fecha,
                    cita.Horario,
                    cita.Cliente.NombreCompleto,
                    cita.Expediente?.Codigo ?? "Sin expediente",
                    ModalidadCitaMapper.DeBDaUI(cita.Modalidad),
                    EstadoCitaMapper.DeBDaUI(cita.Estado),
                    string.IsNullOrEmpty(cita.Motivo) ? "Sin motivo" : cita.Motivo
                );
            }

            return dataSet;
        }
    }
}
