using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Model;
using Model.Mappers;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la ventana de creación de nueva actuación
    /// </summary>
    public class NuevaActuacionViewModel : INotifyPropertyChanged
    {
        private readonly ActuacionService _actuacionService;
        private readonly ExpedienteService _expedienteService;
        private Expediente _expedienteSeleccionado;
        private string _tipo;
        private string _descripcion;
        private string _errorMessage;
        private string _buscarExpediente;

        /// <summary>
        /// Lista de expedientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Expediente> Expedientes { get; set; }

        /// <summary>
        /// Lista completa de expedientes (para filtrado)
        /// </summary>
        private ObservableCollection<Expediente> _todosLosExpedientes;

        /// <summary>
        /// Expediente seleccionado
        /// </summary>
        public Expediente ExpedienteSeleccionado
        {
            get => _expedienteSeleccionado;
            set
            {
                _expedienteSeleccionado = value;
                OnPropertyChanged(nameof(ExpedienteSeleccionado));
            }
        }

        /// <summary>
        /// Texto de búsqueda del expediente en el ComboBox
        /// </summary>
        public string BuscarExpediente
        {
            get => _buscarExpediente;
            set
            {
                _buscarExpediente = value;
                OnPropertyChanged(nameof(BuscarExpediente));
                FiltrarExpedientes();
            }
        }

        /// <summary>
        /// Tipo de actuación
        /// </summary>
        public string Tipo
        {
            get => _tipo;
            set
            {
                _tipo = value;
                OnPropertyChanged(nameof(Tipo));
            }
        }

        /// <summary>
        /// Descripción de la actuación
        /// </summary>
        public string Descripcion
        {
            get => _descripcion;
            set
            {
                _descripcion = value;
                OnPropertyChanged(nameof(Descripcion));
            }
        }

        /// <summary>
        /// Mensaje de error para mostrar al usuario
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>
        /// Indica si hay un error activo
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Diccionario de tipos de actuación disponibles
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> TiposDisponibles { get; }

        /// <summary>
        /// Comando para crear la actuación
        /// </summary>
        public ICommand CrearCommand { get; }

        /// <summary>
        /// Comando para cancelar
        /// </summary>
        public ICommand CancelarCommand { get; }

        /// <summary>
        /// Acción para cerrar la ventana con éxito (DialogResult = true)
        /// </summary>
        public Action CerrarVentanaExito { get; set; }

        /// <summary>
        /// Acción para cerrar la ventana cancelando (DialogResult = false)
        /// </summary>
        public Action CerrarVentanaCancelar { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NuevaActuacionViewModel()
        {
            _actuacionService = new ActuacionService();
            _expedienteService = new ExpedienteService();
            Expedientes = new ObservableCollection<Expediente>();
            _todosLosExpedientes = new ObservableCollection<Expediente>();

            TiposDisponibles = TipoActuacionMapper.ObtenerTodos();

            // Seleccionar el primer tipo por defecto (llamada)
            if (TiposDisponibles.Count > 0)
            {
                Tipo = TiposDisponibles.First().Key;
            }

            CrearCommand = new RelayCommand(CrearActuacion);
            CancelarCommand = new RelayCommand(Cancelar);

            CargarExpedientesAsync();
        }

        /// <summary>
        /// Carga la lista de expedientes desde la base de datos
        /// </summary>
        private async void CargarExpedientesAsync()
        {
            try
            {
                var expedientes = await _expedienteService.ObtenerExpedientesAsync();
                
                Expedientes.Clear();
                _todosLosExpedientes.Clear();
                
                // Filtrar expedientes que NO estén archivados ni cerrados
                foreach (var expediente in expedientes.Where(e => e.Estado != "archivado" && e.Estado != "cerrado"))
                {
                    Expedientes.Add(expediente);
                    _todosLosExpedientes.Add(expediente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar expedientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Filtra la lista de expedientes según el texto de búsqueda
        /// Permite buscar por: código de expediente, nombre completo de cliente, y DNI del cliente
        /// </summary>
        private void FiltrarExpedientes()
        {
            if (string.IsNullOrWhiteSpace(BuscarExpediente))
            {
                Expedientes.Clear();
                foreach (var expediente in _todosLosExpedientes)
                {
                    Expedientes.Add(expediente);
                }
            }
            else
            {
                string busqueda = BuscarExpediente.Trim().ToUpper();
                var expedientesFiltrados = _todosLosExpedientes
                    .Where(e => 
                        (e.Codigo != null && e.Codigo.ToUpper().Contains(busqueda)) ||
                        (e.Cliente != null && e.Cliente.NombreCompleto != null && e.Cliente.NombreCompleto.ToUpper().Contains(busqueda)) ||
                        (e.Cliente != null && e.Cliente.Dni != null && e.Cliente.Dni.ToUpper().Contains(busqueda)))
                    .ToList();

                Expedientes.Clear();
                foreach (var expediente in expedientesFiltrados)
                {
                    Expedientes.Add(expediente);
                }
            }
        }

        /// <summary>
        /// Crea la nueva actuación
        /// </summary>
        private async void CrearActuacion()
        {
            if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    var nuevaActuacion = new Actuacion
                    {
                        IdExpediente = ExpedienteSeleccionado.IdExpediente,
                        Tipo = Tipo,
                        Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                        FechaHora = DateTime.Now,
                        Estado = "pendiente"
                    };

                    await _actuacionService.CrearActuacionAsync(nuevaActuacion);

                    CerrarVentanaExito?.Invoke();
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al crear actuación: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Valida el formulario antes de crear la actuación
        /// </summary>
        private bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = ActuacionValidador.ValidarNuevaActuacion(
                ExpedienteSeleccionado,
                Tipo,
                Descripcion,
                out mensajeError
            );

            if (!esValido)
            {
                ErrorMessage = mensajeError;
            }

            return esValido;
        }

        /// <summary>
        /// Cancela y cierra la ventana
        /// </summary>
        private void Cancelar()
        {
            CerrarVentanaCancelar?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
