using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Model.DataSets;
using ViewModel.Command;
using ViewModel.Models;
using ViewModel.Services;
using Reports.Windows;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la vista de informes (InformesView).
    /// Maneja la lista de informes disponibles y coordina la generación de cada tipo de informe
    /// </summary>
    public class InformeViewModel : INotifyPropertyChanged
    {
        private readonly InformeService _informeService;
        private Informe _informeSeleccionado;
        private DateTime _fechaSeleccionada;

        /// <summary>
        /// Colección observable de informes disponibles para el ListBox
        /// </summary>
        public ObservableCollection<Informe> InformesDisponibles { get; set; }

        /// <summary>
        /// Informe seleccionado de la lista
        /// </summary>
        public Informe InformeSeleccionado
        {
            get => _informeSeleccionado;
            set
            {
                _informeSeleccionado = value;
                OnPropertyChanged(nameof(InformeSeleccionado));
                OnPropertyChanged(nameof(TieneInformeSeleccionado));
                OnPropertyChanged(nameof(MostrarFiltroFecha));
            }
        }

        /// <summary>
        /// Fecha seleccionada para el informe de Agenda de Citas
        /// </summary>
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                _fechaSeleccionada = value;
                OnPropertyChanged(nameof(FechaSeleccionada));
            }
        }

        /// <summary>
        /// Indica si hay un informe seleccionado para habilitar el botón de generación
        /// </summary>
        public bool TieneInformeSeleccionado => InformeSeleccionado != null;

        /// <summary>
        /// Indica si se debe mostrar el DatePicker para seleccionar fecha (solo para el informe Agenda de Citas)
        /// </summary>
        public bool MostrarFiltroFecha => InformeSeleccionado?.Tipo == TipoInforme.AgendaCitas;

        /// <summary>
        /// Comando para generar el informe seleccionado
        /// </summary>
        public ICommand GenerarInformeCommand { get; }

        /// <summary>
        /// Constructor que inicializa los servicios, comandos y carga los datos
        /// </summary>
        public InformeViewModel()
        {
            _informeService = new InformeService();

            InformesDisponibles = new ObservableCollection<Informe>();

            // Inicializar Command
            GenerarInformeCommand = new RelayCommand(GenerarInforme);

            // Cargar datos
            InicializarAsync();
        }

        /// <summary>
        /// Inicializa la carga de datos de forma asíncrona
        /// </summary>
        private void InicializarAsync()
        {
            // Inicializar fecha por defecto a hoy
            FechaSeleccionada = DateTime.Now;

            // Cargar informes disponibles
            CargarInformesDisponibles();
        }

        /// <summary>
        /// Carga la lista de informes disponibles en el sistema
        /// </summary>
        private void CargarInformesDisponibles()
        {
            var informes = ListaInformes.ObtenerInformesDisponibles();

            foreach (var informe in informes)
            {
                InformesDisponibles.Add(informe);
            }

            // Seleccionar el primer informe automáticamente
            if (InformesDisponibles.Count > 0)
            {
                InformeSeleccionado = InformesDisponibles[0];
            }
        }

        /// <summary>
        /// Genera y muestra el informe seleccionado según su tipo
        /// </summary>
        private async void GenerarInforme()
        {
            // Validar que haya un informe seleccionado
            if (InformeSeleccionado != null)
            {
                try
                {
                    switch (InformeSeleccionado.Tipo)
                    {
                        case TipoInforme.AgendaCitas:
                            await GenerarAgendaCitas();
                            break;

                        case TipoInforme.ActuacionesPendientes:
                            await GenerarActuacionesPendientes();
                            break;

                        case TipoInforme.ExpedientesPorEstado:
                            await GenerarExpedientesPorEstado();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Mostrar error directamente
                    MessageBox.Show(
                        $"Error al generar el informe: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Genera el informe de Agenda de Citas y abre la ventana del visor
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        private async Task GenerarAgendaCitas()
        {
            var dataSet = await _informeService.GenerarDataSetAgendaCitasAsync(FechaSeleccionada);

            // Verificar si hay datos
            if (dataSet.Citas.Count == 0)
            {
                MessageBox.Show(
                    $"No hay citas programadas para el día {FechaSeleccionada:dd/MM/yyyy}",
                    "Sin datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Abrir ventana con el informe
            var ventana = new AgendaCitasWindow(dataSet, FechaSeleccionada);
            ventana.ShowDialog();
        }

        /// <summary>
        /// Genera el informe de Actuaciones Pendientes y abre la ventana del visor
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        private async Task GenerarActuacionesPendientes()
        {
            var dataSet = await _informeService.GenerarDataSetActuacionesPendientesAsync();

            // Verificar si hay datos
            if (dataSet.Actuaciones.Count == 0)
            {
                MessageBox.Show(
                    "No hay actuaciones pendientes en expedientes activos.",
                    "Sin datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Abrir ventana con el informe
            var ventana = new ActuacionesPendientesWindow(dataSet);
            ventana.ShowDialog();
        }

        /// <summary>
        /// Genera el informe de Expedientes por Estado y abre la ventana del visor
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        private async Task GenerarExpedientesPorEstado()
        {
            var dataSet = await _informeService.GenerarDataSetExpedientesPorEstadoAsync();

            // Verificar si hay datos
            if (dataSet.Expedientes.Count == 0)
            {
                MessageBox.Show(
                    "No hay expedientes registrados en el sistema.",
                    "Sin datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Abrir ventana con el informe
            var ventana = new ExpedientesPorEstadoWindow(dataSet);
            ventana.ShowDialog();
        }

        /// <summary>
        /// Evento que notifica cambios en las propiedades para actualizar la interfaz
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Método auxiliar para invocar el evento PropertyChanged
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que cambió</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


