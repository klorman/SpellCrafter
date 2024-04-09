using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SpellCrafter
{
    public class RangedObservableCollection<T> : ObservableCollection<T>
    {
        bool _suppressNotification = false;

        public RangedObservableCollection() { }

        public RangedObservableCollection(IEnumerable<T> list) : base(list) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            DisableOnCollectionChanged();
            foreach (var item in list)
            {
                Add(item);
            }
            EnableOnCollectionChanged();
        }

        public void RemoveRange(IEnumerable<T> list)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            DisableOnCollectionChanged();
            foreach (var item in list)
            {
                Remove(item);
            }
            EnableOnCollectionChanged();
        }

        public void DisableOnCollectionChanged()
        {
            _suppressNotification = true;
        }

        public void EnableOnCollectionChanged()
        {
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
