using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLipsum.Core;

namespace DocumentServices
{
    public interface IDocumentService
    {
        IList<Paragraph> GetDocument(int numParagraphs, int latency);
        IEnumerable<Paragraph> GetDocumentEnumerable(int numParagraphs, int latency);
        Task<IList<Paragraph>> GetDocumentAsync(int numParagraphs, int latency, IProgress<int> progress, CancellationToken token);
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
