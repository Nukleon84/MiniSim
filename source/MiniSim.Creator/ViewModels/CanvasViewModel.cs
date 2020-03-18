using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Numerics;
using MiniSim.Core.Reporting;
using MiniSim.Core.Thermodynamics;
using MiniSim.Creator.Flowsheeting;
using MiniSim.Creator.Messaging;
using MiniSim.Creator.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MiniSim.Creator.ViewModels
{
    public class CanvasViewModel : Screen
    {
        #region Fields
        IEventAggregator _eventAggregator;

        Point _mousePosition;
        Point _mousePositionBeforeContextMenu;
        Point _dragStart;

        VisualFlowsheet _currentFlowsheet;
        ThermodynamicSystem _currentPropertyBlock;

        Connector _selectedConnector;
        VisualUnit _selectedUnit;

        bool _isConnecting = false;
        bool _isDragging = false;
        bool _snapToGrid = true;
        bool _showGrid = true;
        bool _showTemperature = true;
        bool _showPressure = true;
        bool _showMassflow = true;
        bool _showVapourFraction = false;


        #endregion

        #region Properties
        public string FlowsheetName
        {
            get
            {
                return CurrentFlowsheet?.Name;
            }

            set
            {
                CurrentFlowsheet.Name = value;
                NotifyOfPropertyChange(() => FlowsheetName);
            }
        }
        public IList<VisualUnit> Items
        {
            get { return CurrentFlowsheet?.Items; }

        }
        public IList<Connection> Connections
        {
            get { return CurrentFlowsheet?.Connections; }

        }
        public bool ShowGrid
        {
            get
            {
                return _showGrid;
            }

            set
            {
                _showGrid = value;
                NotifyOfPropertyChange(() => ShowGrid);
            }
        }
        public VisualFlowsheet CurrentFlowsheet
        {
            get => _currentFlowsheet;
            set
            {
                _currentFlowsheet = value;
                NotifyOfPropertyChange(() => CurrentFlowsheet);
                NotifyOfPropertyChange(() => Items);
                NotifyOfPropertyChange(() => Connections);
                NotifyOfPropertyChange(() => FlowsheetName);
            }
        }
        public ThermodynamicSystem CurrentPropertyBlock { get => _currentPropertyBlock; set { _currentPropertyBlock = value; NotifyOfPropertyChange(() => CurrentPropertyBlock); } }

        public Point MousePosition { get => _mousePosition; set { _mousePosition = value; NotifyOfPropertyChange(() => MousePosition); } }
        public Connector SelectedConnector { get => _selectedConnector; set { _selectedConnector = value; NotifyOfPropertyChange(() => SelectedConnector); } }

        public bool IsDragging { get => _isDragging; set { _isDragging = value; NotifyOfPropertyChange(() => IsDragging); } }

        public VisualUnit SelectedUnit { get => _selectedUnit; set { _selectedUnit = value; NotifyOfPropertyChange(() => SelectedUnit); } }

        public bool SnapToGrid { get => _snapToGrid; set { _snapToGrid = value; NotifyOfPropertyChange(() => SnapToGrid); } }
        public bool ShowTemperature { get => _showTemperature; set { _showTemperature = value; NotifyOfPropertyChange(() => ShowTemperature); } }
        public bool ShowPressure { get => _showPressure; set { _showPressure = value; NotifyOfPropertyChange(() => ShowPressure); } }
        public bool ShowMassflow { get => _showMassflow; set { _showMassflow = value; NotifyOfPropertyChange(() => ShowMassflow); } }
        public bool ShowVapourFraction
        {
            get => _showVapourFraction;
            set { _showVapourFraction = value; NotifyOfPropertyChange(() => ShowVapourFraction); }
        }




        #endregion

        public CanvasViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

        }

        #region Event Handlers
        #region Model Building and Numerics


        public void Rebuild(object context)
        {
            StringBuilder reportBuilder = new StringBuilder();
            foreach (var unit in Items.Where(i=>i.RequiresRebuild))
            {
                try
                {
                    unit.Rebuild();
                    reportBuilder.AppendLine(unit.Report);
                    unit.RequiresRebuild = false;
                }
                catch (Exception e)
                {
                    reportBuilder.AppendLine(e.Message);
                }
            }
            var report = reportBuilder.ToString();

            if (_eventAggregator != null)
                _eventAggregator.BeginPublishOnUIThread(new BroadcastEphemeralMessage("Flowsheet rebuild!", "Show Report", () => ShowSolveReport(report)));
        }

        public void Initialize(object context)
        {
            StringBuilder reportBuilder = new StringBuilder();
            foreach (var unit in Items)
            {
                try
                {
                    unit.Initialize();
                    reportBuilder.AppendLine(unit.Report);
                    unit.RequiresInitialization = false;
                }
                catch (Exception e)
                {
                    reportBuilder.AppendLine(e.Message);
                }
            }
            var report = reportBuilder.ToString();


            if (_eventAggregator != null)
                _eventAggregator.BeginPublishOnUIThread(new BroadcastEphemeralMessage("Flowsheet initialized!", "Show Report", () => ShowSolveReport(report)));
        }

        public void Solve(object context)
        {
            Task.Run(() => SolveInBackground());
        }

        void SolveInBackground()
        {
            var logger = new StringBuilderLogger();
            var solver = new DecompositionSolver(logger);

            solver.OnProgress += i => _eventAggregator?.BeginPublishOnUIThread(new UpdateProgressBarMessage(i));

            var flowsheet = new Flowsheet(CurrentFlowsheet.Name);
            flowsheet.AddMaterialStreams(Connections.Select(c => c.ModelInstance).ToArray());
            flowsheet.AddUnits(Items.Select(u => u.ModelInstance).ToArray());


            var equationSystem = new AlgebraicSystem(CurrentFlowsheet.Name);
            flowsheet.CreateEquations(equationSystem);

            _eventAggregator?.BeginPublishOnUIThread(new UpdateEquationStatusMessage(equationSystem.NumberOfEquations, equationSystem.NumberOfVariables));

            if (equationSystem.NumberOfEquations != equationSystem.NumberOfVariables)
                return;

            var status = solver.Solve(equationSystem);
            var statusMessage = status ? "Solve Succeeded." : "Solve failed!";
            var report = logger.Flush();

            Generator generator = new Generator(logger);
            foreach (var connection in Connections)
            {
                generator.Report(connection.ModelInstance);
                connection.Report = logger.Flush();
            }
            foreach (var item in Items)
            {
                generator.Report(item.ModelInstance);
                item.Report = logger.Flush();
                if (status)
                    item.RequiresRecalculation = false;
            }

            if (status)
            {
                _eventAggregator?.BeginPublishOnUIThread(new UpdateSolverStatusMessage("OK", "YellowGreen"));
            }
            else
            {
                _eventAggregator?.BeginPublishOnUIThread(new UpdateSolverStatusMessage("Failed", "Red"));
            }

            if (_eventAggregator != null)
                _eventAggregator.BeginPublishOnUIThread(new BroadcastEphemeralMessage(statusMessage, "Show Report", () => ShowSolveReport(report)));

        }

        public void ShowSolveReport(string report)
        {
            _eventAggregator.BeginPublishOnUIThread(new ShowReportMessage() { Report = report });
        }

        #endregion

        #region Item Picking / Selection
        public void SelectAll()
        {
            foreach (var unit in Items)
                unit.IsSelected = true;
        }
        public void SelectConnector(Connector vm)
        {
            if (SelectedConnector == null)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    SelectedConnector = vm;
                    Mouse.SetCursor(Cursors.Cross);
                    _isConnecting = true;
                    return;
                }
            }
            else
            {
                if (SelectedConnector != vm && vm != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {

                        CurrentFlowsheet.Connect(SelectedConnector, vm, CurrentPropertyBlock, GetUnusedStreamName());
                        SelectedConnector = null;
                        _isConnecting = false;

                        if (_eventAggregator != null)
                            _eventAggregator.BeginPublishOnUIThread(new RecountFlowsheetElementsMessage());
                    }
                    else
                    {
                        SelectedConnector = vm;
                    }
                }
            }
        }

        public async void SelectItem(VisualUnit vm, MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {
                //_eventAggregator?.BeginPublishOnUIThread(new BroadcastEphemeralMessage("Show item " + vm.Name));

                var view = new EditFlowsheetItemDialog()
                {
                    DataContext = vm
                };

                //show the dialog
                var result = await DialogHost.Show(view, "RootDialog");
                return;
            }

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                foreach (var item in Items)
                    item.IsSelected = false;
                foreach (var item in Connections)
                    item.IsSelected = false;
            }

            if (vm != null)
                vm.Select();

            IsDragging = true;
            SelectedUnit = vm;
            _dragStart = MousePosition;

            args.Handled = true;
        }

        public async void SelectStream(Connection vm, MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {           
                var view = new EditFlowsheetItemDialog()
                {
                    DataContext = vm.ModelInstance
                };

                //show the dialog
                var result = await DialogHost.Show(view, "RootDialog");
                return;
            }


            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                foreach (var item in Items)
                    item.IsSelected = false;
                foreach (var item in Connections)
                    item.IsSelected = false;
            }
            if (vm != null)
                vm.Select();
            args.Handled = true;
        }
        #endregion

        public void AddItem(object sender, object context)
        {
            var menuItem = (MenuItem)sender;
            if (menuItem == null)
                return;

            var position = _mousePositionBeforeContextMenu;
            var relativePort = context as Connector;
            VisualUnit newUnit = null;

            position = updatePositionByPortDirection(position, relativePort);


            newUnit = AddItemOnPosition(menuItem.Name, position);

            if (relativePort != null)
            {
                var relativeUnit = relativePort.Owner;
                if (relativeUnit != null && newUnit != null)
                {
                    Connector inPort = findInletPortByUnitTypeAndPortDirection(relativePort, newUnit);
                    if (inPort != null)
                        CurrentFlowsheet.Connect(relativePort, inPort, CurrentPropertyBlock, GetUnusedStreamName());
                }
            }

            _eventAggregator?.BeginPublishOnUIThread(new RecountFlowsheetElementsMessage());
        }

        public void DeleteSelection()
        {
            foreach (var item in Items.Where(i => i.IsSelected).ToArray())
            {
                var affectedConnections = Connections.Where(c => c.Sink.Owner == item || c.Source.Owner == item).ToArray();

                foreach (var connectionToRemove in affectedConnections)
                {
                    connectionToRemove.Source.IsConnected = false;
                    connectionToRemove.Source.Connection = null;
                    connectionToRemove.Sink.IsConnected = false;
                    connectionToRemove.Sink.Connection = null;
                    Connections.Remove(connectionToRemove);
                }

                Items.Remove(item);
            }
        }

        #region Mouse Handling

        public void ContextMenuOpening(object sender, EventArgs context)
        {
            _mousePositionBeforeContextMenu = MousePosition;
        }

        public void MouseDownOnGrid(object sender, MouseEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                foreach (var item in Items)
                    item.IsSelected = false;
                foreach (var item in Connections)
                    item.IsSelected = false;
            }
        }

        public void MouseUpOnGrid(object sender, MouseEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SelectedConnector = null;
            }

            if (IsDragging)
            {
                var gridSize = 20;

                foreach (var unit in Items.Where(i => i.IsSelected))
                {
                    unit.X = round((int)unit.X, gridSize);
                    unit.Y = round((int)unit.Y, gridSize);
                }
                IsDragging = false;
            }
        }

        public void MouseMoveOnGrid(object sender, MouseEventArgs e)
        {
            var grid = (FrameworkElement)sender;
            var p = e.GetPosition(grid);
            MousePosition = p;

            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _isConnecting = false;
                SelectedConnector = null;
            }

            if (_isConnecting)
                Mouse.SetCursor(Cursors.Cross);
            else
                Mouse.SetCursor(Cursors.Arrow);

            if (IsDragging)
            {
                var delta = MousePosition - _dragStart;

                foreach (var unit in Items.Where(i => i.IsSelected))
                {
                    unit.X += delta.X;
                    unit.Y += delta.Y;
                }
                _dragStart = MousePosition;
            }
        }
        #endregion

        #endregion

        #region Private Helper Functions
        string GetUnusedStreamName()
        {
            int i = 1;
            var existingConnectionNames = Connections.Select(c => c.Name);
            var name = "S001";
            do
            {
                name = "S" + (i).ToString("000");
                i++;

            } while (existingConnectionNames.Contains(name));
            return name;
        }

        string GetUnusedUnitName(string pattern)
        {
            int i = 1;
            var existingUnitNames = Items.Select(c => c.Name);
            var name = pattern + "001";
            do
            {
                name = pattern + (i).ToString("000");
                i++;

            } while (existingUnitNames.Contains(name));
            return name;
        }


        static int round(int n, int grid)
        {
            // Smaller multiple 
            int a = (n / grid) * grid;

            // Larger multiple 
            int b = a + grid;

            // Return of closest of two 
            return (n - a > b - n) ? b : a;
        }


        VisualUnit AddItemOnPosition(string modelType, Point position)
        {
            var gridSize = 20;

            var roundedPos = position;
            roundedPos.X = round((int)position.X, gridSize);
            roundedPos.Y = round((int)position.Y, gridSize);

            switch (modelType)
            {
                case "AddSource":
                    return CurrentFlowsheet.AddSource(GetUnusedUnitName("Source"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddSink":
                    return CurrentFlowsheet.AddSink(GetUnusedUnitName("Sink"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddHeater":
                    return CurrentFlowsheet.AddHeater(GetUnusedUnitName("Heater"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddFlash":
                    return CurrentFlowsheet.AddFlash(GetUnusedUnitName("Flash"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddMixer":
                    return CurrentFlowsheet.AddMixer(GetUnusedUnitName("Mix"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddSplitter":
                    return CurrentFlowsheet.AddSplitter(GetUnusedUnitName("Split"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddColumn":
                    return CurrentFlowsheet.AddColumn(GetUnusedUnitName("Column"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                case "AddRectification":
                    var column = CurrentFlowsheet.AddColumn(GetUnusedUnitName("Column"), CurrentPropertyBlock, (int)roundedPos.X, (int)roundedPos.Y);
                    var condenser = CurrentFlowsheet.AddHeater(GetUnusedUnitName("Heater"), CurrentPropertyBlock, (int)roundedPos.X + 100, (int)roundedPos.Y - 80);
                    var splitter = CurrentFlowsheet.AddSplitter(GetUnusedUnitName("Split"), CurrentPropertyBlock, (int)roundedPos.X + 240, (int)roundedPos.Y - 80);
                    var reboiler = CurrentFlowsheet.AddFlash(GetUnusedUnitName("Flash"), CurrentPropertyBlock, (int)roundedPos.X + 100, (int)roundedPos.Y + 360);
                    CurrentFlowsheet.Connect(column, "VOut", condenser, "In", CurrentPropertyBlock, GetUnusedStreamName());
                    CurrentFlowsheet.Connect(condenser, "Out", splitter, "In", CurrentPropertyBlock, GetUnusedStreamName());
                    CurrentFlowsheet.Connect(splitter, "Out2", column, "LIn", CurrentPropertyBlock, GetUnusedStreamName());
                    CurrentFlowsheet.Connect(column, "LOut", reboiler, "In", CurrentPropertyBlock, GetUnusedStreamName());
                    CurrentFlowsheet.Connect(reboiler, "Vap", column, "VIn", CurrentPropertyBlock, GetUnusedStreamName());
                    return column;
                default:
                    return null;
            }
        }

        private static Connector findInletPortByUnitTypeAndPortDirection(Connector relativePort, VisualUnit newUnit)
        {
            var inPort = newUnit.GetConnectorByName("In");
            if (newUnit.DisplayIcon == IconTypes.Mixer)
            {
                if (relativePort.Direction == PortNormal.Up)
                    inPort = newUnit.GetConnectorByName("In3");
                else if (relativePort.Direction == PortNormal.Down)
                    inPort = newUnit.GetConnectorByName("In1");
                else
                    inPort = newUnit.GetConnectorByName("In2");
            }

            if (newUnit.DisplayIcon == IconTypes.ColumnSection)
            {
                inPort = newUnit.GetConnectorByName("Feeds");
            }


            return inPort;
        }

        private static Point updatePositionByPortDirection(Point position, Connector relativePort)
        {
            if (relativePort != null)
            {
                if (relativePort.Direction == PortNormal.Right)
                    position.X += 100;
                if (relativePort.Direction == PortNormal.Left)
                    position.X -= 100;
                if (relativePort.Direction == PortNormal.Up)
                {
                    position.X += 100;
                    position.Y -= 60;
                }
                if (relativePort.Direction == PortNormal.Down)
                {
                    position.X += 100;
                    position.Y += 60;
                }
            }

            return position;
        }
        #endregion

    }
}
