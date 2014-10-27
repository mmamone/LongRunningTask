using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using ReactiveUI;

namespace LongRunningTask
{
    public class RxTest
    {
        public RxTest()
        {
            DownloadDocument = ReactiveCommand.CreateAsyncObservable(_ => GetItems().ToObservable());
            DownloadDocument.Subscribe(Console.WriteLine, () => Console.WriteLine("Done"));
        }

        public ReactiveCommand<int> DownloadDocument { get; set; }

        public void RunCommand()
        {
            DownloadDocument.Execute(new object());
        }

        public void RunCommand2()
        {
            IObservable<int> range = GetItems().ToObservable();
            range.ObserveOn(Scheduler.Default).Subscribe(Console.WriteLine, () => Console.WriteLine("Done 2"));
        }

        IEnumerable<int> GetItems()
        {
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(100);
                yield return i;
            }
        }
    }
}