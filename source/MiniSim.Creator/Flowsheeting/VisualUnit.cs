using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.ModelLibrary;
using MiniSim.Core.Reporting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MiniSim.Creator.Flowsheeting
{
    /// <summary>
    /// Base class for serialization information of a drawable unit on the process flowsheet like a unit or a stream indicator.
    /// </summary>
    [Serializable]
    public class VisualUnit : DrawableItem
    {
        #region Fields
        bool _isActive = true;
        IList<Connector> _connectors = new ObservableCollection<Connector>();
        IconTypes _displayIcon = IconTypes.Vessel;

        private bool _requiresRebuild = false;
        private bool _requiresInit = false;
        private bool _requiresRecalculation = false;
        string _report = "";

        ProcessUnit _modelInstance;

        #endregion


        public VisualUnit()
        {
            this.OnPositionUpdated += UpdateConnectorsOnMove;
        }


        #region Properties
        public virtual bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; NotifyOfPropertyChange(() => IsActive); }
        }

        public virtual string Report
        {
            get
            {
                return _report;
            }
            set { _report = value; NotifyOfPropertyChange(() => Report); }
        }


        [Category("Information")]
        [DisplayName("Display Icon")]
        [Description("The Visual Representation of the unit.")]
        public virtual IconTypes DisplayIcon
        {
            get { return _displayIcon; }
            set { _displayIcon = value; NotifyOfPropertyChange(() => DisplayIcon); }
        }

        [Category("Information")]
        [DisplayName("Connectors")]
        [Description("The ports (inlets and outlets) of the unit.")]
        public virtual IList<Connector> Connectors
        {
            get { return _connectors; }
            set { _connectors = new ObservableCollection<Connector>(value); NotifyOfPropertyChange(() => Connectors); }
        }


        public bool RequiresRebuild
        {
            get
            {
                return _requiresRebuild;
            }

            set
            {
                _requiresRebuild = value;
                NotifyOfPropertyChange(() => RequiresRebuild);

            }
        }

        public bool RequiresInitialization
        {
            get
            {
                return _requiresInit;
            }

            set
            {
                _requiresInit = value;
                NotifyOfPropertyChange(() => RequiresInitialization);

            }
        }

        public bool RequiresRecalculation
        {
            get
            {
                return _requiresRecalculation;
            }

            set
            {
                _requiresRecalculation = value;
                NotifyOfPropertyChange(() => RequiresRecalculation);
            }
        }

        public ProcessUnit ModelInstance
        {
            get => _modelInstance;
            set
            {
                _modelInstance = value;
                NotifyOfPropertyChange(() => ModelInstance);
                NotifyOfPropertyChange(() => Variables);
                NotifyOfPropertyChange(() => Parameters);
                NotifyOfPropertyChange(() => ModelClass);
            }
        }

        public IList<Variable> Variables { get => ModelInstance?.Variables; }
        public IList<Variable> Parameters { get => ModelInstance?.Parameters; }
        public string ModelClass { get => ModelInstance?.Class; }

        #endregion

        #region Public Functions
        public virtual Connector GetConnectorByName(string name)
        {
            return Connectors.FirstOrDefault(connector => connector.Name == name);
        }

        public virtual void AddConnector(Connector newConnector)
        {
            newConnector.Owner = this;
            Connectors.Add(newConnector);
        }

        public void Rebuild()
        {
            StringBuilder log = new StringBuilder();

            log.AppendLine($"Rebuilding Unit {Name} ({ModelClass})");
            foreach (var connector in Connectors.Where(c => c.IsConnected))
            {
                var stream = connector.Connection?.ModelInstance;
                if (stream != null)
                {
                    var portName = connector.Name;
                    if (ModelInstance.Class == "Mixer")
                    {
                        if (portName.StartsWith("In"))
                            portName = "In";
                    }

                    if (ModelClass == "TraySection" && portName == "Feeds")
                    {
                        var trayModel = ModelInstance as EquilibriumStageSection;
                        trayModel.ConnectFeed(stream, 10);
                    }
                    else
                        ModelInstance.Connect(portName, stream);

                    log.AppendLine($"Connecting stream {stream.Name} to port {portName}");
                }
            }
            log.AppendLine("");
            Report = log.ToString();
        }

        public void Initialize()
        {
            var logger = new StringBuilderLogger();
            logger.Log($"Initializing Unit {Name} ({ModelClass})");


            ModelInstance.Initialize();
            Generator generator = new Generator(logger);
            generator.Report(ModelInstance);
            logger.Log("");
            Report = logger.Flush();

            foreach (var connector in Connectors.Where(c => c.IsConnected))
            {
                generator.Report(connector.Connection.ModelInstance);
                connector.Connection.Report = logger.Flush();
            }
        }

        public void Solve()
        {
            var logger = new StringBuilderLogger();
            logger.Log($"Solving Unit {Name} ({ModelClass})");
            ModelInstance.Solve(logger);
            logger.Log("");
            logger.Log("Results");
            logger.Log("");
            Generator generator = new Generator(logger);
            generator.Report(ModelInstance);
            logger.Log("");
            Report = logger.Flush();
        }
        #endregion

        void UpdateConnectorsOnMove(DrawableItem item)
        {
            foreach (var con in Connectors)
            {
                if (con.OnPositionUpdated != null)
                    con.OnPositionUpdated(con);
            }
        }
        public void Select()
        {
            IsSelected = true;
        }
    }
}
