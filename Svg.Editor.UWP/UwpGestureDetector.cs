using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using SkiaSharp.Views.UWP;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;

namespace Svg.Editor.UWP
{
    public class UwpGestureDetector : IGestureDetector
    {
        private readonly Subject<UserInputEvent> _gesturesSubject = new Subject<UserInputEvent>();
        public IObservable<UserInputEvent> DetectedGestures => _gesturesSubject.AsObservable();

        /// <summary>Dictionary to maintain information about each active contact. 
        /// An entry is added during PointerPressed/PointerEntered events and removed 
        /// during PointerReleased/PointerCaptureLost/PointerCanceled/PointerExited events.</summary>
        Dictionary<uint, Pointer> _contacts = new Dictionary<uint, Pointer>();

        private UIElement _control;

        public UwpGestureDetector(UIElement control)
        {
            _control = control;
        }

        public void OnTouch(PointerRoutedEventArgs args)
        {
            UserInputEvent uie = null;

            var x = 0.0;
            var y = 0.0;

            var pointerPoint = args.GetCurrentPoint(_control);
            _contacts[args.Pointer.PointerId] = args.Pointer;
            var pointerCount = _contacts.Count;
            foreach (var pointer in _contacts.Values)
            {
                x += pointerPoint.Position.X / pointerCount;
                y += pointerPoint.Position.Y / pointerCount;
            }

            //var action = (int) ev.Action;
            //var maskedAction = action & (int) MotionEventActions.Mask;
            //switch (maskedAction)
            //{
            //    case (int) MotionEventActions.Down:
            //    case (int) MotionEventActions.Pointer1Down:
            //        uie = new PointerEvent(EventType.PointerDown,
            //            Engine.Factory.CreatePointF(_pointerDownX, _pointerDownY),
            //            Engine.Factory.CreatePointF(_lastTouchX, _lastTouchY),
            //            Engine.Factory.CreatePointF(x, y), ev.PointerCount);

            //        _lastTouchX = x;
            //        _lastTouchY = y;

            //        _pointerDownX = x;
            //        _pointerDownY = y;

            //        ActivePointerId = ev.GetPointerId(0);
            //        break;

            //    case (int) MotionEventActions.Up:
            //        ActivePointerId = InvalidPointerId;
            //        uie = new PointerEvent(EventType.PointerUp,
            //            Engine.Factory.CreatePointF(_pointerDownX, _pointerDownY),
            //            Engine.Factory.CreatePointF(_lastTouchX, _lastTouchY),
            //            Engine.Factory.CreatePointF(x, y), ev.PointerCount);
            //        break;

            //    case (int) MotionEventActions.Cancel:
            //        ActivePointerId = InvalidPointerId;
            //        uie = new PointerEvent(EventType.Cancel,
            //            Engine.Factory.CreatePointF(_pointerDownX, _pointerDownY),
            //            Engine.Factory.CreatePointF(_lastTouchX, _lastTouchY),
            //            Engine.Factory.CreatePointF(x, y), 1);
            //        break;

            //    case (int) MotionEventActions.Move:
            //        var relativeDeltaX = x - _lastTouchX;
            //        var relativeDeltaY = y - _lastTouchY;

            //        uie = new MoveEvent(Engine.Factory.CreatePointF(_pointerDownX, _pointerDownY),
            //            Engine.Factory.CreatePointF(_lastTouchX, _lastTouchY),
            //            Engine.Factory.CreatePointF(x, y),
            //            Engine.Factory.CreatePointF(relativeDeltaX, relativeDeltaY),
            //            ev.PointerCount);

            //        _lastTouchX = x;
            //        _lastTouchY = y;
            //        break;

            //    case (int) MotionEventActions.PointerUp:

            //        var pointerIndex2 = ((int) ev.Action & (int) MotionEventActions.PointerIndexMask)
            //                            >> (int) MotionEventActions.PointerIndexShift;

            //        var pointerId = ev.GetPointerId(pointerIndex2);
            //        if (pointerId == ActivePointerId)
            //        {
            //            // This was our active pointer going up. Choose a new
            //            // active pointer and adjust accordingly.
            //            var newPointerIndex = pointerIndex2 == 0 ? 1 : 0;
            //            x = ev.GetX(newPointerIndex);
            //            y = ev.GetY(newPointerIndex);
            //            uie = new PointerEvent(EventType.PointerUp,
            //                Engine.Factory.CreatePointF(_pointerDownX, _pointerDownY),
            //                Engine.Factory.CreatePointF(_lastTouchX, _lastTouchY),
            //                Engine.Factory.CreatePointF(x, y), 1);

            //            _lastTouchX = x;
            //            _lastTouchY = y;
            //            ActivePointerId = ev.GetPointerId(newPointerIndex);
            //        }
            //        else
            //        {
            //            var tempPointerIndex = ev.FindPointerIndex(ActivePointerId);
            //            x = ev.GetX(tempPointerIndex);
            //            y = ev.GetY(tempPointerIndex);
            //            uie = new PointerEvent(EventType.PointerUp,
            //                Engine.Factory.CreatePointF(_pointerDownX, _pointerDownY),
            //                Engine.Factory.CreatePointF(_lastTouchX, _lastTouchY),
            //                Engine.Factory.CreatePointF(x, y), 1);

            //            _lastTouchX = ev.GetX(tempPointerIndex);
            //            _lastTouchY = ev.GetY(tempPointerIndex);
            //        }

            //        break;
            //}

            if (uie != null)
                _gesturesSubject.OnNext(uie);
        }
    }
}
