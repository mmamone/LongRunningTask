using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLipsum.Core;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace DocumentServices
{
    public interface IDocumentService
    {
        IList<Paragraph> GetDocument(int numParagraphs, int latency);
        IEnumerable<Paragraph> GetDocumentEnumerable(int numParagraphs, int latency);
        Task<IList<Paragraph>> GetDocumentAsync(int numParagraphs, int latency, IProgress<int> progress, CancellationToken token);

        IObservable<Paragraph> GetDocumentObservable(int numParagraphs, int latency);

        IObservable<int> UploadDoc();
    }

    public class DocumentService: IDocumentService
    {
        private LipsumGenerator _li;
        private Random _rand;

        public DocumentService()
        {
            _li = new LipsumGenerator();
            _rand = new Random();
        }

        public IList<Paragraph> GetDocument(int numParagraphs, int latency)
        {
            var document = new List<Paragraph>();
            for (int i = 0; i < numParagraphs; i++)
            {
                Thread.Sleep(_rand.Next(100,1000) + latency);
                document.Add(new Paragraph {Content = string.Join(" ", _li.GenerateParagraphs(1))});
            }
            return document;
        }

        public IEnumerable<Paragraph> GetDocumentEnumerable(int numParagraphs, int latency)
        {
            for (int i = 0; i < numParagraphs; i++)
            {
                Thread.Sleep(_rand.Next(100, 1000) + latency);
                //if (_rand.Next(0,5) == 1) throw new Exception("Hello");
                Debug.WriteLine(string.Format("Printing item {0}", i));
                yield return new Paragraph { Content = string.Join(" ", _li.GenerateParagraphs(1)) };
            }
           // throw new Exception("Hello");
        }

        public async Task<IList<Paragraph>> GetDocumentAsync(int numParagraphs, int latency, IProgress<int> progress, CancellationToken token)
        {
            IList<Paragraph> document = new List<Paragraph>();
            for (int i = 0; i < numParagraphs; i++)
            {
                await Task.Delay(_rand.Next(100, 1000) + latency, token);
                document.Add(new Paragraph { Content = string.Join(" ", _li.GenerateParagraphs(1)) });
                progress.Report((int)((i/((double)numParagraphs - 1)) * 100));
            }
            return document;
        }

        public IObservable<Paragraph> GetDocumentObservable(int numParagraphs, int latency)
        {
            return Observable.Create<Paragraph>(obs =>
            {
                //var cancel = new CancellationDisposable ();
                bool stopEarly = false;
                Task.Run(() =>
                {
                    for (int i = 0; i < numParagraphs; i++)
                    {
                        Console.WriteLine("Service On thread {0}", Thread.CurrentThread.ManagedThreadId);
                        Thread.Sleep(1000);
                        if (stopEarly) return;
                        obs.OnNext(new Paragraph { Content = string.Join(" ", _li.GenerateParagraphs(1)) });
                    }
                    obs.OnCompleted();
                });
                return Disposable.Create(() =>
                {
                    stopEarly = true;
                });
            })
            //.Publish()
            //.RefCount()
            ;
        }

        public IObservable<int> UploadDoc()
        {
            return Observable.Create<int>(obs =>
            {
                var ct = new CancellationDisposable();
                Task.Run(() =>
                {
                    var rand = new Random();
                    for (int i = 0; i <= 100; i++)
                    {
                        Thread.Sleep(rand.Next(10, 100));
                        if (ct.Token.IsCancellationRequested)
                        {
                            obs.OnError(new Exception("Boo"));
                            return;
                        }
                        obs.OnNext(i);
                    }
                    obs.OnCompleted();
                });
                return ct;
            });
        }

        //public IObservable<Paragraph> GetDocumentObservable(int numParagraphs, int latency)
        //{
        //    var subject = new BehaviorSubject<Paragraph>(new Paragraph());
        //    Task.Run(() =>
        //    {
        //        try
        //        {
        //            for (int i = 0; i < numParagraphs; i++)
        //            {
        //                if (i == 5) throw new Exception("Hello");
        //                Console.WriteLine("Service On thread {0}", Thread.CurrentThread.ManagedThreadId);
        //                Thread.Sleep(1000);
        //                subject.OnNext(new Paragraph { Content = string.Join(" ", _li.GenerateParagraphs(1)) });
        //            }
        //            subject.OnCompleted();
        //        }
        //        catch (Exception e)
        //        {
        //            subject.OnError(e);
        //        }
        //    });
        //    return subject;
        //}

    }

    public class Paragraph : PropertyChangedBase
    {
        private string _content;

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
