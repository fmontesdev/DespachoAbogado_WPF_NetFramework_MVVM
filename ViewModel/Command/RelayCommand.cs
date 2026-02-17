using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModel.Command
{
    /// <summary>
    /// Implementación reutilizable de ICommand para enlazar acciones del ViewModel con la Vista.
    /// Permite ejecutar comandos con lógica personalizada y evaluar si el comando puede ejecutarse
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Constructor que inicializa el comando con una acción a ejecutar
        /// </summary>
        /// <param name="execute">Acción a ejecutar cuando el comando se invoca</param>
        /// <param name="canExecute">Función opcional que determina si el comando puede ejecutarse</param>
        /// <exception cref="ArgumentNullException">Si execute es nulo</exception>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determina si el comando puede ejecutarse en su estado actual
        /// </summary>
        /// <param name="parameter">Parámetro del comando (no utilizado en esta implementación)</param>
        /// <returns>True si el comando puede ejecutarse, false en caso contrario</returns>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// Ejecuta la acción asociada al comando
        /// </summary>
        /// <param name="parameter">Parámetro del comando (no utilizado en esta implementación)</param>
        public void Execute(object parameter) => _execute();

        /// <summary>
        /// Evento que WPF escucha para reevaluar el estado del comando (habilitado/deshabilitado)
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

