using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Reactive.Linq;
using Caliburn.Micro.ReactiveUI;
using DocumentServices;
using ReactiveUI;
using System.Threading;

namespace LongRunningTask.ViewModels
{
    public class DocumentReactiveViewModel : ReactiveScreen
    {
        private readonly IDocumentService _documentService;
        //private IObservable<Paragraph> _takeNum;

        #region PropertyHelpers
        private ObservableAsPropertyHelper<bool> _isBusy;
        public bool IsBusy {get { return _isBusy.Value; }}

        //private ObservableAsPropertyHelper<int> _progress;
        //public int Progress{get { return _progress.Value; }}

        //private ObservableAsPropertyHelper<bool> _indeterminant;
        //public bool Indeterminant { get { return _indeterminant.Value; } }
        #endregion

        #region Commands
        private ReactiveCommand<Paragraph> _downloadDocument;
        public ReactiveCommand<Paragraph> DownloadDocument
        {
            get { return _downloadDocument; }
            set { _downloadDocument = value; }
        }

        private ReactiveCommand<object> _cancel;
        public ReactiveCommand<object> Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
        #endregion

        #region Properties
        private ReactiveList<Paragraph> _document = new ReactiveList<Paragraph>();
        public ReactiveList<Paragraph> Document
        {
            get { return _document; }
            set { this.RaiseAndSetIfChanged(ref _document, value); }
        }

        private int _numParagraphs;
        public int NumParagraphs
        {
            get { return _numParagraphs; }
            set { this.RaiseAndSetIfChanged(ref _numParagraphs, value); }
        }

        private CompletionState _completionState;
        private string _errorMessage;
        private IDisposable _subscription;
        private bool _commandStarted;

        public CompletionState CompletionState
        {
            get { return _completionState; }
            set { this.RaiseAndSetIfChanged(ref _completionState, value); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { this.RaiseAndSetIfChanged(ref _errorMessage, value); }
        }

        public ReactiveCommand<object> Something { get; private set; }

        #endregion

        public DocumentReactiveViewModel(IDocumentService documentService)
        {
            _documentService = documentService;

            DisplayName = "Reactive Example";

            NumParagraphs = 10;

            Something = ReactiveCommand.Create();
            var disp = Something.Subscribe(x =>
            {
                _documentService.GetDocumentObservable(NumParagraphs, 0)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(p =>
                    {
                        Console.WriteLine("ViewModel OnNext thread {0}", Thread.CurrentThread.ManagedThreadId);
                        Document.Add(p);
                    },
                    ex => { },
                    () =>
                    {
                        Console.WriteLine("ViewModel OnComplete thread {0}", Thread.CurrentThread.ManagedThreadId);
                    });
            });

            DownloadDocument = ReactiveCommand
                .CreateAsyncObservable(_ =>
                {
                    Console.WriteLine("ViewModel Invoking On thread {0}", Thread.CurrentThread.ManagedThreadId);
                    return _documentService.GetDocumentObservable(NumParagraphs, 0);
                });

            DownloadDocument
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p =>
                {
                    Console.WriteLine("ViewModel OnNext thread {0}", Thread.CurrentThread.ManagedThreadId);
                    Document.Add(p);
                },
                x => { },
                () =>
                {
                    Console.WriteLine("ViewModel OnComplete thread {0}", Thread.CurrentThread.ManagedThreadId);
                });

            Cancel = ReactiveCommand.Create(DownloadDocument.IsExecuting);
            Cancel.Subscribe(_ => {CompletionState = CompletionState.Fail;});

            //This triggers OnComplete on the DownloadDcoument command before it is finished executing
            //_takeNum = DownloadDocument.TakeUntil(Cancel);

            //_subscription = _takeNum
            //    .Subscribe(Document.Add);

            //_takeNum
            //    .Scan(0, (acc, x) => IncrementPercentage(acc))
            //    .ToProperty(this, x => x.Progress, out _progress);

            DownloadDocument
                .ThrownExceptions
                .Subscribe(DownloadFailed);

            // On cancel, this remains true for a short while longer (after OnComplete is raised)
            DownloadDocument
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, out _isBusy);

            //this.WhenAny(x => x.CompletionState, x => x.Progress, (c, p) => c.Value == CompletionState.None && p.Value == 0)
            //    .Merge(DownloadDocument.IsExecuting)
            //    .ToProperty(this, x => x.Indeterminant, out _indeterminant);
        }

        private int IncrementPercentage(int acc)
        {
            return acc + (int)(100.0/NumParagraphs);
        }

        private void DownloadFailed(Exception ex)
        {
            CompletionState = CompletionState.Fail;
            ErrorMessage = ex.Message;
        }
    }

    public enum CompletionState
    {
        None,
        Success,
        Cancelling,
        Fail
    }
}