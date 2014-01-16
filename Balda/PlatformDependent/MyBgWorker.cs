using System;
using Balda.PlatformDependent;

namespace Balda.Logic
{
    public class BgWorkerEventArgs : EventArgs
    {
        public object Result;
        public object Argument;
        public MyBgWorker worker;
    }
    public class MyBgWorker
    {
        public MyBgWorker()
        {
        }

        public delegate void WorkerHandler(object sender,BgWorkerEventArgs arg);
        public delegate void CompleteHandler(object sender, BgWorkerEventArgs arg);

        public WorkerHandler work;
        public event CompleteHandler onComplete;

        public void Run(object param)
        {
            BgWorkerEventArgs args = new BgWorkerEventArgs();
            args.worker = this;
            args.Argument = param;
            IsBusy = true;
#if SILVERLIGHT
            System.Threading.ThreadPool.QueueUserWorkItem(DoWork,args);
#else
            Windows.System.Threading.ThreadPool.RunAsync((operation) => { DoWork(args); });
                //(DoWork, args);
#endif
        }

        public volatile bool IsBusy = false;

        private static void DoWork(object stateInfo)
        {
            BgWorkerEventArgs arg = stateInfo as BgWorkerEventArgs;
            if (arg == null) return;
            if (arg.worker.work != null)
            {
                arg.worker.work(arg.worker,arg);
            }
            if (arg.worker.onComplete != null)
            {
                UiThreadRunner.runOnUiThread(() =>
                    {
                        arg.worker.onComplete(arg.worker, arg);
                    });
            }
            arg.worker.IsBusy = false;
        }
    }
}
