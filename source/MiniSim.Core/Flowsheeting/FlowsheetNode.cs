using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public class FlowsheetNode
    {
        string _name;
        public string Name { get => _name; set => _name = value; }
        FlowsheetIcon _icon = new FlowsheetIcon();

        public FlowsheetIcon Icon
        {
            get
            {
                return _icon;
            }

            set
            {
                _icon = value;
            }
        }

        public FlowsheetNode SetIcon(IconTypes type, double x, double y)
        {
            Icon.IconType = type;
            Icon.X = x;
            Icon.Y = y;


            switch (Icon.IconType)
            {
                case IconTypes.Stream:
                    Icon.Width = 80;
                    Icon.Height = 40;                  
                    break;
                case IconTypes.Splitter:
                case IconTypes.Mixer:
                    Icon.Width = 40;
                    Icon.Height = 40;                  
                    break;

                case IconTypes.ColumnSection:
                    Icon.Width = 40;
                    Icon.Height = 200;                   
                    break;
                case IconTypes.Heater:
                case IconTypes.TwoPhaseFlash:
                    Icon.Width = 40;
                    Icon.Height = 40;                   
                    break;
            }
            return this;
        }

        public FlowsheetNode SetPosition(double x, double y)
        {
            Icon.X = x;
            Icon.Y = y;
            return this;
        }
        public FlowsheetNode SetSize(double width, double height)
        {
            Icon.Width = width;
            Icon.Height = height;
            return this;
        }
        public FlowsheetNode SetColors(string border, string fill)
        {
            Icon.BorderColor = border;
            Icon.FillColor = fill;
            return this;
        }
    }
}
