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
    public class DocumentsReactiveViewModel : ReactiveScreen
    {
        private IDocumentService _documentService;
       
        public ReactiveCommand<object> DownloadDocuments { get; set; }

        private int _numDocuments = 0;
        
        public int NumDocuments
        {
            get { return _numDocuments; }
            set { this.RaiseAndSetIfChanged(ref _numDocuments, value); }
        }

        private ReactiveList<DocumentProgress> _documents;
        public ReactiveList<DocumentProgress> Documents
        {
            get { return _documents; }
            set { this.RaiseAndSetIfChanged(ref _documents, value); }
        }

        public DocumentsReactiveViewModel(IDocumentService documentService)
        {
            _documentService = documentService;
            Documents = new ReactiveList<DocumentProgress>();

            DownloadDocuments = ReactiveCommand.Create();
            DownloadDocuments.Subscribe(_ =>
            {
                using (Documents.SuppressChangeNotifications())
                {
                    for (int i = 0; i < NumDocuments; i++)
                    {
                        var doc = new DocumentProgress();
                        doc.Do(_documentService.UploadDoc());
                        Documents.Add(doc);
                    }
                }

            });
        }
    }

    public class DocumentProgress : ReactiveObject
    {
        public DocumentProgress()
        {
            Cancel = ReactiveCommand.Create();
            Cancel.Subscribe(_ => {
                _cancel.Dispose();
                _cancel = null;
            });

            this.WhenAnyValue(x => x.Progress, (p) => p == 0).ToProperty(this, x => x.Indeterminant, out _indeterminant);
            this.WhenAnyValue(x => x.Progress, (p) => p == 100).ToProperty(this, x => x.IsDone, out _isDone);
        }

        private ObservableAsPropertyHelper<bool> _isDone;
        public bool IsDone { get { return _isDone.Value; } }

        //private ObservableAsPropertyHelper<int> _progress;
        //public int Progress { get { return _progress.Value; } }

        private int _progress;
        public int Progress { get { return _progress; } set { this.RaiseAndSetIfChanged(ref _progress, value); } }

        private ObservableAsPropertyHelper<bool> _indeterminant;
        private IDisposable _cancel;

        public bool Indeterminant { get { return _indeterminant.Value; } }

        public ReactiveCommand<object> Cancel { get; private set; }

        public void Do(IObservable<int> observable)
        {
            _cancel = observable
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Progress = x, ex =>
                {
                    Console.WriteLine(ex.Message);
                },
                () => { });
        }
    }
}