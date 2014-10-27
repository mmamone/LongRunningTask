using System;
using Caliburn.Micro.ReactiveUI;
using LongRunningTask.Views;

namespace LongRunningTask.ViewModels
{
    public interface IShell
    {
        
    }
    public class ContainerViewModel : ReactiveConductor<ReactiveScreen>.Collection.OneActive, IShell
    {
        private readonly Func<DocumentAsyncViewModel> _documentAsyncVmFac;
        private readonly Func<DocumentReactiveAsyncViewModel> _documentRxAsyncVmFac;
        private readonly Func<DocumentReactiveViewModel> _documentRxVmFac;

        public ContainerViewModel(
            Func<DocumentAsyncViewModel> documentAsyncVmFac,
            Func<DocumentReactiveAsyncViewModel> documentRxAsyncVmFac,
            Func<DocumentReactiveViewModel> documentRxVmFac)
        {
            _documentAsyncVmFac = documentAsyncVmFac;
            _documentRxAsyncVmFac = documentRxAsyncVmFac;
            _documentRxVmFac = documentRxVmFac;
            ActivateItem(_documentAsyncVmFac());
            ActivateItem(_documentRxAsyncVmFac());
            ActivateItem(_documentRxVmFac());
        }
    }
}