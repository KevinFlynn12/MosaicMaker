﻿using System;
using System.Windows.Input;

namespace ImageSandbox.Util
{
    /// <summary>
    /// This relays a command
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommand : ICommand
    {
        #region Data members

        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <param name="canExecute">The can execute.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     WeatherData used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            var result = this.canExecute?.Invoke(parameter) ?? true;
            return result;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     WeatherData used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this.execute(parameter);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        /// <returns>An event</returns>
        public event EventHandler CanExecuteChanged;

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