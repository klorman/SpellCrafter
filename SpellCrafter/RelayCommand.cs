using System;
using System.Windows.Input;

namespace SpellCrafter
{
    public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        : ICommand
    {
        private readonly Action<object?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
