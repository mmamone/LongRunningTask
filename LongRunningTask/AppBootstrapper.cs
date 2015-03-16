using System;
using System.Collections.Generic;
using Caliburn.Micro;
using DocumentServices;
using LongRunningTask.ViewModels;
using LongRunningTask.Views;

namespace LongRunningTask
{
    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, ContainerViewModel>();
            container.PerRequest<DocumentAsyncViewModel>();
            container.PerRequest<DocumentReactiveAsyncViewModel>();
            container.PerRequest<DocumentReactiveViewModel>();
            container.PerRequest<DocumentsReactiveViewModel>();
            container.PerRequest<IDocumentService, DocumentService>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
    }
}