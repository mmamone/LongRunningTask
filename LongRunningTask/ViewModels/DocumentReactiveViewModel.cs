﻿using System;
using System.Reactive.Linq;
using Caliburn.Micro.ReactiveUI;
using DocumentServices;
using ReactiveUI;

namespace LongRunningTask.ViewModels
{
    public class DocumentReactiveViewModel : ReactiveScreen
    {
        private readonly IDocumentService _documentService;
        private IObservable<Paragraph> _takeNum;

        #region PropertyHelpers
        private ObservableAsPropertyHelper<bool> _isBusy;
        public bool IsBusy {get { return _isBusy.Value; }}

        private ObservableAsPropertyHelper<int> _progress;
        public int Progress{get { return _progress.Value; }}

        private ObservableAsPropertyHelper<bool> _indeterminant;
        public bool Indeterminant { get { return _indeterminant.Value; } }
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

        #endregion

        public DocumentReactiveViewModel(IDocumentService documentService)
        {
            _documentService = documentService;

            DisplayName = "Reactive Example";

            NumParagraphs = 10;

            DownloadDocument = ReactiveCommand
                .CreateAsyncObservable(_ => _documentService.GetDocumentEnumerable(NumParagraphs, 0)
                                                            .ToObservable(RxApp.TaskpoolScheduler));

            Cancel = ReactiveCommand.Create(DownloadDocument.IsExecuting);

            _takeNum = DownloadDocument.TakeUntil(Cancel);
                //.ObserveOn(RxApp.MainThreadScheduler)
                //.Catch(Observable.Return(new Paragraph { Content = "Paragraph missing" }))
                //.Catch(Observable.Empty<Paragraph>())
                //.Catch<Paragraph, Exception>(ex => Observable.Return(new Paragraph { Content = "Paragraph missing" }))
                //.ObserveOn(RxApp.MainThreadScheduler)
                //.Take(NumParagraphs);
                ;

            _subscription = _takeNum
                .Subscribe(x => Document.Add(x), DownloadDone);

            _takeNum
                .Scan(0, (acc, x) => IncrementPercentage(acc))
                .ToProperty(this, x => x.Progress, out _progress);

            DownloadDocument
                .ThrownExceptions
                .Subscribe(DownloadFailed);

            DownloadDocument
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, out _isBusy);

            this.WhenAny(x => x.CompletionState, x => x.Progress, (c, p) => c.Value == CompletionState.None && p.Value == 0)
                .Merge(DownloadDocument.IsExecuting)
                .ToProperty(this, x => x.Indeterminant, out _indeterminant);
        }

        private int IncrementPercentage(int acc)
        {
            return acc + (int)(100.0/NumParagraphs);
        }

        private void DownloadDone()
        {
            CompletionState = CompletionState.Success;
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
        Fail
    }
}