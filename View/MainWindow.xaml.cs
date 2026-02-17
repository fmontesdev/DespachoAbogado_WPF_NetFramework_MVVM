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
using View.Views;

namespace View
{
    /// <summary>
    /// Code-behind para MainWindow.xaml.
    /// Ventana principal de la aplicación que gestiona la navegación entre vistas
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructor que inicializa la ventana principal y configura el ViewModel
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Crear ViewModel
            var viewModel = new MainViewModel();

            // Asignar Actions (más simple que eventos)
            viewModel.CambiarVista = CambiarVista;
            viewModel.SolicitarCerrar = () => Application.Current.Shutdown();

            // Asignar DataContext
            this.DataContext = viewModel;

            // Inicializar vista por defecto
            viewModel.InicializarVistaInicial();
        }

        /// <summary>
        /// Maneja el cambio de vista en el panel de contenido principal
        /// </summary>
        /// <param name="nombreVista">Nombre de la vista a mostrar (Citas, Clientes, Expedientes, Actuaciones, Informes)</param>
        /// <param name="parametroFiltro">Parámetro opcional para filtrar (nombre cliente para expedientes, código expediente para actuaciones)</param>
        public void CambiarVista(string nombreVista, string parametroFiltro = null)
        {
            switch (nombreVista)
            {
                case "Citas":
                    MainContent.Content = new CitasView();
                    rbCitas.IsChecked = true;
                    break;

                case "Clientes":
                    MainContent.Content = new ClientesView();
                    rbClientes.IsChecked = true;
                    break;

                case "Expedientes":
                    var expedientesView = new ExpedientesView();
                    MainContent.Content = expedientesView;
                    rbExpedientes.IsChecked = true;
                    
                    // Si se pasa el nombre del cliente como parámetro, aplicar filtro
                    if (parametroFiltro != null)
                    {
                        var viewModel = expedientesView.DataContext as ExpedienteViewModel;
                        viewModel?.FiltrarPorCliente(parametroFiltro);
                    }
                    break;

                case "Actuaciones":
                    var actuacionesView = new ActuacionesView();
                    MainContent.Content = actuacionesView;
                    rbActuaciones.IsChecked = true;
                    
                    // Si se pasa el código del expediente como parámetro, aplicar filtro
                    if (parametroFiltro != null)
                    {
                        var viewModel = actuacionesView.DataContext as ActuacionViewModel;
                        viewModel?.FiltrarPorExpediente(parametroFiltro);
                    }
                    break;

                case "Informes":
                    MainContent.Content = new InformesView();
                    rbInformes.IsChecked = true;
                    break;
            }
        }
    }
}

