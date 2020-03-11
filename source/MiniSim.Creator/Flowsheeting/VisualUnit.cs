using MiniSim.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;


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
        private bool _requiresInput = false;
        private bool _requiresRecalculation = false;
         string _report = "";
       
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
            set { _report = value; NotifyOfPropertyChange(() => Report);  }
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

        public bool RequiresInput
        {
            get
            {
                return _requiresInput;
            }

            set
            {
                _requiresInput = value;
                NotifyOfPropertyChange(() => RequiresInput);
            
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
        #endregion

        void UpdateConnectorsOnMove(DrawableItem item)
        {
            foreach (var con in Connectors)
            {
                if (con.OnPositionUpdated != null)
                    con.OnPositionUpdated(con);
            }
        }

    }
}
