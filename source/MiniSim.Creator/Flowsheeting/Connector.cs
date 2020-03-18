using MiniSim.Core.Flowsheeting;
using System;

namespace MiniSim.Creator.Flowsheeting
{
    /// <summary>
    /// Class for serialization of connectors. 
    /// </summary>    
    public class Connector : DrawableItem
    {
        #region Fields
        VisualUnit _owner;
        bool _isConnected = false;
        PortDirection _intent = PortDirection.In;
        PortNormal _direction = PortNormal.Right;
        Connection _connection;

        #endregion

        #region Properties
        public virtual bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; NotifyOfPropertyChange(() => IsConnected);  }
        }

        public virtual VisualUnit Owner
        {
            get { return _owner; }
            set { _owner = value; NotifyOfPropertyChange(() => Owner);  }
        }

     

        public PortNormal Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value; NotifyOfPropertyChange(() => Direction); 
            }
        }

        public PortDirection Intent
        {
            get
            {
                return _intent;
            }

            set
            {
                _intent = value; NotifyOfPropertyChange(() => Intent);
            }
        }

        public Connection Connection { get => _connection; set => _connection = value; }
        #endregion

        public Connector()
        {
            this.Width = 10;
            this.Height = 10;
        }

        public override string ToString()
        {
            return Owner?.Name+"."+Name;
        }
    }
}
