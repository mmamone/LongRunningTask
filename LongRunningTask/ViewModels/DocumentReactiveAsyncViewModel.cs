using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Caliburn.Micro.ReactiveUI;
using DocumentServices;
using ReactiveUI;

namespace LongRunningTask.ViewModels
{
    public class DocumentReactiveAsyncViewModel : ReactiveScreen
    {
        private readonly IDocumentService _documentService;
        
        private ReactiveCommand<IList<Paragraph>> _downloadDocument;
        private ReactiveList<Paragraph> _document = new ReactiveList<Paragraph>();

        private ObservableAsPropertyHelper<bool> _isBusy;
        public bool IsBusy
        {
            get { return _isBusy.Value; }
        }

        public int Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }

        public ReactiveCommand<IList<Paragraph>> DownloadDocument
        {
            get { return _downloadDocument; }
            set { _downloadDocument = value; }
        }

        public ReactiveCommand<object> Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

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

        public CompletionState CompletionState
        {
            get { return _completionState; }
            set { this.RaiseAndSetIfChanged(ref _completionState, value); }
        }

        private ObservableAsPropertyHelper<bool> _indeterminant;
        private Progress<int> _progressReporter;
        private int _progress;
        private IDisposable _subscription;
        private ReactiveCommand<object> _cancel;
        public bool Indeterminant { get { return _indeterminant.Value; } }

        public DocumentReactiveAsyncViewModel(IDocumentService documentService)
        {
            DisplayName = "Reactive Async Example";

            _documentService = documentService;

            _progressReporter = new Progress<int>(i => Progress = i);

            NumParagraphs = 10;

            DownloadDocument = ReactiveCommand.CreateAsyncTask((o, ct) => _documentService.GetDocumentAsync(NumParagraphs, 0, _progressReporter, ct));
            _subscription = DownloadDocument.SubscribeOn(RxApp.TaskpoolScheduler).Subscribe(x =>
            {
                using (Document.SuppressChangeNotifications())
                {
                    Document.AddRange(x);
                }
            }, DownloadFailed);

            DownloadDocument
                .ThrownExceptions
                .Subscribe(DownloadFailed);

            DownloadDocument
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, out _isBusy);

            this.WhenAny(x => x.CompletionState, x => x.Progress, (c, p) => c.Value == CompletionState.None && p.Value == 0)
                .Merge(DownloadDocument.IsExecuting)
                .ToProperty(this, x => x.Indeterminant, out _indeterminant);

            //TODO: cancel - create async task to cancel that will wait until DownloadDocument is finished ...
            Cancel = ReactiveCommand.Create(DownloadDocument.IsExecuting);
            Cancel.Subscribe(x =>
            {
                _subscription.Dispose();
                CompletionState = CompletionState.Fail;
            });
        }

        private void DownloadFailed(Exception ex)
        {
            CompletionState = CompletionState.Fail;
        }
    }
}