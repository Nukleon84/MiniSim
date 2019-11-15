using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting.Documentation
{
    public class TextElement:DocumentationElement
    {
        string _text;
        string _fontColor = "Black";
        string _horizontalTextAlignment = "Center";
        string _verticalTextAlignment = "Center";

        double _fontSize = 12.0;

        

        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
            }
        }

        public double FontSize
        {
            get
            {
                return _fontSize;
            }

            set
            {
                _fontSize = value;
            }
        }

        public string FontColor
        {
            get
            {
                return _fontColor;
            }

            set
            {
                _fontColor = value;
            }
        }

        public string HorizontalTextAlignment
        {
            get
            {
                return _horizontalTextAlignment;
            }

            set
            {
                _horizontalTextAlignment = value;
            }
        }

        public string VerticalTextAlignment
        {
            get
            {
                return _verticalTextAlignment;
            }

            set
            {
                _verticalTextAlignment = value;
            }
        }

        public TextElement(string text, double fontSize, string fontColor)
        {
            Text = text;
            FontSize = fontSize;
            FontColor = fontColor;
            Icon.IconType = IconTypes.Text;
            Icon.BorderColor = "White";
            Icon.FillColor = "White";
        }
    }
}
