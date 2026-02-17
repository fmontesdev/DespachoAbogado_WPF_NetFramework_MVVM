using System.Windows.Controls;
using ViewModel;

namespace View.Views
{
    /// <summary>
    /// Code-behind para InformesView.xaml.
    /// Vista para la generacion y visualizacion de informes del despacho de abogados
    /// </summary>
    public partial class InformesView : UserControl
    {
        /// <summary>
        /// Constructor que inicializa la vista y configura el ViewModel.
        /// El ViewModel maneja toda la logica de generacion de informes
        /// </summary>
        public InformesView()
        {
            InitializeComponent();

            // Crear ViewModel y asignar DataContext
            // El ViewModel maneja toda la logica (incluida la apertura de ventanas)
            this.DataContext = new InformeViewModel();
        }
    }
}


