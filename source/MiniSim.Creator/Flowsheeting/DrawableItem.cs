using Caliburn.Micro;
using System;
using System.ComponentModel;

namespace MiniSim.Creator.Flowsheeting

{
    /// <summary>
    /// Base class for persistent drawable items on a flowsheet diagram like streams, connectors and units
    /// </summary>        
    public class DrawableItem : PropertyChangedBase
    {
        #region Fields
        bool _isSelected = false;
        bool _isLabelVisible = true;

        string _name;
        public double x;
        public double y;
        double _width = 100;
        double _height = 50;
        string _type;
        private string _fillColor = "White";
        private string _borderColor = "DimGray";

        [field: NonSerialized]
        Action<DrawableItem> _onPositionUpdated;
        
        public Action<DrawableItem> OnPositionUpdated
        {
            get { return _onPositionUpdated; }
            set { _onPositionUpdated = value; NotifyOfPropertyChange(() => OnPositionUpdated);  }
        }
        #endregion

        #region Properties

        public virtual Guid Id { get; set; }
  

        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyOfPropertyChange(() => IsSelected);  }
        }
        [Category("Information")]
        [DisplayName("Name")]
        [Description("The name of the unit.")]
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => Name); }
        }
        public virtual string Type
        {
            get { return _type; }
            set { _type = value; NotifyOfPropertyChange(() => Type);  }
        }
        [Category("Graphics")]
        [DisplayName("X")]
        [Description("The X position of the unit on the flowshet")]
        public virtual double X
        {
            get { return x; }
            set { x = value; NotifyOfPropertyChange(() => X); OnPositionUpdated?.Invoke(this); }
        }
        [Category("Graphics")]
        [DisplayName("Y")]
        [Description("The Y position of the unit on the flowshet")]
        public virtual double Y
        {
            get { return y; }
            set { y = value; NotifyOfPropertyChange(() => Y); OnPositionUpdated?.Invoke(this); }
        }
        [Category("Graphics")]
        [DisplayName("Width")]
        [Description("The width of the unit on the flowshet")]
        public virtual double Width
        {
            get { return _width; }
            set { _width = value; NotifyOfPropertyChange(() => Width);  }
        }
        [Category("Graphics")]
        [DisplayName("Height")]
        [Description("The height of the unit on the flowshet")]
        public virtual double Height
        {
            get { return _height; }
            set { _height = value; NotifyOfPropertyChange(() => Height);  }
        }
        [Category("Graphics")]
        [DisplayName("Fill Color")]
        [Description("The fill color of the unit on the flowshet")]
        public string FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; NotifyOfPropertyChange(() => FillColor); }
        }
        [Category("Graphics")]
        [DisplayName("Border Color")]
        [Description("The border color of the unit on the flowshet")]
        public string BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; NotifyOfPropertyChange(() => BorderColor);  }
        }

        public bool IsLabelVisible
        {
            get
            {
                return _isLabelVisible;
            }

            set
            {
                _isLabelVisible = value;
                NotifyOfPropertyChange(() => IsLabelVisible);
              
            }
        }

        #endregion

      
    }
}
