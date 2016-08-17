using Android.Graphics;

namespace Svg.Platform
{
    public class AndroidPen : Pen
    {
        private readonly AndroidBrushBase _brush;
        private DashPathEffect _dashes;
        private float[] _dashPattern;
        private LineJoin _lineJoin;
        private float _miterLimit;
        private LineCap _cap;

        public AndroidPen(Brush brush, float strokeWidth)
        {
            _brush = (AndroidBrushBase)brush;

            _brush.Paint.StrokeWidth = strokeWidth;
            _brush.Paint.SetStyle(Paint.Style.Stroke);
        }

        public override void Dispose()
        {
            _brush?.Dispose();
            _dashes?.Dispose();
        }

        public override float[] DashPattern
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
                    _brush.Paint.SetPathEffect(_dashes);
                }
            }
        }

        public override float DashOffset { get; set; }

        public override LineJoin LineJoin
        {
            get { return _lineJoin; }
            set
            {
                _lineJoin = value;

                switch (value)
                {
                    case LineJoin.Bevel:
                        _brush.Paint.StrokeJoin = Paint.Join.Bevel;
                        break;
                    case LineJoin.Miter:
                        _brush.Paint.StrokeJoin = Paint.Join.Miter;
                        break;
                    case LineJoin.MiterClipped:
                        _brush.Paint.StrokeJoin = Paint.Join.Miter;
                        break;
                    case LineJoin.Round:
                        _brush.Paint.StrokeJoin = Paint.Join.Round;
                        break;
                }
            }
        }

        public override float MiterLimit
        {
            get { return _miterLimit; }
            set
            {
                _miterLimit = value;
                _brush.Paint.StrokeMiter = MiterLimit;
            }
        }

        public override LineCap StartCap
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
                _brush.Paint.StrokeCap = cap;
            }
        }

        public override LineCap EndCap
        {
            get { return StartCap; }
            set { StartCap = value; }
        }

        public Paint Paint
        {
            get { return _brush.Paint; }
        }

        public float TextSize
        {
            get { return _brush.Paint.TextSize; }
            set { _brush.Paint.TextSize = value; }
        }

        public Paint.Align TextAlign
        {
            get { return _brush.Paint.TextAlign; }
            set { _brush.Paint.TextAlign = value; }
        }

        public Paint.Style Style
        {
            get { return _brush.Paint.GetStyle(); }
            set { _brush.Paint.SetStyle(value);}
        }
    }
}