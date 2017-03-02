using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CoreGraphics;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using UIKit;

namespace Svg.Editor.iOS
{
    /// <summary>
    /// see: https://developer.xamarin.com/guides/ios/application_fundamentals/touch/touch_in_ios/
    /// </summary>
    public class TouchGestureDetector : IGestureDetector
    {
        private readonly UIView _owner;
        private readonly Subject<UserInputEvent> _gestureSubject = new Subject<UserInputEvent>();
        private UIPinchGestureRecognizer _zoomRecognizer;
        private UIRotationGestureRecognizer _rotationRecognizer;

        public TouchGestureDetector(UIView owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _owner = owner;
            _owner.UserInteractionEnabled = true;
            _owner.MultipleTouchEnabled = true;

            _rotationRecognizer = new UIRotationGestureRecognizer(this.OnRotate);
            _rotationRecognizer.CancelsTouchesInView = false;
            _rotationRecognizer.ShouldRecognizeSimultaneously += (r1, r2) => true;

            _zoomRecognizer = new UIPinchGestureRecognizer(this.OnZoom);
            _zoomRecognizer.CancelsTouchesInView = false;
            _zoomRecognizer.ShouldRecognizeSimultaneously += (r1, r2) => true;
            _zoomRecognizer.RequireGestureRecognizerToFail(_rotationRecognizer);
        }

        private void OnZoom(UIPinchGestureRecognizer r)
        {
            var state = r.State;
            
            if (state == UIGestureRecognizerState.Began)
            {
                var s = new ScaleEvent(ScaleStatus.Start, (float) r.Scale, 0f, 0f);
                System.Diagnostics.Debug.WriteLine($"Zoom Begin: {s}");
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Changed)
            {
                var c = new ScaleEvent(ScaleStatus.Scaling, (float) r.Scale, 0f, 0f);
                System.Diagnostics.Debug.WriteLine($"Zooming: {c}");
                _gestureSubject.OnNext(c);
            }
            else if( state == UIGestureRecognizerState.Cancelled ||
                state == UIGestureRecognizerState.Ended ||
                state ==UIGestureRecognizerState.Recognized)
            {
                var e = new ScaleEvent(ScaleStatus.End, (float)r.Scale, 0f, 0f);
                System.Diagnostics.Debug.WriteLine($"Zoom End: {e}");
                _gestureSubject.OnNext(e);
            }
        }

        private float _absoluteRotation = 0;

        private void OnRotate(UIRotationGestureRecognizer r)
        {
            var state = r.State;

            if (state == UIGestureRecognizerState.Began)
            {
                _absoluteRotation = RadianToDegree(r.Rotation);
                var relativeRotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(relativeRotation, _absoluteRotation, RotateStatus.Start, (int)r.NumberOfTouches);
                System.Diagnostics.Debug.WriteLine($"Rotate Begin: {s}");
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Changed)
            {
                _absoluteRotation += RadianToDegree(r.Rotation);
                var relativeRotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(relativeRotation, _absoluteRotation, RotateStatus.Rotating, (int)r.NumberOfTouches);
                System.Diagnostics.Debug.WriteLine($"Rotating: {s}");
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Cancelled ||
                state == UIGestureRecognizerState.Ended ||
                state == UIGestureRecognizerState.Recognized)
            {
                _absoluteRotation += RadianToDegree(r.Rotation);
                var relativeRotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(relativeRotation, _absoluteRotation, RotateStatus.End, (int)r.NumberOfTouches);
                System.Diagnostics.Debug.WriteLine($"Rotate End: {s}");
                _gestureSubject.OnNext(s);
            }
        }
        private static float RadianToDegree(double angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public IObservable<UserInputEvent> DetectedGestures => _gestureSubject.AsObservable();


        private List<PointF> _pointerDownPositions = new List<PointF>();
        private List<PointF> _previousPointerPositions = new List<PointF>();

        internal void OnBegin(UITouch[] events)
        {
            for (int i = events.Length - 1; i >= 0; i--)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);
                
                if (!_owner.Frame.Contains(point))
                    return;
                var pointF = point.ToPointF();
                UpdatePoint(pointF, i, _pointerDownPositions);
                UpdatePoint(pointF, i, _previousPointerPositions);

                var pe = new PointerEvent(EventType.PointerDown, pointF, pointF, pointF, events.Length);
                _gestureSubject.OnNext(pe);
                System.Diagnostics.Debug.WriteLine($"Down: {pe}");
            }
        }

        internal void OnMove(UITouch[] events)
        {
            for (int i = events.Length - 1; i >= 0; i--)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);

                if (!_owner.Frame.Contains(point))
                    return;
                var pointF = point.ToPointF();
                UpdatePoint(pointF, i, _pointerDownPositions);
                UpdatePoint(pointF, i, _previousPointerPositions);

                var pe = new PointerEvent(EventType.Move, _pointerDownPositions[i], _previousPointerPositions[i], pointF, events.Length);
                _gestureSubject.OnNext(pe);
                System.Diagnostics.Debug.WriteLine($"Move: {pe}");
            }
        }

        internal void OnEnd(UITouch[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);

                if (!_owner.Frame.Contains(point))
                    return;
                var pointF = point.ToPointF();
                UpdatePoint(pointF, i, _pointerDownPositions);
                UpdatePoint(pointF, i, _previousPointerPositions);

                var pe = new PointerEvent(EventType.PointerUp, _pointerDownPositions[i], _previousPointerPositions[i], pointF, events.Length);
                _gestureSubject.OnNext(pe);
                System.Diagnostics.Debug.WriteLine($"End: {pe}");
            }

            for (int i = 0; i < events.Length; i++)
            {
                _pointerDownPositions.RemoveAt(i);
                _previousPointerPositions.RemoveAt(i);
            }
        }

        internal void OnCancel(UITouch[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);

                if (!_owner.Frame.Contains(point))
                    return;
                var pointF = point.ToPointF();
                UpdatePoint(pointF, i, _pointerDownPositions);
                UpdatePoint(pointF, i, _previousPointerPositions);
                var pe = new PointerEvent(EventType.Cancel, _pointerDownPositions[i], _previousPointerPositions[i], pointF, events.Length);
                _gestureSubject.OnNext(pe);
                System.Diagnostics.Debug.WriteLine($"Cancel: {pe}");
            }

            for (int i = 0; i < events.Length; i++)
            {
                _pointerDownPositions.RemoveAt(i);
                _previousPointerPositions.RemoveAt(i);
            }
        }

        private void UpdatePoint(PointF point, int index, List<PointF> list)
        {
            if (list.Count > index)
            {
                list[index] = point;
            }
            else
            {
                list.Add(point);
            }
        }
    }

    internal static class CGPointExtensions
    {
        public static PointF ToPointF(this CGPoint point)
        {
            return PointF.Create((float)point.X, (float)point.Y);
        }
    }
}
