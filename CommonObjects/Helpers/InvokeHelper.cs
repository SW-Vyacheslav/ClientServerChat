using System;
using System.Windows;

namespace CommonObjects.Helpers
{
    public static class InvokeHelper
    {
        public static T ApplicationInvoke<T>(Func<T> callback)
        {
            return Application.Current.Dispatcher.Invoke(callback);
        }

        public static void ApplicationInvoke(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        public static void ApplicationInvoke(Delegate method, params object[] args)
        {
            Application.Current.Dispatcher.Invoke(method, args);
        }
    }
}
