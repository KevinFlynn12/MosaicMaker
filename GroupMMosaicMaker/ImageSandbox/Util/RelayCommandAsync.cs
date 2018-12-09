using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageSandbox.Util
{
    public class RelayCommandAsync : ICommand
    {
        #region Data members

        private readonly Func<object, Task> executedMethod;
        private readonly Predicate<object> canExecute;

        #endregion

        #region Constructors

        public RelayCommandAsync(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            this.executedMethod = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Methods

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var result = this.canExecute?.Invoke(parameter) ?? true;
            return result;
        }

        public async void Execute(object parameter)
        {
            await this.executedMethod(parameter);
        }

        /// <summary>
        ///     Called when [can execute changed].
        /// </summary>
        public virtual void OnCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}