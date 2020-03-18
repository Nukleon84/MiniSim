using Caliburn.Micro;
using MiniSim.Creator.Interfaces;
using MiniSim.Creator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MiniSim.Creator
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        #region Constructor
        public Bootstrapper()
        {
            Initialize();
        }
        #endregion
      

        #region Overrides
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
             DisplayRootViewFor<IShell>();
        }

        protected override void Configure()
        {
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IShell, ShellViewModel>();
        }
        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;
            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        #endregion
    }
}
