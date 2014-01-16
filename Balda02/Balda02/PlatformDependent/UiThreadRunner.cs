using System;
using Windows.UI.Core;

namespace Balda.PlatformDependent
{
    public class UiThreadRunner
    {
        public static void runOnUiThread(Action action)
        {
#if SILVERLIGHT
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(action);
#else
            //metro
            Balda02.App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>{action();});
#endif

        }
    }
}
