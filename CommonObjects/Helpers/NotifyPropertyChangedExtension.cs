using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommonObjects.Helpers
{
    public static class NotifyPropertyChangedExtension
    {
        public static void MutateVerbose<T>(this INotifyPropertyChanged instance,
                                            ref T field,
                                            T new_value,
                                            Action<PropertyChangedEventArgs> raise,
                                            [CallerMemberName] String property_name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, new_value)) return;
            field = new_value;
            raise?.Invoke(new PropertyChangedEventArgs(property_name));
        }
    }
}
