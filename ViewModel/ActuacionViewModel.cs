using Model;
using Model.Mappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal de gestión de actuaciones (ActuacionesView).
    /// Maneja la lista de actuaciones, selección y operaciones CRUD
    /// </summary>
    public class ActuacionViewModel : INotifyPropertyChanged
    {
        private readonly ActuacionService _actuacionService;
        private readonly ExpedienteService _expedienteService;
        private Actuacion _actuacionSeleccionada;
        private string _codigoExpediente;
        private string _nombreCompletoCliente;
        private string _tipo;
        private string _descripcion;
        private string _estado;
        private string _errorMessage;
        private string _buscar;
        private List<Actuacion> _todasLasActuaciones;
        private bool _inicializado = false;
        private string _filtroExpedientePendiente = null;

        /// <summary>
        /// Colección observable de actuaciones para el DataGrid.
        /// Se actualiza automáticamente en la interfaz de usuario
        /// </summary>
        public ObservableCollection<Actuacion> Actuaciones { get; set; }

        /// <summary>
        /// Lista de expedientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Expediente> Expedientes { get; set; }

        /// <summary>
        /// Expediente seleccionado (para crear nueva actuación)
        /// </summary>
        public Expediente ExpedienteSeleccionado { get; set; }

        /// <summary>
        /// Actuación seleccionada en el DataGrid.
        /// Al seleccionar una actuación, se cargan sus datos en los campos
        /// </summary>
        public Actuacion ActuacionSeleccionada
        {
            get => _actuacionSeleccionada;
            set
            {
                _actuacionSeleccionada = value;
                OnPropertyChanged(nameof(ActuacionSeleccionada));

                // Cargar datos en los campos cuando se selecciona una actuación
                if (_actuacionSeleccionada != null)
                {
                    CodigoExpediente = _actuacionSeleccionada.Expediente?.Codigo ?? string.Empty;
                    NombreCompletoCliente = _actuacionSeleccionada.Expediente?.Cliente?.NombreCompleto ?? string.Empty;
                    Tipo = _actuacionSeleccionada.Tipo;
                    Descripcion = _actuacionSeleccionada.Descripcion;
                    Estado = _actuacionSeleccionada.Estado;
                    ExpedienteSeleccionado = _actuacionSeleccionada.Expediente;
                }
                else
                {
                    LimpiarFormulario();
                }
            }
        }

        /// <summary>
        /// Código del expediente de la actuación seleccionada
        /// </summary>
        public string CodigoExpediente
        {
            get => _codigoExpediente;
            set
            {
                _codigoExpediente = value;
                OnPropertyChanged(nameof(CodigoExpediente));
            }
        }

        /// <summary>
        /// Nombre completo del cliente del expediente de la actuación seleccionada
        /// </summary>
        public string NombreCompletoCliente
        {
            get => _nombreCompletoCliente;
            set
            {
                _nombreCompletoCliente = value;
                OnPropertyChanged(nameof(NombreCompletoCliente));
            }
        }

        /// <summary>
        /// Tipo de actuación enlazado al ComboBox de edición
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
        /// Descripción de la actuación enlazada al TextBox de edición
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
        /// Estado de la actuación enlazado al ComboBox de edición
        /// </summary>
        public string Estado
        {
            get => _estado;
            set
            {
                _estado = value;
                OnPropertyChanged(nameof(Estado));
            }
        }

        /// <summary>
        /// Criterio de búsqueda global para filtrar actuaciones
        /// </summary>
        public string Buscar
        {
            get => _buscar;
            set
            {
                _buscar = value;
                OnPropertyChanged(nameof(Buscar));
                AplicarFiltros();
            }
        }

        /// <summary>
        /// Total de actuaciones registradas en la colección
        /// </summary>
        public int TotalActuaciones => Actuaciones?.Count ?? 0;

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
        /// Indica si hay un error activo para mostrar en la interfaz
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Diccionario de tipos de actuación disponibles para el ComboBox. Convierte enums de BD a texto amigable para la UI
        /// </summary>
        public Dictionary<string, string> TiposDisponibles { get; }

        /// <summary>
        /// Diccionario de estados disponibles para el ComboBox. Convierte enums de BD a texto amigable para la UI
        /// </summary>
        public Dictionary<string, string> EstadosDisponibles { get; }

        /// <summary>
        /// Comando para crear una nueva actuación
        /// </summary>
        public ICommand NuevoCommand { get; }

        /// <summary>
        /// Comando para editar la actuación seleccionada
        /// </summary>
        public ICommand EditarCommand { get; }

        /// <summary>
        /// Comando para eliminar la actuación seleccionada
        /// </summary>
        public ICommand EliminarCommand { get; }

        /// <summary>
        /// Comando para limpiar los filtros de búsqueda
        /// </summary>
        public ICommand LimpiarBusquedaCommand { get; }

        /// <summary>
        /// Constructor que inicializa los servicios, comandos y carga los datos iniciales
        /// </summary>
        public ActuacionViewModel()
        {
            _actuacionService = new ActuacionService();
            _expedienteService = new ExpedienteService();
            Actuaciones = new ObservableCollection<Actuacion>();
            Expedientes = new ObservableCollection<Expediente>();
            _todasLasActuaciones = new List<Actuacion>();

            // Inicializar diccionarios para ComboBox
            TiposDisponibles = TipoActuacionMapper.ObtenerTodos();
            EstadosDisponibles = EstadoActuacionMapper.ObtenerTodos();

            // Inicializar Commands
            NuevoCommand = new RelayCommand(NuevaActuacion);
            EditarCommand = new RelayCommand(EditarActuacion);
            EliminarCommand = new RelayCommand(EliminarActuacion);
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);

            // Cargar actuaciones y expedientes al inicializar
            InicializarAsync();
        }

        /// <summary>
        /// Inicializa la carga de datos de forma asíncrona y selecciona la primera actuación
        /// </summary>
        public async void InicializarAsync()
        {
            // Evitar recargas innecesarias si ya se ha inicializado antes
            // Lo implementamos para que el método FiltrarPorExpediente no se ejecute antes de que se hayan cargado los datos
            if (!_inicializado)
            {
                await CargarActuacionesAsync();
                await CargarExpedientesAsync();
                _inicializado = true;
            }

            // Si hay un filtro pendiente, aplicarlo ahora
            if (!string.IsNullOrEmpty(_filtroExpedientePendiente))
            {
                Buscar = _filtroExpedientePendiente;
                _filtroExpedientePendiente = null;
            }
            else
            {
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Carga la lista de actuaciones desde la base de datos y actualiza la colección observable
        /// </summary>
        private async Task CargarActuacionesAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                var actuaciones = await _actuacionService.ObtenerActuacionesAsync();
                _todasLasActuaciones = actuaciones;

                Actuaciones.Clear();
                foreach (var actuacion in actuaciones)
                {
                    Actuaciones.Add(actuacion);
                }

                OnPropertyChanged(nameof(TotalActuaciones));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar actuaciones: {ex.Message}";
            }
        }

        /// <summary>
        /// Carga la lista de expedientes desde la base de datos
        /// </summary>
        private async Task CargarExpedientesAsync()
        {
            try
            {
                var expedientes = await _expedienteService.ObtenerExpedientesAsync();

                Expedientes.Clear();
                foreach (var expediente in expedientes)
                {
                    Expedientes.Add(expediente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar expedientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Selecciona la primera actuación de la lista
        /// </summary>
        private void SeleccionarPrimero()
        {
            if (Actuaciones.Count > 0)
            {
                ActuacionSeleccionada = Actuaciones[0];
            }
            else
            {
                ActuacionSeleccionada = null;
            }
        }

        /// <summary>
        /// Mantiene la selección de la actuación en el índice especificado.
        /// Si el índice es inválido, selecciona el primero
        /// </summary>
        private void MantieneSeleccion(int indice)
        {
            if (Actuaciones.Count == 0)
            {
                ActuacionSeleccionada = null;
            }
            else if (indice >= 0 && indice < Actuaciones.Count)
            {
                ActuacionSeleccionada = Actuaciones[indice];
            }
            else
            {
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Selecciona la actuación anterior al índice especificado al eliminar
        /// </summary>
        private void SeleccionaAnterior(int indiceEliminado)
        {
            if (Actuaciones.Count == 0)
            {
                ActuacionSeleccionada = null;
            }
            else if (indiceEliminado == 0)
            {
                ActuacionSeleccionada = Actuaciones[0];
            }
            else
            {
                int nuevoIndice = indiceEliminado - 1;
                if (nuevoIndice >= Actuaciones.Count)
                {
                    nuevoIndice = Actuaciones.Count - 1;
                }
                ActuacionSeleccionada = Actuaciones[nuevoIndice];
            }
        }

        /// <summary>
        /// Abre la ventana modal para crear una nueva actuación
        /// </summary>
        private void NuevaActuacion()
        {
            VentanaNuevaActuacion?.Invoke();
        }

        /// <summary>
        /// Edita la actuación seleccionada con los datos de los campos,
        /// validando los datos y guardando los cambios en la base de datos
        /// </summary>
        private async void EditarActuacion()
        {
            string estadoExpediente = ActuacionSeleccionada?.Expediente?.Estado?.ToLower();

            if (ActuacionSeleccionada == null)
            {
                ErrorMessage = "No hay ninguna actuación seleccionada para editar";
            }
            // Validar que el expediente no esté archivado o cerrado
            else if (estadoExpediente == "archivado" || estadoExpediente == "cerrado")
            {
                ErrorMessage = "No se puede editar una actuación de un expediente archivado o cerrado";
            }
            else if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    int indiceActuacionActual = Actuaciones.IndexOf(ActuacionSeleccionada);

                    // Actualizar propiedades de la actuación seleccionada
                    ActuacionSeleccionada.Tipo = Tipo;
                    ActuacionSeleccionada.Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim();
                    ActuacionSeleccionada.Estado = Estado;

                    await _actuacionService.ActualizarActuacionAsync(ActuacionSeleccionada);

                    await CargarActuacionesAsync();
                    MantieneSeleccion(indiceActuacionActual);
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = $"{ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al editar actuación: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Solicita confirmación para eliminar la actuación seleccionada
        /// </summary>
        private void EliminarActuacion()
        {
            string estadoExpediente = ActuacionSeleccionada.Expediente?.Estado?.ToLower();

            if (ActuacionSeleccionada == null)
            {
                ErrorMessage = "No hay ninguna actuación seleccionada para eliminar";
            }
            // Validar que el expediente no esté archivado o cerrado
            else if (estadoExpediente == "archivado" || estadoExpediente == "cerrado")
            {
                ErrorMessage = "No se puede eliminar una actuación de un expediente archivado o cerrado";
            }
            else
            {
                ErrorMessage = string.Empty;
                ConfirmarEliminar?.Invoke((
                    ActuacionSeleccionada.IdActuacion,
                    ActuacionSeleccionada.FechaHora,
                    ActuacionSeleccionada.Expediente.Codigo
                ));
            }
        }

        /// <summary>
        /// Confirma y ejecuta la eliminación de la actuación de la base de datos
        /// </summary>
        public async void ConfirmarEliminarActuacion(int idActuacion)
        {
            try
            {
                if (ActuacionSeleccionada == null || ActuacionSeleccionada.IdActuacion != idActuacion)
                {
                    ErrorMessage = "La actuación seleccionada no coincide con la actuación a eliminar";
                }
                else
                {
                    int indiceActuacionEliminada = Actuaciones.IndexOf(ActuacionSeleccionada);

                    await _actuacionService.EliminarActuacionAsync(ActuacionSeleccionada);

                    await CargarActuacionesAsync();
                    SeleccionaAnterior(indiceActuacionEliminada);
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"{ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar actuación: {ex.Message}";
            }
        }

        /// <summary>
        /// Valida los datos del formulario de edición de actuaciones
        /// </summary>
        private bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = ActuacionValidador.ValidarActuacion(
                Tipo,
                Descripcion,
                Estado,
                out mensajeError
            );

            if (!esValido)
            {
                ErrorMessage = mensajeError;
            }

            return esValido;
        }

        /// <summary>
        /// Aplica el filtro de búsqueda global sobre la lista completa de actuaciones
        /// </summary>
        private void AplicarFiltros()
        {
            if (_todasLasActuaciones != null)
            {
                var actuacionesFiltradas = _todasLasActuaciones.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(Buscar))
                {
                    string busqueda = Buscar.Trim().ToUpper();
                    actuacionesFiltradas = actuacionesFiltradas
                        .Where(a =>
                            (a.Expediente != null && a.Expediente.Codigo != null && a.Expediente.Codigo.ToUpper().Contains(busqueda)) ||
                            (a.Expediente != null && a.Expediente.Cliente != null && a.Expediente.Cliente.Nombre != null && a.Expediente.Cliente.Nombre.ToUpper().Contains(busqueda)) ||
                            (a.Expediente != null && a.Expediente.Cliente != null && a.Expediente.Cliente.Apellidos != null && a.Expediente.Cliente.Apellidos.ToUpper().Contains(busqueda)) ||
                            (a.Expediente != null && a.Expediente.Cliente != null && a.Expediente.Cliente.NombreCompleto != null && a.Expediente.Cliente.NombreCompleto.ToUpper().Contains(busqueda)) ||
                            (a.Tipo != null && TipoActuacionMapper.DeBDaUI(a.Tipo).ToUpper().Contains(busqueda)) ||
                            (a.Estado != null && EstadoActuacionMapper.DeBDaUI(a.Estado).ToUpper().Contains(busqueda))
                        )
                        .ToList();
                }

                Actuaciones.Clear();
                foreach (var actuacion in actuacionesFiltradas)
                {
                    Actuaciones.Add(actuacion);
                }

                OnPropertyChanged(nameof(TotalActuaciones));
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Limpia el filtro de búsqueda y muestra todas las actuaciones
        /// </summary>
        private void LimpiarBusqueda()
        {
            Buscar = string.Empty;
        }

        /// <summary>
        /// Aplica un filtro para mostrar solo las actuaciones del expediente especificado
        /// </summary>
        /// <param name="codigoExpediente">Código del expediente por el cual filtrar</param>
        public void FiltrarPorExpediente(string codigoExpediente)
        {
            if (!string.IsNullOrEmpty(codigoExpediente))
            {
                if (_inicializado)
                {
                    // Ya está inicializado, aplicar filtro inmediatamente
                    Buscar = codigoExpediente;
                }
                else
                {
                    // Todavía no está inicializado, guardar para aplicar después
                    _filtroExpedientePendiente = codigoExpediente;
                }
            }
        }

        /// <summary>
        /// Limpia los campos del formulario de edición
        /// </summary>
        private void LimpiarFormulario()
        {
            CodigoExpediente = string.Empty;
            NombreCompletoCliente = string.Empty;
            Tipo = null;
            Descripcion = string.Empty;
            Estado = null;
            ExpedienteSeleccionado = null;
        }

        /// <summary>
        /// Acción para comunicar con la Vista y abrir la ventana de nueva actuación
        /// </summary>
        public Action VentanaNuevaActuacion { get; set; }

        /// <summary>
        /// Acción para comunicar con la Vista y solicitar confirmación de eliminación
        /// </summary>
        public Action<(int IdActuacion, DateTime FechaHora, string CodigoExpediente)> ConfirmarEliminar { get; set; }

        /// <summary>
        /// Evento que notifica cambios en las propiedades para actualizar la interfaz
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Método auxiliar para invocar el evento PropertyChanged
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
