using Caliburn.Micro;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.ModelLibrary;
using MiniSim.Core.Thermodynamics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MiniSim.Creator.Flowsheeting
{
    public class VisualFlowsheet : PropertyChangedBase
    {
        string _canvasColor = "White";

        IList<VisualUnit> _items = new ObservableCollection<VisualUnit>();
        IList<Connection> _connections = new ObservableCollection<Connection>();
        string _name = "Base";
    
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
   
        public IList<VisualUnit> Items
        {
            get { return _items; }
            set { _items = new ObservableCollection<VisualUnit>(value); }
        }

        public IList<Connection> Connections
        {
            get { return _connections; }
            set { _connections = new ObservableCollection<Connection>(value); }
        }

        public string CanvasColor
        {
            get
            {
                return _canvasColor;
            }

            set
            {
                _canvasColor = value;
            }
        }

        public VisualUnit GetUnitByName(string name)
        {
            return Items.FirstOrDefault(i => i.Name == name);
        }
        public Connection GetStreamByName(string name)
        {
            return Connections.FirstOrDefault(i => i.Name == name);
        }

        public IList<VisualUnit> GetAllDescendantsOfUnit(VisualUnit unit)
        {
            var descendants = new List<VisualUnit>();

            // descendants.Add(unit);

            var visited = new Dictionary<VisualUnit, bool>();


            descendants.AddRange(GetDescendantsOfUnit(unit, visited));
            return descendants;
        }

        IList<VisualUnit> GetDescendantsOfUnit(VisualUnit unit, Dictionary<VisualUnit, bool> visited)
        {
            var descendants = new List<VisualUnit>();

            foreach (var outgoing in GetAllOutputStreams(unit))
            {
                if (!visited.ContainsKey(outgoing.Sink.Owner))
                {
                    descendants.Add(outgoing.Sink.Owner);
                    visited.Add(outgoing.Sink.Owner, true);
                }
            }

            foreach (var descendant in descendants.ToArray())
            {
                var otherNodes = GetDescendantsOfUnit(descendant, visited);
                descendants.AddRange(otherNodes);
            }
            return descendants;
        }

        public IList<Connection> GetAllOutputStreams(VisualUnit unit)
        {
            return Connections.Where(c => c.Source.Owner == unit).ToList();
        }


       

        public void Connect(VisualUnit source, string sourcePort, VisualUnit sink, string sinkPort, ThermodynamicSystem sys, string label)
        {
            if (source != null && source != null)
            {
                var sourceConnector = source.Connectors.FirstOrDefault(c => c.Name == sourcePort);
                var sinkConnector = sink.Connectors.FirstOrDefault(c => c.Name == sinkPort);

                if (sourceConnector != null && sinkConnector != null)
                {
                    var newCon = new Connection() { Source = sourceConnector, Sink = sinkConnector };
                    sourceConnector.IsConnected = true;
                    sinkConnector.IsConnected = true;
                    sinkConnector.Connection = newCon;
                    sourceConnector.Connection = newCon;
                    newCon.ModelInstance = new MaterialStream(label, sys);
                    newCon.Name = label;
                    newCon.UpdatePathGeometry();
                    Connections.Add(newCon);
                }
            }
        }

        public void Connect(Connector sourcePort, Connector sinkPort, ThermodynamicSystem sys, string label)
        {
            if (sourcePort != null && sinkPort != null)
            {
                var newCon = new Connection() { Source = sourcePort, Sink = sinkPort };
                sourcePort.IsConnected = true;
                sinkPort.IsConnected = true;
                newCon.Name = label;
                newCon.UpdatePathGeometry();
                newCon.ModelInstance = new MaterialStream(label, sys);
                sinkPort.Connection = newCon;
                sourcePort.Connection = newCon;
                Connections.Add(newCon);
            }
        }
               
        public void Connect(string source, string sourcePort, string sink, string sinkPort, ThermodynamicSystem sys, string label)
        {
            var sourceUnit = Items.FirstOrDefault(i => i.Name == source);
            var sinkUnit = Items.FirstOrDefault(i => i.Name == sink);

            if (sourceUnit != null && sinkUnit != null)
            {
                Connect(sourceUnit, sourcePort, sinkUnit, sinkPort, sys, label);
            }
        }


        #region Helper Functions for Adding Units to the Flowsheet

        public VisualUnit AddSource(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Stream };

            unit.ModelInstance = new Source(name, sys);
            unit.ModelInstance.ApplyDefaultSpecifications();

            unit.RequiresRebuild = true;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;
            
            unit.X = x;
            unit.Y = y;
            unit.Width = 40;
            unit.Height = 40;

            unit.Connectors.Add(new Connector
            {
                Name = "Out",
                Type = "Material",
                X = 45,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Right
            });
            Items.Add(unit);

            return unit;
        }
        public VisualUnit AddSink(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Stream };
            unit.X = x;
            unit.Y = y;
            unit.Width = 40;
            unit.Height = 40;

            unit.ModelInstance = new Sink(name, sys);
            unit.ModelInstance.ApplyDefaultSpecifications();
            unit.RequiresRebuild = true;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;


            unit.Connectors.Add(new Connector
            {
                Name = "In",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            Items.Add(unit);
            return unit;
        }

        public VisualUnit AddHeater(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Heater };
            unit.X = x;
            unit.Y = y;
            unit.Width = 40;
            unit.Height = 40;

            unit.ModelInstance = new Heater(name, sys);
            unit.ModelInstance.ApplyDefaultSpecifications();
            unit.RequiresRebuild = true;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;

            unit.Connectors.Add(new Connector
            {
                Name = "In",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });

            unit.Connectors.Add(new Connector
            {
                Name = "Out",
                Type = "Material",
                X = 35,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Right
            });


            Items.Add(unit);
            return unit;
        }

        public VisualUnit AddFlash(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.TwoPhaseFlash };

            unit.X = x;
            unit.Y = y;
            unit.Width = 40;
            unit.Height = 40;

            unit.ModelInstance = new Flash(name, sys);
            unit.ModelInstance.ApplyDefaultSpecifications();
            unit.RequiresRebuild = true;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;



            unit.Connectors.Add(new Connector
            {
                Name = "In",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            unit.Connectors.Add(new Connector
            {
                Name = "Vap",
                Type = "Material",
                X = 15,
                Y = -5,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Up
            });
            unit.Connectors.Add(new Connector
            {
                Name = "Liq",
                Type = "Material",
                X = 15,
                Y = 35,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Down
            });
            Items.Add(unit);
            return unit;
        }

        public VisualUnit AddColumn(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.ColumnSection };

            unit.X = x;
            unit.Y = y;
            unit.Width = 60;
            unit.Height = 300;

            unit.ModelInstance = new EquilibriumStageSection(name, sys,20);
            unit.ModelInstance.ApplyDefaultSpecifications();
            unit.RequiresRebuild = true;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;
            

            unit.Connectors.Add(new Connector
            {
                Name = "Feeds",
                Type = "Material",
                X = -5,
                Y = 135,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            unit.Connectors.Add(new Connector
            {
                Name = "Sidestreams",
                Type = "Material",
                X = 55,
                Y = 135,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Left
            });

            unit.Connectors.Add(new Connector
            {
                Name = "VIn",
                Type = "Material",
                X = 55,
                Y = 275,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Right
            });
            unit.Connectors.Add(new Connector
            {
                Name = "LIn",
                Type = "Material",
                X = 55,
                Y = 25,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Right
            });
            unit.Connectors.Add(new Connector
            {
                Name = "VOut",
                Type = "Material",
                X = 25,
                Y = -5,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Up
            });
            unit.Connectors.Add(new Connector
            {
                Name = "LOut",
                Type = "Material",
                X = 25,
                Y = 295,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Down
            });


            Items.Add(unit);
            return unit;
        }

        public VisualUnit AddSplitter(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Splitter };

            unit.X = x;
            unit.Y = y;
            unit.Width = 40;
            unit.Height = 40;

            unit.ModelInstance = new Splitter(name, sys);
            unit.ModelInstance.ApplyDefaultSpecifications();
            unit.RequiresRebuild = true;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;


            unit.Connectors.Add(new Connector
            {
                Name = "In",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            unit.Connectors.Add(new Connector
            {
                Name = "Out1",
                Type = "Material",
                X = 15,
                Y = -5,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Up
            });
            unit.Connectors.Add(new Connector
            {
                Name = "Out2",
                Type = "Material",
                X = 15,
                Y = 35,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Down
            });
            Items.Add(unit);
            return unit;
        }
        public VisualUnit AddMixer(string name, ThermodynamicSystem sys, int x, int y)
        {
            var unit = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Mixer };

            unit.X = x;
            unit.Y = y;
            unit.Width = 40;
            unit.Height = 40;
            unit.ModelInstance = new Mixer(name, sys);
            unit.ModelInstance.ApplyDefaultSpecifications();

            unit.RequiresRebuild = false;
            unit.RequiresInitialization = true;
            unit.RequiresRecalculation = true;


            unit.Connectors.Add(new Connector
            {
                Name = "In1",
                Type = "Material",
                X = 15,
                Y = -5,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Up
            });
            unit.Connectors.Add(new Connector
            {
                Name = "In2",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            unit.Connectors.Add(new Connector
            {
                Name = "In3",
                Type = "Material",
                X = 15,
                Y = 35,
                Owner = unit,
                Intent = PortDirection.In,
                Direction = PortNormal.Down
            });
            unit.Connectors.Add(new Connector
            {
                Name = "Out",
                Type = "Material",
                X = 35,
                Y = 15,
                Owner = unit,
                Intent = PortDirection.Out,
                Direction = PortNormal.Right
            });
            Items.Add(unit);
            return unit;
        }
        #endregion

    }
}
