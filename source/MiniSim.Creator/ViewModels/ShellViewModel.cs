using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using MiniSim.Core.Thermodynamics;
using MiniSim.Creator.DebugHelper;
using MiniSim.Creator.Flowsheeting;
using MiniSim.Creator.Interfaces;
using MiniSim.Creator.Messaging;
using MiniSim.Creator.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiniSim.Creator.ViewModels
{
    public class ShellViewModel : Screen, IShell, 
        IHandle<BroadcastEphemeralMessage>, 
        IHandle<RecountFlowsheetElementsMessage>, 
        IHandle<ShowReportMessage>, 
        IHandle<UpdateSolverStatusMessage>,
        IHandle<UpdateProgressBarMessage>,
        IHandle<UpdateEquationStatusMessage>
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
        ThermodynamicSystem _currentPropertyBlock;

        IList<VisualFlowsheet> _availableFlowsheets;
        IList<ThermodynamicSystem> _availablePropertyBlocks;

        IEventAggregator _eventAggregator;
        SnackbarMessageQueue _messageQueue = new SnackbarMessageQueue();
        ICommand _selectAllCommand;
        ICommand _deleteCommand;

        public ICommand SelectAllCommand { get => _selectAllCommand; set => _selectAllCommand = value; }
        public ICommand DeleteCommand { get => _deleteCommand; set => _deleteCommand = value; }

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
        public IList<ThermodynamicSystem> AvailablePropertyBlocks { get => _availablePropertyBlocks; set { _availablePropertyBlocks = value; NotifyOfPropertyChange(() => AvailablePropertyBlocks); } }

        public VisualFlowsheet CurrentFlowsheet { get => _currentFlowsheet; set { _currentFlowsheet = value; NotifyOfPropertyChange(() => CurrentFlowsheet); ChangeFlowsheet(); } }
        public ThermodynamicSystem CurrentPropertyBlock { get => _currentPropertyBlock; set { _currentPropertyBlock = value; NotifyOfPropertyChange(() => CurrentPropertyBlock); ChangePropertyBlock(); } }

        public SnackbarMessageQueue MessageQueue { get => _messageQueue; set => _messageQueue = value; }


        public ShellViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            SelectAllCommand = new RelayCommand((o) => SelectAll());
            DeleteCommand = new RelayCommand((o) => DeleteSelection());

            CurrentCanvas = new CanvasViewModel(eventAggregator);

            AvailablePropertyBlocks = new ObservableCollection<ThermodynamicSystem>();
            AvailablePropertyBlocks.Add(PropertyBlockFactory.CreateSystem1());
            AvailablePropertyBlocks.Add(PropertyBlockFactory.CreateSystem2());
            AvailablePropertyBlocks.Add(PropertyBlockFactory.CreateSystem3());

            CurrentPropertyBlock = AvailablePropertyBlocks.First();


            AvailableFlowsheets = new ObservableCollection<VisualFlowsheet>();
            AvailableFlowsheets.Add(VisualFlowsheetFactory.CreateTrivialDemo3());
          //  AvailableFlowsheets.Add(VisualFlowsheetFactory.CreateTrivialDemo());
            AvailableFlowsheets.Add(VisualFlowsheetFactory.CreateTrivialDemo2(CurrentPropertyBlock));
            CurrentFlowsheet = AvailableFlowsheets.First();

     


        }

        public void Shutdown()
        {
            App.Current.Shutdown();
        }

        public void SelectAll()
        {
           CurrentCanvas.SelectAll();
        }

        public void DeleteSelection()
        {
            CurrentCanvas.DeleteSelection();
        }

        void ChangeFlowsheet()
        {
            if (CurrentCanvas != null)
            {
                CurrentCanvas.CurrentFlowsheet = CurrentFlowsheet;
               
            }
        }

        void ChangePropertyBlock()
        {
            if (CurrentCanvas != null)
            {
                CurrentCanvas.CurrentPropertyBlock = CurrentPropertyBlock;
               
            }

        }
        public void Handle(BroadcastEphemeralMessage message)
        {
            if (String.IsNullOrEmpty(message.ActionText))
                MessageQueue.Enqueue(message.MessageText);
            else
                MessageQueue.Enqueue(message.MessageText, message.ActionText,message.Callback);
        }

        public void Handle(RecountFlowsheetElementsMessage message)
        {
            NumberOfUnits = AvailableFlowsheets.Select(f => f.Items.Count).Sum();
            NumberOfStreams = AvailableFlowsheets.Select(f => f.Connections.Count).Sum();

            NumberOfVariables = 0;
            NumberOfEquations = 0;
            NumberOfDOF = 0;



        }

        public  void Handle(ShowReportMessage message)
        {
            var view = new ShowLogDialog()
            {
                DataContext = message.Report
            };

            //show the dialog
            var result =  DialogHost.Show(view, "RootDialog");
        }

        public void Handle(UpdateSolverStatusMessage message)
        {
            SimulationStatusMessage = message.SolverStatus;
            SimulationStatusColor = message.Color;
        }

        public void Handle(UpdateProgressBarMessage message)
        {
            CurrentProgress = message.CurrentProgress;
        }

        public void Handle(UpdateEquationStatusMessage message)
        {
            NumberOfEquations = message.NumberOfEquations;
            NumberOfVariables = message.NumberOfVariables;
            NumberOfDOF = NumberOfVariables - NumberOfEquations;

            if(NumberOfDOF!=0)
            {
                SimulationStatusMessage = "Non-square";
                SimulationStatusColor = "Orange";
            }
        }
    }
}
