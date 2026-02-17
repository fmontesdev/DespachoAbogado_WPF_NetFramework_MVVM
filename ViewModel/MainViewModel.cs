using System;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel.Command;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la ventana principal (MainWindow).
    /// Maneja la navegación entre vistas y coordina la interfaz principal de la aplicación
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _titulo;

        /// <summary>
        /// Título de la ventana principal que cambia según la vista activa
        /// </summary>
        public string Titulo
        {
            get => _titulo;
            set
            {
                _titulo = value;
                OnPropertyChanged(nameof(Titulo));
            }
        }

        /// <summary>
        /// Comando para mostrar la vista de citas
        /// </summary>
        public ICommand MostrarCitasCommand { get; }
        
        /// <summary>
        /// Comando para mostrar la vista de clientes
        /// </summary>
        public ICommand MostrarClientesCommand { get; }
        
        /// <summary>
        /// Comando para mostrar la vista de expedientes
        /// </summary>
        public ICommand MostrarExpedientesCommand { get; }
        
        /// <summary>
        /// Comando para mostrar la vista de actuaciones
        /// </summary>
        public ICommand MostrarActuacionesCommand { get; }
        
        /// <summary>
        /// Comando para mostrar la vista de informes
        /// </summary>
        public ICommand MostrarInformesCommand { get; }
        
        /// <summary>
        /// Comando para salir de la aplicación
        /// </summary>
        public ICommand SalirCommand { get; }

        /// <summary>
        /// Constructor que inicializa todos los comandos de navegación
        /// </summary>
        public MainViewModel()
        {
            // Inicializar Commands
            MostrarCitasCommand = new RelayCommand(MostrarCitas);
            MostrarClientesCommand = new RelayCommand(MostrarClientes);
            MostrarExpedientesCommand = new RelayCommand(MostrarExpedientes);
            MostrarActuacionesCommand = new RelayCommand(MostrarActuaciones);
            MostrarInformesCommand = new RelayCommand(MostrarInformes);
            SalirCommand = new RelayCommand(Salir);
        }

        /// <summary>
        /// Inicializa la vista por defecto mostrando las citas
        /// </summary>
        public void InicializarVistaInicial()
        {
            MostrarCitas();
        }

        /// <summary>
        /// Cambia a la vista de citas
        /// </summary>
        private void MostrarCitas()
        {
            CambiarVista?.Invoke("Citas", null);
            Titulo = "Gestión de Citas";
        }

        /// <summary>
        /// Cambia a la vista de clientes
        /// </summary>
        private void MostrarClientes()
        {
            CambiarVista?.Invoke("Clientes", null);
            Titulo = "Gestión de Clientes";
        }

        /// <summary>
        /// Cambia a la vista de expedientes
        /// </summary>
        private void MostrarExpedientes()
        {
            CambiarVista?.Invoke("Expedientes", null);
            Titulo = "Gestión de Expedientes";
        }

        /// <summary>
        /// Cambia a la vista de actuaciones
        /// </summary>
        private void MostrarActuaciones()
        {
            CambiarVista?.Invoke("Actuaciones", null);
            Titulo = "Gestión de Actuaciones";
        }

        /// <summary>
        /// Cambia a la vista de informes
        /// </summary>
        private void MostrarInformes()
        {
            CambiarVista?.Invoke("Informes", null);
            Titulo = "Generación de Informes";
        }

        /// <summary>
        /// Solicita cerrar la aplicación
        /// </summary>
        private void Salir()
        {
            SolicitarCerrar?.Invoke();
        }

        /// <summary>
        /// Acción para comunicar con la Vista y cambiar de vista activa
        /// </summary>
        public Action<string, string> CambiarVista { get; set; }
        
        /// <summary>
        /// Acción para comunicar con la Vista y solicitar el cierre de la aplicación
        /// </summary>
        public Action SolicitarCerrar { get; set; }

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
