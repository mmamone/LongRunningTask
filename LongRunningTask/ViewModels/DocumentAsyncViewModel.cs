using System;
using System.Collections.ObjectModel;
using System.Threading;
using Caliburn.Micro.ReactiveUI;
using DocumentServices;

namespace LongRunningTask.ViewModels
{
    public class DocumentAsyncViewModel : ReactiveScreen
    {
        private readonly IDocumentService _documentService;
        private ObservableCollection<Paragraph> _document = new ObservableCollection<Paragraph>();
        private bool _isBusy;
        private int _numParagraphs;
        private CompletionState _completionState;
        private CancellationTokenSource _cts;
        private int _progress;
        private string _errorMessage;
        private Progress<int> _progressReporter;

        public bool Indeterminant
        {
            get { return CompletionState == CompletionState.None && Progress == 0 && IsBusy == true; }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("Indeterminant");
            }
        }

        public ObservableCollection<Paragraph> Document
        {
            get { return _document; }
            set
            {
                _document = value;
                NotifyOfPropertyChange();
            }
        }

        public int NumParagraphs
        {
            get { return _numParagraphs; }
            set
            {
                _numParagraphs = value;
                NotifyOfPropertyChange();
            }
        }

        public CompletionState CompletionState
        {
            get { return _completionState; }
            set
            {
                _completionState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("Indeterminant");
            }
        }

        public bool CanDownloadDocument
        {
            get { return !IsBusy; }
        }

        public bool CanCancel
        {
            get { return IsBusy; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("CanDownloadDocument");
                NotifyOfPropertyChange("CanCancel");
                NotifyOfPropertyChange("Indeterminant");
            }
        }

        public DocumentAsyncViewModel(IDocumentService documentService)
        {
            _documentService = documentService;
            DisplayName = "Async Example";

            NumParagraphs = 10;

            _cts = new CancellationTokenSource();
            _progressReporter = new Progress<int>(i => Progress = i);
        }

        public async void DownloadDocument()
        {
            IsBusy = true;
            try
            {
                Document =
                    new ObservableCollection<Paragraph>(
                        await _documentService.GetDocumentAsync(NumParagraphs, 0, _progressReporter, _cts.Token));
                CompletionState = CompletionState.Success;
            }
            catch (Exception e)
            {
                CompletionState = CompletionState.Fail;
                
            }
            
            IsBusy = false;
        }

        public void Cancel()
        {
            CompletionState = CompletionState.Cancelling;
            _cts.Cancel();
        }
    }
}