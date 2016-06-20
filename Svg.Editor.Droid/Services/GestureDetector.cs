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
        private float _pointerDownX;
        private float _pointerDownY;

        public GestureDetector(Context ctx, Func<UserInputEvent, Task> callback)
        {
            _callback = callback;
            _scaleDetector = new ScaleGestureDetector(ctx, new ZoomDetector(this));
        }

        public async Task OnTouch(MotionEvent ev)
        {
            // detectors always have priority
            _scaleDetector.OnTouchEvent(ev);

            if (_scaleInProgress)
                return;

            UserInputEvent uie = null;

            var x = ev.GetX();
            var y = ev.GetY();

            int action = (int)ev.Action;
            switch (action & (int)MotionEventActions.Mask)
            {
                case (int)MotionEventActions.Down:
                    uie = new PointerEvent(EventType.PointerDown, Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY), Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY), Svg.Factory.Instance.CreatePointF(x, y));
                    _lastTouchX = x;
                    _lastTouchY = y;
                    _pointerDownX = x;
                    _pointerDownY = y;
                    ActivePointerId = ev.GetPointerId(0);
                    break;

                case (int)MotionEventActions.Up:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.PointerUp, Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY), Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY), Svg.Factory.Instance.CreatePointF(x, y));
                    break;

                case (int)MotionEventActions.Cancel:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.Cancel, Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY), Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY), Svg.Factory.Instance.CreatePointF(x, y));
                    break;

                case (int)MotionEventActions.Move:
                    var pointerIndex = ev.FindPointerIndex(ActivePointerId);
                    x = ev.GetX(pointerIndex);
                    y = ev.GetY(pointerIndex);
                    
                    var relativeDeltaX = x - _lastTouchX;
                    var relativeDeltaY = y - _lastTouchY;

                    //System.Diagnostics.Debug.WriteLine($"{absoluteDeltaX}:{absoluteDeltaY}");

                    uie = new MoveEvent(Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY), Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY), Svg.Factory.Instance.CreatePointF(x, y), Svg.Factory.Instance.CreatePointF(relativeDeltaX, relativeDeltaY));
                    
                    _lastTouchX = x;
                    _lastTouchY = y;

                    break;

                case (int)MotionEventActions.PointerUp:

                    int pointerIndex2 = ((int)ev.Action & (int)MotionEventActions.PointerIndexMask)
                            >> (int)MotionEventActions.PointerIndexShift;

                    int pointerId = ev.GetPointerId(pointerIndex2);
                    if (pointerId == ActivePointerId)
                    {
                        // This was our active pointer going up. Choose a new
                        // active pointer and adjust accordingly.
                        int newPointerIndex = pointerIndex2 == 0 ? 1 : 0;
                        x = ev.GetX(newPointerIndex);
                        y = ev.GetY(newPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp, Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY), Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY), Svg.Factory.Instance.CreatePointF(x, y));

                        _lastTouchX = x;
                        _lastTouchY = y;
                        ActivePointerId = ev.GetPointerId(newPointerIndex);
                    }
                    else
                    {
                        int tempPointerIndex = ev.FindPointerIndex(ActivePointerId);
                        x = ev.GetX(tempPointerIndex);
                        y = ev.GetY(tempPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp, Svg.Factory.Instance.CreatePointF(_pointerDownX, _pointerDownY), Svg.Factory.Instance.CreatePointF(_lastTouchX, _lastTouchY), Svg.Factory.Instance.CreatePointF(x, y));

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

        private class ZoomDetector : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private readonly GestureDetector _owner;

            public ZoomDetector(GestureDetector owner)
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
    }
}