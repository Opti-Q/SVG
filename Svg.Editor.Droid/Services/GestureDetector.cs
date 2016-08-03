using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Svg.Core.Events;

namespace Svg.Droid.Editor.Services
{
    public class GestureDetector
    {
        private readonly Func<UserInputEvent, Task> _callback;
        public const int InvalidPointerId = -1;
        public int ActivePointerId = InvalidPointerId;

        private float _lastTouchX;
        private float _lastTouchY;

        private bool _scaleInProgress = false;
        private readonly ScaleGestureDetector _scaleDetector;
        private readonly RotateDetector _rotateDetector;
        private float _pointerDownX;
        private float _pointerDownY;

        public GestureDetector(Context ctx, Func<UserInputEvent, Task> callback)
        {
            _callback = callback;
            _scaleDetector = new ScaleGestureDetector(ctx, new ScaleDetector(this));
            _rotateDetector = new RotateDetector(ctx, this);
        }

        public async Task OnTouch(MotionEvent ev)
        {
            // detectors always have priority
            _scaleDetector.OnTouchEvent(ev);
            _rotateDetector.OnTouchEvent(ev);

            if (_scaleInProgress)
                return;

            UserInputEvent uie = null;

            var x = ev.GetX();
            var y = ev.GetY();

            int action = (int) ev.Action;
            switch (action & (int) MotionEventActions.Mask)
            {
                case (int) MotionEventActions.Down:
                    uie = new PointerEvent(EventType.PointerDown,
                        Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY),
                        Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY),
                        Svg.Factory.Instance.CreatePointF(x, y));
                    _lastTouchX = x;
                    _lastTouchY = y;
                    _pointerDownX = x;
                    _pointerDownY = y;
                    ActivePointerId = ev.GetPointerId(0);
                    break;

