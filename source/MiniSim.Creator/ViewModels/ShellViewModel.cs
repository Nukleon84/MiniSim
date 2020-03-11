using Caliburn.Micro;
using MiniSim.Creator.DebugHelper;
using MiniSim.Creator.Flowsheeting;
using MiniSim.Creator.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.ViewModels
{
    public class ShellViewModel : Screen, IShell
    {
        CanvasViewModel _currentCanvas;
        int _currentProgress = 20;
        int _numberOfUnits = 1;
        int _numberOfStreams = 2;
        int _numberOfEquations = 20;
        int _numberOfVariables = 20;
        int _numberOfDOF = 0;

        string _simulationStatusColor = "YellowGreen";
        string _simulationStatusMessage = "OK";

        VisualFlowsheet _currentFlowsheet;
        IList<VisualFlowsheet> _availableFlowsheets;



        public string SimulationStatusMessage
        {
            get
            {
                return _simulationStatusMessage;
            }

            set
            {
                _simulationStatusMessage = value;
                NotifyOfPropertyChange(() => SimulationStatusMessage);
            }
        }
        public string SimulationStatusColor
        {
            get
            {
                return _simulationStatusColor;
            }

            set
            {
                _simulationStatusColor = value;
                NotifyOfPropertyChange(() => SimulationStatusColor);
            }
        }


        public int CurrentProgress
        {
            get
            {
                return _currentProgress;
            }

            set
            {
                _currentProgress = value;
                NotifyOfPropertyChange(() => CurrentProgress);
            }
        }


        public CanvasViewModel CurrentCanvas
        {
            get
            {
                return _currentCanvas;
            }

            set
            {
                _currentCanvas = value;
                NotifyOfPropertyChange(() => CurrentCanvas);
            }
        }

        public int NumberOfUnits { get => _numberOfUnits; set { _numberOfUnits = value; NotifyOfPropertyChange(() => NumberOfUnits); } }
        public int NumberOfStreams { get => _numberOfStreams; set { _numberOfStreams = value; NotifyOfPropertyChange(() => NumberOfStreams); } }
        public int NumberOfEquations { get => _numberOfEquations; set { _numberOfEquations = value; NotifyOfPropertyChange(() => NumberOfEquations); } }
        public int NumberOfVariables { get => _numberOfVariables; set { _numberOfVariables = value; NotifyOfPropertyChange(() => NumberOfVariables); } }
        public int NumberOfDOF { get => _numberOfDOF; set { _numberOfDOF = value; NotifyOfPropertyChange(() => NumberOfDOF); } }

        public IList<VisualFlowsheet> AvailableFlowsheets { get => _availableFlowsheets; set { _availableFlowsheets = value; NotifyOfPropertyChange(() => AvailableFlowsheets); } }
        public VisualFlowsheet CurrentFlowsheet { get => _currentFlowsheet; set { _currentFlowsheet = value; NotifyOfPropertyChange(() => CurrentFlowsheet); ChangeFlowsheet(); } }


        public ShellViewModel()
        {
            CurrentCanvas = new CanvasViewModel();
          

            AvailableFlowsheets = new ObservableCollection<VisualFlowsheet>();
            AvailableFlowsheets.Add(VisualFlowsheetFactory.CreateTrivialDemo());
            AvailableFlowsheets.Add(VisualFlowsheetFactory.CreateTrivialDemo2());
            CurrentFlowsheet = AvailableFlowsheets.First();
        }

        public void Shutdown()
        {
            App.Current.Shutdown();
        }

        void ChangeFlowsheet()
        {
            if (CurrentCanvas != null)
                CurrentCanvas.CurrentFlowsheet = CurrentFlowsheet;
        }
    }
}
