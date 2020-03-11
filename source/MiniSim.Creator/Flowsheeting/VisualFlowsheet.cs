using Caliburn.Micro;
using MiniSim.Core.Flowsheeting;
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
        public void Connect(VisualUnit source, string sourcePort, VisualUnit sink, string sinkPort, string label )
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
                    newCon.Name = label;
                    newCon.UpdatePathGeometry();
                    Connections.Add(newCon);
                }
            }
        }

        public void Connect(string source, string sourcePort, string sink, string sinkPort, string label)
        {
            var sourceUnit = Items.FirstOrDefault(i => i.Name == source);
            var sinkUnit = Items.FirstOrDefault(i => i.Name == sink);

            if (sourceUnit != null && sinkUnit != null)
            {
                Connect(sourceUnit, sourcePort, sinkUnit, sinkPort,  label);
            }
        }


        public VisualUnit AddSource(string name, ThermodynamicSystem sys, int x, int y)
        {
            var source = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Stream };

            source.X = x;
            source.Y = y;
            source.Width = 40;
            source.Height = 40;

            source.Connectors.Add(new Connector
            {
                Name = "Out",
                Type = "Material",
                X = 45,
                Y = 15,
                Owner = source,
                Intent = PortDirection.Out,
                Direction = PortNormal.Right
            });
            Items.Add(source);

            return source;
        }

        public VisualUnit AddSink(string name, ThermodynamicSystem sys, int x, int y)
        {
            var sink = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.Stream };
            sink.X = x;
            sink.Y = y;
            sink.Width = 40;
            sink.Height = 40;

            sink.Connectors.Add(new Connector
            {
                Name = "In",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = sink,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            Items.Add(sink);
            return sink;
        }
        public VisualUnit AddFlash(string name, ThermodynamicSystem sys, int x, int y)
        {
            var flash = new VisualUnit() { Name = name, DisplayIcon = Core.Flowsheeting.IconTypes.TwoPhaseFlash };

            flash.X = x;
            flash.Y = y;
            flash.Width = 40;
            flash.Height = 40;

            flash.Connectors.Add(new Connector
            {
                Name = "In",
                Type = "Material",
                X = -5,
                Y = 15,
                Owner = flash,
                Intent = PortDirection.In,
                Direction = PortNormal.Left
            });
            flash.Connectors.Add(new Connector
            {
                Name = "Vap",
                Type = "Material",
                X = 15,
                Y = -5,
                Owner = flash,
                Intent = PortDirection.Out,
                Direction = PortNormal.Up
            });
            flash.Connectors.Add(new Connector
            {
                Name = "Liq",
                Type = "Material",
                X = 15,
                Y = 35,
                Owner = flash,
                Intent = PortDirection.Out,
                Direction = PortNormal.Down
            });
            Items.Add(flash);
            return flash;
        }

    }
}