                case (int) MotionEventActions.Up:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.PointerUp,
                        Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY),
                        Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY),
                        Svg.Factory.Instance.CreatePointF(x, y));
                    break;

                case (int) MotionEventActions.Cancel:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.Cancel,
                        Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY),
                        Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY),
                        Svg.Factory.Instance.CreatePointF(x, y));
                    break;

                case (int) MotionEventActions.Move:
                    var pointerIndex = ev.FindPointerIndex(ActivePointerId);
                    x = ev.GetX(pointerIndex);
                    y = ev.GetY(pointerIndex);

                    var relativeDeltaX = x - _lastTouchX;
                    var relativeDeltaY = y - _lastTouchY;

                    //System.Diagnostics.Debug.WriteLine($"{absoluteDeltaX}:{absoluteDeltaY}");

                    uie = new MoveEvent(Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY),
                        Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY),
                        Svg.Factory.Instance.CreatePointF(x, y),
                        Svg.Factory.Instance.CreatePointF(relativeDeltaX, relativeDeltaY));

                    _lastTouchX = x;
                    _lastTouchY = y;

                    break;

                case (int) MotionEventActions.PointerUp:

                    int pointerIndex2 = ((int) ev.Action & (int) MotionEventActions.PointerIndexMask)
                                        >> (int) MotionEventActions.PointerIndexShift;

                    int pointerId = ev.GetPointerId(pointerIndex2);
                    if (pointerId == ActivePointerId)
                    {
                        // This was our active pointer going up. Choose a new
                        // active pointer and adjust accordingly.
                        int newPointerIndex = pointerIndex2 == 0 ? 1 : 0;
                        x = ev.GetX(newPointerIndex);
                        y = ev.GetY(newPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp,
                            Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY),
                            Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY),
                            Svg.Factory.Instance.CreatePointF(x, y));

                        _lastTouchX = x;
                        _lastTouchY = y;
                        ActivePointerId = ev.GetPointerId(newPointerIndex);
                    }
                    else
                    {
                        int tempPointerIndex = ev.FindPointerIndex(ActivePointerId);
                        x = ev.GetX(tempPointerIndex);
                        y = ev.GetY(tempPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp,
                            Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY),
                            Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY),
                            Svg.Factory.Instance.CreatePointF(x, y));

                        _lastTouchX = ev.GetX(tempPointerIndex);
                        _lastTouchY = ev.GetY(tempPointerIndex);
                    }

                    break;
            }

            if (uie != null)
                await _callback(uie);

            return;
        }

        public void Reset()
        {
            _lastTouchX = 0;
            _lastTouchY = 0;

            ActivePointerId = InvalidPointerId;
        }

        private class ScaleDetector : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private readonly GestureDetector _owner;

            public ScaleDetector(GestureDetector owner)
            {
                _owner = owner;
            }

            public override bool OnScaleBegin(ScaleGestureDetector detector)
            {
                _owner._scaleInProgress = true;
                var uie = new ScaleEvent(ScaleStatus.Start, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                _owner._callback(uie);
                return true;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.Scaling, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                _owner._callback(uie);

                return true;
            }

            public override void OnScaleEnd(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.End, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                _owner._callback(uie);
                _owner._scaleInProgress = false;
            }
        }

        /// <summary>
        /// Copied from: http://stackoverflow.com/questions/10682019/android-two-finger-rotation
        /// </summary>
        private class RotateDetector
        {
            private static readonly int INVALID_POINTER_ID = -1;
            private readonly GestureDetector _owner;
            private readonly Context _ctx;
            private float _fX, _fY, _sX, _sY;
            private int _ptrId1, _ptrId2;
            private float? _startAngle = null;
            private float? _previousAngle = null;

            public RotateDetector(Context ctx, GestureDetector owner)
            {
                _ctx = ctx;
                _owner = owner;
                _ptrId1 = INVALID_POINTER_ID;
                _ptrId2 = INVALID_POINTER_ID;
            }
            
            public bool OnTouchEvent(MotionEvent ev)
            {
                int action = (int) ev.Action;
                switch (action & (int) MotionEventActions.Mask)
                {
                    case (int) MotionEventActions.Down:
                        _ptrId1 = ev.GetPointerId(ev.ActionIndex);
                        break;
                    case (int) MotionEventActions.PointerDown:
                        _ptrId2 = ev.GetPointerId(ev.ActionIndex);
                        _sX = ev.GetX(ev.FindPointerIndex(_ptrId1));
                        _sY = ev.GetY(ev.FindPointerIndex(_ptrId1));
                        _fX = ev.GetX(ev.FindPointerIndex(_ptrId2));
                        _fY = ev.GetY(ev.FindPointerIndex(_ptrId2));
                        break;
                    case (int) MotionEventActions.Move:
                        if (_ptrId1 != INVALID_POINTER_ID && _ptrId2 != INVALID_POINTER_ID)
                        {
                            float nfX, nfY, nsX, nsY;
                            nsX = ev.GetX(ev.FindPointerIndex(_ptrId1));
                            nsY = ev.GetY(ev.FindPointerIndex(_ptrId1));
                            nfX = ev.GetX(ev.FindPointerIndex(_ptrId2));
                            nfY = ev.GetY(ev.FindPointerIndex(_ptrId2));

                            var angle = AngleBetweenLines(_fX, _fY, _sX, _sY, nfX, nfY, nsX, nsY);

                            if (_startAngle == null)
                            {
                                _startAngle = angle;
                            }
                            if (_previousAngle != null)
                            {
                                var delta = (_previousAngle.Value - angle) % 360;
                                var absoluteDelta = (_startAngle.Value - angle) % 360;

                                var uie = new RotateEvent(delta, absoluteDelta);
                                _owner._callback(uie);
                            }
                            _previousAngle = angle;

                        }
                        break;
                    case (int) MotionEventActions.Up:
                        _ptrId1 = INVALID_POINTER_ID;
                        CleanUp();
                        break;
                    case (int) MotionEventActions.PointerUp:
                        _ptrId2 = INVALID_POINTER_ID;
                        CleanUp();
                        break;
                    case (int) MotionEventActions.Cancel:
                        _ptrId1 = INVALID_POINTER_ID;
                        _ptrId2 = INVALID_POINTER_ID;
                        CleanUp();
                        break;
                }
                return true;
            }

            private void CleanUp()
            {
                _startAngle = null;
                _previousAngle = null;
            }

            private float AngleBetweenLines(float fX, float fY, float sX, float sY, float nfX, float nfY, float nsX, float nsY)
            {
                float angle1 = (float) Math.Atan2((fY - sY), (fX - sX));
                float angle2 = (float) Math.Atan2((nfY - nsY), (nfX - nsX));

                float angle = ((float) RadianToDegree(angle1 - angle2))%360;
                if (angle < -180f) angle += 360.0f;
                if (angle > 180f) angle -= 360.0f;
                return angle;
            }

            private double RadianToDegree(double angle)
            {
                return angle*(180.0/Math.PI);
            }
        }
    }
}