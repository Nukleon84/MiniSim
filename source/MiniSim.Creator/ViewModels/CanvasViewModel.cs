using Caliburn.Micro;
using MiniSim.Creator.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.ViewModels
{
    public class CanvasViewModel : Screen
    {
        bool _showGrid = true;

        VisualFlowsheet _currentFlowsheet;

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
    }
}
