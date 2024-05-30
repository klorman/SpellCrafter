using SpellCrafter.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpellCrafter
{
    public class ShowDialogEventFactory
    {
        private readonly Dictionary<Type, Delegate> _events = new();

        public Func<T, Task<T?>>? GetEvent<T>() where T : ViewModelBase
        {
            var eventType = typeof(T);

            if (_events.TryGetValue(eventType, out var eventDelegate) && eventDelegate is Func<T, Task<T?>> func)
            {
                return func;
            }

            Debug.WriteLine($"No method is bound to the event for {typeof(T).Name}");
            return null;
        }

        public void SetEvent<T>(Func<T, Task<T?>> func) where T : ViewModelBase
        {
            var eventType = typeof(T);

            _events[eventType] = func;
        }

        public async Task<T?> RaiseEvent<T>(T viewModel) where T : ViewModelBase
        {
            var targetType = typeof(T);

            if (_events.TryGetValue(targetType, out var eventDelegate) && eventDelegate is Func<T, Task<T>> func)
            {
                return await func.Invoke(viewModel);
            }

            return null;
        }
    }
}
