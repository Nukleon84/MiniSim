using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public enum PortDirection { In, Out, Both };
    public enum PortNormal { Up,Right,Down,Left };

    public class Port<T> where T : BaseStream
    {

        PortDirection _direction = PortDirection.In;
        PortNormal _normal = PortNormal.Right;
        List<T> _streams = new List<T>();
        bool _isConnected = false;
        int _multiplicity = 1;
        string _name;
        bool _isOptional = true;
        double _widthFraction = 0.0;
        double _heightFraction = 0.0;

        public int NumberOfStreams
        {
            get { return _streams.Count; }
        }

        public int Multiplicity
        {
            get
            {
                return _multiplicity;
            }

            set
            {
                _multiplicity = value;
            }
        }

        public PortDirection Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
            }
        }

        public List<T> Streams
        {
            get
            {
                return _streams;
            }

            set
            {
                _streams = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            set
            {
                _isConnected = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public bool IsOptional
        {
            get
            {
                return _isOptional;
            }

            set
            {
                _isOptional = value;
            }
        }

        public double WidthFraction { get => _widthFraction; set => _widthFraction = value; }
        public double HeightFraction { get => _heightFraction; set => _heightFraction = value; }
        public PortNormal Normal { get => _normal; set => _normal = value; }

        public Port(string name, PortDirection direction, int multiplicty)
        {
            Name = name;
            Direction = direction;
            Multiplicity = multiplicty;

        }
        public void Connect(T stream)
        {
            if (Streams.Count < Multiplicity || Multiplicity == -1)
            {
                if (!Streams.Contains(stream))
                {
                    IsConnected = true;
                    Streams.Add(stream);
                }
                else
                    throw new InvalidOperationException("Stream " + stream.Name + " already connected to port " + Name);
            }
            else
                throw new InvalidOperationException("No more streams allowed for port " + Name);
        }
    }
}
