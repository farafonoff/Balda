using System;


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
#endif

        }
    }
}
