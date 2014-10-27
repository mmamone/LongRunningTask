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

        private ObservableAsPropertyHelper<int> _progress;
        public int Progress
        {
            get { return _progress.Value; }
        }

        public ReactiveCommand<IList<Paragraph>> DownloadDocument
        {
            get { return _downloadDocument; }
            set { _downloadDocument = value; }
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
        public bool Indeterminant { get { return _indeterminant.Value; } }

        public DocumentReactiveAsyncViewModel(IDocumentService documentService)
        {
            DisplayName = "Reactive Async Example";

            _documentService = documentService;

            NumParagraphs = 10;

            DownloadDocument = ReactiveCommand.CreateAsyncTask((o, ct) => _documentService.GetDocumentAsync(NumParagraphs, 0));
            DownloadDocument.Subscribe(x =>
            {
                using (Document.SuppressChangeNotifications())
                {
                    Document.AddRange(x);
                }
            }, DownloadFailed, DownloadDone);

            DownloadDocument
                .ThrownExceptions
                .Subscribe(DownloadFailed);

            DownloadDocument
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, out _isBusy);

            DownloadDocument.IsExecuting
                .ToProperty(this, x => x.Indeterminant, out _indeterminant);

            //TODO: cancel - create async task to cancel that will wait until DownloadDocument is finished ...
            //Cancel = ReactiveCommand.Create(DownloadDocument.IsExecuting);
            //Cancel.Subscribe(x => { });
        }

        private void DownloadDone()
        {
            CompletionState = CompletionState.Success;
        }

        private void DownloadFailed(Exception ex)
        {
            CompletionState = CompletionState.Fail;
        }
    }
}