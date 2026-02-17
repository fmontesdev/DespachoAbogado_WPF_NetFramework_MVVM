using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Model;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la ventana modal de creación de clientes.
    /// Implementa validación y gestión de errores para el formulario de nuevo cliente
    /// </summary>
    public class NuevoClienteViewModel : INotifyPropertyChanged
    {
        private readonly ClienteService _clienteService;
        private string _nombre;
        private string _apellidos;
        private string _dni;
        private string _telefono;
        private string _email;
        private string _poblacion;
        private string _direccion;
        private string _errorMessage;

        /// <summary>
        /// Nombre del cliente enlazado al TextBox
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        /// <summary>
        /// Apellidos del cliente enlazado al TextBox
        /// </summary>
        public string Apellidos
        {
            get => _apellidos;
            set
            {
                _apellidos = value;
                OnPropertyChanged(nameof(Apellidos));
            }
        }

        /// <summary>
        /// DNI del cliente enlazado al TextBox
        /// </summary>
        public string Dni
        {
            get => _dni;
            set
            {
                _dni = value;
                OnPropertyChanged(nameof(Dni));
            }
        }

        /// <summary>
        /// Teléfono del cliente enlazado al TextBox
        /// </summary>
        public string Telefono
        {
            get => _telefono;
            set
            {
                _telefono = value;
                OnPropertyChanged(nameof(Telefono));
            }
        }

        /// <summary>
        /// Email del cliente enlazado al TextBox
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        /// <summary>
        /// Población del cliente enlazado al TextBox
        /// </summary>
        public string Poblacion
        {
            get => _poblacion;
            set
            {
                _poblacion = value;
                OnPropertyChanged(nameof(Poblacion));
            }
        }

        /// <summary>
        /// Dirección del cliente enlazado al TextBox
        /// </summary>
        public string Direccion
        {
            get => _direccion;
            set
            {
                _direccion = value;
                OnPropertyChanged(nameof(Direccion));
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
        /// Indica si hay un error activo para mostrar en la interfaz
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Comando para crear el cliente y cerrar la ventana
        /// </summary>
        public ICommand CrearCommand { get; }

        /// <summary>
        /// Comando para cerrar la ventana sin guardar
        /// </summary>
        public ICommand CancelarCommand { get; }

        /// <summary>
        /// Constructor que inicializa el servicio y los comandos
        /// </summary>
        public NuevoClienteViewModel()
        {
            _clienteService = new ClienteService();

            // Inicializar Commands
            CrearCommand = new RelayCommand(CrearCliente);
            CancelarCommand = new RelayCommand(Cancelar);
        }

        /// <summary>
        /// Crea un nuevo cliente en la base de datos después de validar el formulario
        /// </summary>
        private async void CrearCliente()
        {
            // Valida el formulario y crea el cliente
            if (ValidarFormulario())
            {
                try
                {
                    // Limpiar mensaje de error previo
                    ErrorMessage = string.Empty;

                    // Crear nuevo cliente
                    var nuevoCliente = new Cliente
                    {
                        Nombre = Nombre.Trim(),
                        Apellidos = Apellidos.Trim(),
                        Dni = Dni.Trim(),
                        Telefono = string.IsNullOrWhiteSpace(Telefono) ? string.Empty : Telefono.Trim(),
                        Email = Email.Trim(),
                        Poblacion = Poblacion.Trim(),
                        Direccion = string.IsNullOrWhiteSpace(Direccion) ? string.Empty : Direccion.Trim()
                    };

                    await _clienteService.CrearClienteAsync(nuevoCliente);

                    // Notificar éxito y cerrar ventana
                    CerrarVentanaExito?.Invoke();
                }
                catch (ArgumentException ex)
                {
                    // Errores de validación del servicio
                    ErrorMessage = $"{ex.Message}";
                }
                catch (Exception ex)
                {
                    // Otros errores
                    ErrorMessage = $"Error al guardar el cliente: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Valida los campos del formulario de creación de cliente usando el validador
        /// </summary>
        /// <returns>True si todos los datos son válidos, false si hay errores</returns>
        public bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = ClienteValidador.ValidarCliente(
                Nombre,
                Apellidos,
                Dni,
                Telefono,
                Email,
                Poblacion,
                Direccion,
                out mensajeError
            );

            if (!esValido)
            {
                ErrorMessage = mensajeError;
            }

            return esValido;
        }

        /// <summary>
        /// Cancela la operación y cierra la ventana sin guardar
        /// </summary>
        private void Cancelar()
        {
            CerrarVentanaCancelar?.Invoke();
        }

        /// <summary>
        /// Acción para comunicar con la Vista y cerrar la ventana tras éxito
        /// </summary>
        public Action CerrarVentanaExito { get; set; }
        
        /// <summary>
        /// Acción para comunicar con la Vista y cerrar la ventana tras cancelar
        /// </summary>
        public Action CerrarVentanaCancelar { get; set; }

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
