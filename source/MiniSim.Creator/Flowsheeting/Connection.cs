
using System;
using System.ComponentModel;
using System.Windows;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using Caliburn.Micro;
using System.Collections.Generic;

namespace MiniSim.Creator.Flowsheeting
{
    [Serializable]
    public class Connection : PropertyChangedBase
    {
        #region Fields

        private string _name;
        private Connector _sink;
        private Connector _source;
        private System.Windows.Point _labelPoint;
        private bool _isSelected = false;

        double _thickness = 3.0;
        string _color = "DimGray";
        string _dashArray = "1,0";
        MaterialStream _modelInstance;
        string _report = "";

        #endregion

        #region Properties
        public Connector Source
        {
            get { return _source; }
            set
            {

                if (_source != value)
                {
                    if (_source != null)
                    {
                        _source.OnPositionUpdated -= onPositionUpdated;
                    }
                }
                _source = value;

                if (_source != null)
                {
                    _source.OnPositionUpdated += onPositionUpdated;

                }

                UpdatePathGeometry();

                NotifyOfPropertyChange(() => Source);
            }
        }

        public Connector Sink
        {
            get { return _sink; }
            set
            {

                if (_sink != value)
                {
                    if (_sink != null)
                    {
                        _sink.OnPositionUpdated -= onPositionUpdated;
                    }
                }
                _sink = value;

                if (_sink != null)
                {
                    _sink.OnPositionUpdated += onPositionUpdated;

                }

                UpdatePathGeometry();
                NotifyOfPropertyChange(() => Sink);

            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => Name); }
        }

        public Variable Temperature
        {
            get
            {
                return _modelInstance?.GetVariable("T");
            }
        }

        public Variable Pressure
        {
            get
            {
                return _modelInstance?.GetVariable("P");
            }

        }

        public Variable Massflow
        {
            get
            {
                return _modelInstance?.GetVariable("m");
            }

        }
        public Variable VapourFraction
        {
            get
            {
                return _modelInstance?.GetVariable("VF");
            }

        }

        public Point LabelPoint
        {
            get
            {
                return _labelPoint;
            }

            set
            {
                _labelPoint = value; NotifyOfPropertyChange(() => LabelPoint);
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value; NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public double Thickness
        {
            get
            {
                return _thickness;
            }

            set
            {
                _thickness = value; NotifyOfPropertyChange(() => Thickness);
            }
        }

        public string Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value; NotifyOfPropertyChange(() => Color);
            }
        }



        public string DashArray
        {
            get
            {
                return _dashArray;
            }

            set
            {
                _dashArray = value;
                NotifyOfPropertyChange(() => DashArray);
            }
        }

        public MaterialStream ModelInstance
        {
            get => _modelInstance;
            set
            {
                _modelInstance = value;
                NotifyOfPropertyChange(() => ModelInstance);
                NotifyOfPropertyChange(() => Temperature);
                NotifyOfPropertyChange(() => Pressure);
                NotifyOfPropertyChange(() => VapourFraction);
                NotifyOfPropertyChange(() => Massflow);
            }
        }

        public string Report { get => _report;
            set { _report = value;
                NotifyOfPropertyChange(() => Report);
                NotifyOfPropertyChange(() => Temperature);
                NotifyOfPropertyChange(() => Pressure);
                NotifyOfPropertyChange(() => VapourFraction);
                NotifyOfPropertyChange(() => Massflow);
            } }

        #endregion

        #region Public Methods

        [NonSerialized]
        System.Windows.Media.PolyLineSegment path = new System.Windows.Media.PolyLineSegment();

        public void Select()
        {
            IsSelected = true;
        }
        public System.Windows.Media.PolyLineSegment Path
        {
            get { return path; }
            set { path = value; NotifyOfPropertyChange(() => Path); }
        }



