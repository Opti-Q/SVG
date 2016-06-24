using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Svg.Interfaces;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Platform
{
    public class AndroidGraphicsPath : GraphicsPath
    {
        private FillMode _fillmode;
        private readonly List<PointF> _points = new List<PointF>();
        private readonly List<byte> _pathTypes = new List<byte>();
        private Android.Graphics.Path _path;
        private List<TextInfo> _texts;
        private RectangleF _bounds;

        public AndroidGraphicsPath()
        {
            _path = new Android.Graphics.Path();
        }


        public AndroidGraphicsPath(Android.Graphics.Path path)
        {
            _path = path;
        }

        public AndroidGraphicsPath(FillMode fillmode)
        {
            FillMode = fillmode;
        }


        public void Dispose()
        {
            if (_path != null)
            {
                _path.Dispose();
                _path = null;
            }
        }

        public RectangleF GetBounds()
        {
            if (_bounds == null)
            {
                var r = new RectF();
                _path.ComputeBounds(r, true);
                return Engine.Factory.CreateRectangleF(r.Left, r.Top, r.Width(), r.Height());
            }

            return _bounds;
        }

        public void StartFigure()
        {
            _bounds = null;
        }
        public void CloseFigure()
        {
            _bounds = null;
            Path.Close();
        }

        public decimal PointCount { get { return _points.Count; } }
        public PointF[] PathPoints { get { return _points.ToArray(); } }
        public FillMode FillMode
        {
            get { return _fillmode; }
            set
            {
                _fillmode = value;

                switch (_fillmode)
                {
                    case FillMode.Alternate:
                        Path.SetFillType(Path.FillType.EvenOdd);
                        break;
                    case FillMode.Winding:
                        Path.SetFillType(Path.FillType.Winding);
                        break;
                }
            }
        }

        /// <summary>
        /// see: https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.graphicspath.pathtypes%28v=vs.110%29.aspx
        /// </summary>
        public byte[] PathTypes
        {
            get
            {
                return _pathTypes.ToArray();
            }
            set
            {
                _pathTypes.Clear();
                _pathTypes.AddRange(value);
            }
        }

        public PathData PathData
        {
            get
            {
                return new PathData(PathPoints, PathTypes);
            }
        }

        public Path Path
        {
            get { return _path; }
        }

        internal List<TextInfo> Texts
        {
            get { return _texts ?? (_texts = new List<TextInfo>()); }
        }

        public void AddEllipse(float x, float y, float width, float height)
        {
            _bounds = null;
            // TODO LX: Which direction is correct?
            Path.AddOval(new RectF(x, y, x + width, y + height), Path.Direction.Cw);

            _points.Add(Engine.Factory.CreatePointF(x, y));
            _points.Add(Engine.Factory.CreatePointF(x + width, y + height));
            _pathTypes.Add(0); // start of a figure
            _pathTypes.Add(0x80); // last point in closed sublath
        }

        public void MoveTo(PointF start)
        {
            _bounds = null;
            Path.MoveTo(start.X, start.Y);
            _points.Add(start);
            _pathTypes.Add(1); // end point of line
        }


        public void AddLine(PointF start, PointF end)
        {
            _bounds = null;
            var lp = GetLastPoint();
            if(lp == null || lp != start)
            { 
                Path.MoveTo(start.X, start.Y);
                _points.Add(start);
                _pathTypes.Add(1); // start of a line
            }

            Path.LineTo(end.X, end.Y);
            _points.Add(end);
            _pathTypes.Add(1); // end point of line
        }

        public PointF GetLastPoint()
        {
            return _points.Count == 0 ? null : _points[_points.Count - 1];
        }

        public void AddRectangle(RectangleF rectangle)
        {
            _bounds = null;
            Path.AddRect((AndroidRectangleF)rectangle, Path.Direction.Cw);
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X, rectangle.Location.Y));
            _pathTypes.Add(0); // start of a figure
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X + rectangle.Width, rectangle.Location.Y));
            _pathTypes.Add(0x7); // TODO LX: ???
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(0x7); // TODO LX: ???
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(0x80); // TODO LX: ???
        }

        public void AddArc(RectangleF rectangle, float startAngle, float sweepAngle)
        {
            _bounds = null;
            Path.AddArc((AndroidRectangleF)rectangle, startAngle, sweepAngle);

            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X, rectangle.Location.Y));
            _pathTypes.Add(1); // start point of line
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X + rectangle.Width, rectangle.Location.Y));
            _pathTypes.Add(0x20); // TODO LX: ???
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(0x20); // TODO LX: ???
            _points.Add(Engine.Factory.CreatePointF(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(1); // end point of line
        }

        public GraphicsPath Clone()
        {
            var cl = new AndroidGraphicsPath(new Path(this.Path));
            cl._points.AddRange(this._points);
            cl._pathTypes.AddRange(this._pathTypes);
            return cl;
        }

        public void Transform(Matrix transform)
        {
            _bounds = null;
            var m = new Android.Graphics.Matrix();
            m.SetValues(transform.Elements);
            Path.Transform(m);
        }

        public void AddPath(GraphicsPath childPath, bool connect)
        {
            _bounds = null;
            var ap = (AndroidGraphicsPath) childPath;
            // TODO LX: How to connect? And is 0, 0 correct?
            Path.AddPath(ap.Path, 0, 0);

            _points.AddRange(ap._points);
            _pathTypes.AddRange(ap._pathTypes);
        }

        public void AddString(string text, FontFamily fontFamily, int style, float size, PointF location,
            StringFormat createStringFormatGenericTypographic)
        {
            _bounds = null;
            // little hack as android path does not support text!
            _texts.Add(new TextInfo(text, fontFamily, style, size, location, createStringFormatGenericTypographic));
        }

        public void AddBezier(PointF start, PointF point1, PointF point2, PointF point3)
        {
            _bounds = null;
            Path.MoveTo(start.X, start.Y);
            Path.CubicTo(point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y);

            _points.AddRange(new[] { start, point1, point2, point3 });
            _pathTypes.Add(1); // start point of line
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // endpoint of cubic bezier spline
        }

        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            _bounds = null;
            Path.MoveTo(x1, y2);
            Path.CubicTo(x2, y2, x3, y3, x4, y4);

            _points.AddRange(new[] { Engine.Factory.CreatePointF(x1, y1), Engine.Factory.CreatePointF(x2, y2), Engine.Factory.CreatePointF(x3, y3), Engine.Factory.CreatePointF(x4, y4) });
            _pathTypes.Add(1); // start point of line
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // endpoint of cubic bezier spline
        }

        public bool IsVisible(PointF pointF)
        {
            RectF rect = new RectF();
            Path.ComputeBounds(rect, true);

            return rect.Contains(pointF.X, pointF.Y);
        }

        public void Flatten()
        {
            // TODO LX not supported by Android.Graphics.Path
            throw new NotSupportedException();
        }

        public void AddPolygon(PointF[] polygon)
        {
            _bounds = null;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (i == 0)
                {
                    Path.MoveTo(polygon[i].X, polygon[i].Y);
                    _points.Add(polygon[i]);
                    _pathTypes.Add(0); // start point of figure
                }
                else if (i == polygon.Length - 1)
                {
                    Path.Close();
                    _points.Add(polygon[i]);
                    _pathTypes.Add(0x80); // end point of figure
                }
                else
                {
                    Path.LineTo(polygon[i].X, polygon[i].Y);
                    _points.Add(polygon[i]);
                    _pathTypes.Add(1); // TODO LX: ???
                }
            }
        }

        public void Reset()
        {
            _bounds = null;
            Path.Reset();
        }

    
        internal class TextInfo
        {
            public string text;
            public FontFamily fontFamily;
            public int style;
            public float size;
            public PointF location;
            StringFormat createStringFormatGenericTypographic;

            public TextInfo(string text, FontFamily fontFamily, int style, float size, PointF location, StringFormat createStringFormatGenericTypographic)
            {
                this.text = text;
                this.fontFamily = fontFamily;
                this.style = style;
                this.size = size;
                this.location = location;
                this.createStringFormatGenericTypographic = createStringFormatGenericTypographic;
            }
        }
    }
}