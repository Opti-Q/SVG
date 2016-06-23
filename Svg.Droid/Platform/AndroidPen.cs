using Android.Graphics;

namespace Svg.Platform
{
    public class AndroidPen : Pen
    {
        private readonly Brush _brush;
        private Paint _paint;
        private DashPathEffect _dashes;
        private float[] _dashPattern;
        private LineJoin _lineJoin;
        private float _miterLimit;
        private LineCap _cap;
        private LineCap _endCap;

        public AndroidPen(Brush brush, float strokeWidth)
        {
            _brush = brush;
            _paint = new Paint();
            _paint.StrokeWidth = strokeWidth;
            _paint.SetStyle(Paint.Style.Stroke);
            
            var shader = (IAndroidShader) brush;
            shader.ApplyTo(this.Paint);
        }

        public void Dispose()
        {
            _paint.Dispose();
            if(_dashes != null)
                _dashes.Dispose();
        }

        public float[] DashPattern
        {
            get { return _dashPattern; }
            set
            {
                _dashPattern = value;
                if (_dashPattern == null && _dashes != null)
                {
                    _dashes.Dispose();
                    _dashes = null;
                }

                if (_dashPattern != null)
                {
                    if (_dashes != null)
                        _dashes.Dispose();
                    
                    _dashes = new DashPathEffect(_dashPattern, 0f);
                    _paint.SetPathEffect(_dashes);
                }
            }
        }

        public LineJoin LineJoin
        {
            get { return _lineJoin; }
            set
            {
                _lineJoin = value;

                switch (value)
                {
                    case LineJoin.Bevel:
                        _paint.StrokeJoin = Paint.Join.Bevel;
                        break;
                    case LineJoin.Miter:
                        _paint.StrokeJoin = Paint.Join.Miter;
                        break;
                    case LineJoin.MiterClipped:
                        _paint.StrokeJoin = Paint.Join.Miter;
                        break;
                    case LineJoin.Round:
                        _paint.StrokeJoin = Paint.Join.Round;
                        break;
                }
            }
        }

        public float MiterLimit
        {
            get { return _miterLimit; }
            set
            {
                _miterLimit = value;
                _paint.StrokeMiter = MiterLimit;
            }
        }

        public LineCap StartCap
        {
            get { return _cap; }
            set
            {
                _cap = value;
                Paint.Cap cap = Paint.Cap.Butt;

                switch (value)
                {
                    case LineCap.AnchorMask:
                    case LineCap.ArrowAnchor:
                    case LineCap.Custom:
                    case LineCap.NoAnchor:
                    case LineCap.DiamondAnchor:
                    case LineCap.Triangle:
                    case LineCap.Flat:
                        cap = Paint.Cap.Butt;
                        break;
                    case LineCap.Round:
                    case LineCap.RoundAnchor:
                        cap = Paint.Cap.Round;
                        break;
                    case LineCap.Square:
                    case LineCap.SquareAnchor:
                        cap = Paint.Cap.Square;
                        break;
                        
                }
                _paint.StrokeCap = cap;
            }
        }

        public LineCap EndCap
        {
            get { return StartCap; }
            set { StartCap = value; }
        }

        public Paint Paint
        {
            get { return _paint; }
        }

        public float TextSize
        {
            get { return _paint.TextSize; }
            set { _paint.TextSize = value; }
        }

        public Paint.Align TextAlign
        {
            get { return _paint.TextAlign; }
            set { _paint.TextAlign = value; }
        }

        public Paint.Style Style
        {
            get { return _paint.GetStyle(); }
            set { _paint.SetStyle(value);}
        }
    }
}