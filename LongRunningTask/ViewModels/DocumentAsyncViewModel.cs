using System.Collections.ObjectModel;
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

        public bool Indeterminant { get; private set; }

        public DocumentAsyncViewModel(IDocumentService documentService)
        {
            _documentService = documentService;
            DisplayName = "Async Example";

            Indeterminant = true;
            NumParagraphs = 10;
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

        public async void DownloadDocument()
        {
            IsBusy = true;
            Document = new ObservableCollection<Paragraph>(await _documentService.GetDocumentAsync(NumParagraphs, 0));
            IsBusy = false;
        }

        public bool CanDownloadDocument
        {
            get { return !IsBusy; }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("CanDownloadDocument");
            }
        }
    }
}