        public virtual void UpdatePathGeometry()
        {
            if (Path == null)
                Path = new System.Windows.Media.PolyLineSegment();

            if (Source != null && Sink != null)
            {
                if (Source.Owner != null && Sink.Owner != null)
                {

                    System.Windows.Point startPoint = new System.Windows.Point(Source.Owner.X + Source.X + 5, Source.Owner.Y + Source.Y + 5);
                    System.Windows.Point endPoint = new System.Windows.Point(Sink.Owner.X + Sink.X + 5, Sink.Owner.Y + Sink.Y + 5);

                    Path.Points.Clear();

                    double startX = 0, startY = 0, endX = 0, endY = 0, startXO = 0, startYO = 0, endXO = 0, endYO = 0;

                    startX = Source.Owner.X + Source.X + Source.Width / 2.0;
                    startY = Source.Owner.Y + Source.Y + Source.Width / 2.0;

                    endX = Sink.Owner.X + Sink.X + Sink.Width / 2.0;
                    endY = Sink.Owner.Y + Sink.Y + Sink.Width / 2.0;


                    var sourceNormal = Source.Direction;
                    var sinkNormal = Sink.Direction;

                    int relationalType = 0;

                    if (startX < endX && startY >= endY)
                        relationalType = 1;

                    if (startX < endX && startY < endY)
                        relationalType = 2;


                    if (startX > endX && startY > endY)
                        relationalType = 3;

                    if (startX > endX && startY < endY)
                        relationalType = 4;

                    var pointList = new List<Point>();
                    var offset = 20;

                    //Source Connector+ Normal
                    switch (sourceNormal)
                    {
                        case PortNormal.Up:
                            startXO = startX;
                            startYO = startY - offset;
                            break;
                        case PortNormal.Right:
                            startXO = startX + offset;
                            startYO = startY;
                            break;
                        case PortNormal.Down:
                            startXO = startX;
                            startYO = startY + offset;
                            break;
                        case PortNormal.Left:
                            startXO = startX - offset;
                            startYO = startY;
                            break;
                    }
                    //Sink Connector +Normal
                    switch (sinkNormal)
                    {
                        case PortNormal.Up:
                            endXO = endX;
                            endYO = endY - offset;
                            break;
                        case PortNormal.Right:
                            endXO = endX + offset;
                            endYO = endY;
                            break;
                        case PortNormal.Down:
                            endXO = endX;
                            endYO = endY + offset;
                            break;
                        case PortNormal.Left:
                            endXO = endX - offset;
                            endYO = endY;
                            break;
                    }


                    //Source Connector
                    pointList.Add(new Point((int)startX, (int)startY));
                    pointList.Add(new Point((int)startXO, (int)startYO));

                    double centerX = startX;
                    double centerY = startY;

                    //Intermedate
                    if (sourceNormal == PortNormal.Right && sinkNormal == PortNormal.Left)
                    {
                        if (relationalType == 1 || relationalType == 2)
                        {
                            pointList.Add(new Point((int)((startXO + endXO) / 2.0), (int)startYO));
                            pointList.Add(new Point((int)((startXO + endXO) / 2.0), (int)endYO));

                            centerX = (startXO + endXO) / 2.0;
                            centerY = (startYO + endYO) / 2.0;
                        }
                    }
                    else if (sourceNormal == PortNormal.Right && sinkNormal == PortNormal.Right)
                    {
                        if (relationalType == 1)
                        {
                            pointList.Add(new Point((int)((endXO)), (int)startYO));

                            centerX = (endXO);
                            centerY = (startYO);
                        }

                        if (relationalType == 2)
                        {
                            pointList.Add(new Point((int)((endXO)), (int)startYO));

                            centerX = (endXO);
                            centerY = (startYO);
                        }


                        if (relationalType == 3)
                        {
                            pointList.Add(new Point((int)((startXO)), (int)endYO));

                            centerX = (startXO);
                            centerY = (endYO);
                        }


                        if (relationalType == 4)
                        {
                            pointList.Add(new Point((int)((startXO)), (int)endYO));

                            centerX = (startXO);
                            centerY = (endYO);
                        }
                    }
                    else if (sourceNormal == PortNormal.Up && sinkNormal == PortNormal.Left)
                    {
                        if (relationalType == 1)
                        {
                            pointList.Add(new Point((int)startXO, (int)endYO));
                            centerX = (startXO);
                            centerY = (endYO);
                        }

                        if (relationalType == 2)
                        {
                            pointList.Add(new Point((int)endXO, (int)startYO));
                            // pointList.Add(new Point((int)startXO, (int)endYO));

                            centerX = (endXO);
                            centerY = (startYO);
                        }

                    }
                    else if (sourceNormal == PortNormal.Up && sinkNormal == PortNormal.Right)
                    {
                        if (relationalType == 3)
                        {
                            pointList.Add(new Point((int)startXO, (int)endYO));

                            centerX = (startXO);
                            centerY = (endYO);
                        }

                        if (relationalType == 4)
                        {
                            pointList.Add(new Point((int)endXO, (int)startYO));

                            centerX = (endXO);
                            centerY = (startYO);
                        }

                    }
                    else if (sourceNormal == PortNormal.Left && sinkNormal == PortNormal.Left)
                    {
                        if (relationalType == 3 || relationalType == 4)
                        {
                            pointList.Add(new Point((int)((endXO)), (int)startYO));

                            centerX = (endXO);
                            centerY = (startYO);
                        }
                    }
                    else if (sourceNormal == PortNormal.Down && sinkNormal == PortNormal.Left)
                    {
                        if (relationalType == 1)
                        {
                            pointList.Add(new Point((int)((startXO + endXO) / 2.0), (int)startYO));
                            pointList.Add(new Point((int)((startXO + endXO) / 2.0), (int)endYO));
                            centerX = (startXO + endXO) / 2.0;
                            centerY = (startYO + endYO) / 2.0;
                        }
                        if (relationalType == 2)
                        {
                            pointList.Add(new Point((int)startXO, (int)endYO));
                            centerX = (startXO);
                            centerY = (endYO);
                        }

                        if (relationalType == 3)
                        {
                            pointList.Add(new Point((int)((endXO)), (int)startYO));
                            //pointList.Add(new System.Drawing.Point((int)((endXO) / 2.0), (int)endYO));

                            centerX = (startXO + endXO) / 2.0;
                            centerY = (startYO);
                        }

                        if (relationalType == 4)
                        {
                            pointList.Add(new Point((int)((endXO)), (int)startYO));

                            centerX = (startXO + endXO) / 2.0;
                            centerY = (startYO);
                        }

                    }
                    else if (sourceNormal == PortNormal.Down && sinkNormal == PortNormal.Right)
                    {
                        if (relationalType == 1)
                        {
                            pointList.Add(new Point((int)endXO, (int)startYO));
                            centerX = (endXO);
                            centerY = (startYO);
                        }

                        if (relationalType == 2)
                        {
                            pointList.Add(new Point((int)startXO, (int)((startYO + endYO) / 2)));
                            pointList.Add(new Point((int)endXO, (int)((startYO + endYO) / 2)));
                            centerX = (int)((startXO + endXO) / 2);
                            centerY = (int)((startYO + endYO) / 2);
                        }

                        if (relationalType == 3)
                        {
                            pointList.Add(new Point((int)endXO, (int)startYO));
                            centerX = (int)((startXO + endXO) / 2.0);
                            centerY = (startYO);
                        }

                        if (relationalType == 4)
                        {
                            pointList.Add(new Point((int)startXO, (int)endYO));
                            centerX = (startXO);
                            centerY = (endYO);
                        }

                    }
                    else
                    {
                        pointList.Add(new Point((int)((startXO + endXO) / 2.0), (int)startYO));
                        pointList.Add(new Point((int)((startXO + endXO) / 2.0), (int)endYO));
                        centerX = (startXO + endXO) / 2.0;
                        centerY = (startYO + endYO) / 2.0;
                    }

                    pointList.Add(new Point((int)endXO, (int)endYO));
                    //Sink Connector
                    pointList.Add(new Point((int)endX, (int)endY));

                    foreach (var p in pointList)
                        Path.Points.Add(p);

                    LabelPoint = new Point(centerX - 20, centerY - 10);


                    DrawArrowPoint();

                    NotifyOfPropertyChange(() => Path);
                    NotifyOfPropertyChange(() => LabelPoint);
                }

                if (Source.Owner != null && Sink.Owner == null)
                {
                    System.Windows.Point startPoint = new Point(Source.Owner.X + Source.X + 5, Source.Owner.Y + Source.Y + 5);
                    System.Windows.Point endPoint = new Point(Sink.X + 5, Sink.Y + 5);

                    Path.Points.Clear();
                    Path.Points.Add(startPoint);
                    Path.Points.Add(endPoint);
                    LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);

                    NotifyOfPropertyChange(() => Path);
                    NotifyOfPropertyChange(() => LabelPoint);

                }



            }
        }
        protected void onPositionUpdated(DrawableItem sender)
        {
            UpdatePathGeometry();
        }

        void DrawArrowPoint()
        {
            System.Windows.Point endPoint = new System.Windows.Point(Sink.Owner.X + Sink.X + 5, Sink.Owner.Y + Sink.Y + 5);
            double headSize = 5;

            if (Sink.Direction == PortNormal.Up)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X - headSize, endPoint.Y - 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X + headSize, endPoint.Y - 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }

            if (Sink.Direction == PortNormal.Down)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X - headSize, endPoint.Y + 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X + headSize, endPoint.Y + 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }

            if (Sink.Direction == PortNormal.Left)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X - 2 * headSize, endPoint.Y - headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X - 2 * headSize, endPoint.Y + headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }
            if (Sink.Direction == PortNormal.Right)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X + 2 * headSize, endPoint.Y - headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X + 2 * headSize, endPoint.Y + headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }
        }


        #endregion

    }
}
