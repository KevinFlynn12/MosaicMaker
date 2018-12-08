using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageSandbox.Util
{
    public class RelayCommandAsync : ICommand
    {
        private readonly Func<object, Task> executedMethod;
        private readonly Predicate<object> canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommandAsync(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            this.executedMethod = execute;
            this.canExecute = canExecute;
        }
        

        public bool CanExecute(object parameter)
        {
            var result = this.canExecute?.Invoke(parameter) ?? true;
            return result;
        }

        public async  void Execute(object parameter)
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
    }
}
