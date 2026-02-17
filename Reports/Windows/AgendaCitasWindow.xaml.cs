using System;
using System.Windows;
using Model.DataSets;
using Reports.Reports;

namespace Reports.Windows
{
    /// <summary>
    /// Ventana para visualizar el informe de Agenda de Citas con Crystal Reports
    /// </summary>
    public partial class AgendaCitasWindow : Window
    {
        private AgendaCitasInforme _report;

        /// <summary>
        /// Constructor que inicializa la ventana y carga el informe de Crystal Reports
        /// </summary>
        /// <param name="dataSet">DataSet tipado con las citas del día</param>
        /// <param name="fecha">Fecha de las citas para mostrar en el título</param>
        public AgendaCitasWindow(dsAgendaCitas dataSet, DateTime fecha)
        {
            InitializeComponent();

            try
            {
                // Crear instancia del informe (recurso incrustado)
                _report = new AgendaCitasInforme();

                // Asignar el DataSet al informe
                _report.SetDataSource(dataSet);

                // Actualizar el título de la ventana con la fecha
                this.Title = $"Agenda de Citas - {fecha:dd/MM/yyyy}";

                // Asignar el documento al visor
                crystalReportViewer.ReportSource = _report;
                crystalReportViewer.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar el informe:\n\n{ex.Message}\n\nDetalles: {ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                this.Close();
            }
        }

        /// <summary>
        /// Libera los recursos del informe cuando se cierra la ventana
        /// </summary>
        /// <param name="e">Argumentos del evento</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_report != null)
            {
                _report.Close();
                _report.Dispose();
            }
        }
    }
}






