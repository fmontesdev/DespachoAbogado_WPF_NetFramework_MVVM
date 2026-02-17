using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel;
using View.Windows;

namespace View.Views
{
    /// <summary>
    /// Code-behind para ClientesView.xaml.
    /// Vista principal para la gestión de clientes del despacho de abogados
    /// </summary>
    public partial class ClientesView : UserControl
    {
        /// <summary>
        /// Constructor que inicializa la vista y configura el ViewModel
        /// </summary>
        public ClientesView()
        {
            InitializeComponent();

            // Crear e instanciar el ViewModel (variable local)
            var viewModel = new ClienteViewModel();

            // Asignar Actions (más simple que eventos)
            viewModel.VentanaNuevoCliente = VentanaNuevoCliente;
            viewModel.ConfirmarEliminar = ConfirmarEliminar;
            viewModel.IrAExpedientes = IrAExpedientesCliente;

            // Asignar DataContext
            this.DataContext = viewModel;
        }

        /// <summary>
        /// Abre la ventana modal para crear un nuevo cliente
        /// </summary>
        private void VentanaNuevoCliente()
        {
            var ventana = new NuevoClienteWindow();
            if (ventana.ShowDialog() == true)
            {
                // Recargar la lista de clientes después de crear uno nuevo
                ((ClienteViewModel)this.DataContext).InicializarAsync();
            }
        }

        /// <summary>
        /// Muestra un diálogo de confirmación antes de eliminar un cliente
        /// </summary>
        /// <param name="info">Tupla con el ID, nombre y apellidos del cliente a eliminar</param>
        private void ConfirmarEliminar((int IdCliente, string Nombre, string Apellidos) info)
        {
            var resultado = MessageBox.Show(
                $"¿Está seguro que desea eliminar al cliente '{info.Nombre} {info.Apellidos}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (resultado == MessageBoxResult.Yes)
            {
                // Confirmar eliminación en el ViewModel pasando solo el ID
                ((ClienteViewModel)this.DataContext).ConfirmarEliminarCliente(info.IdCliente);
            }
        }

        /// <summary>
        /// Controla la visibilidad del placeholder del campo de búsqueda
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            placeholderBuscar.Visibility = string.IsNullOrEmpty(txtBuscar.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Navega a la vista de Expedientes con el cliente seleccionado como filtro
        /// </summary>
        /// <param name="nombreCompleto">Nombre completo del cliente para filtrar expedientes</param>
        private void IrAExpedientesCliente(string nombreCompletoCliente)
        {
            if (!string.IsNullOrEmpty(nombreCompletoCliente))
            {
                // Obtener MainWindow y cambiar vista pasando el nombre del cliente como parámetro
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.CambiarVista("Expedientes", nombreCompletoCliente);
            }
        }
    }
}
