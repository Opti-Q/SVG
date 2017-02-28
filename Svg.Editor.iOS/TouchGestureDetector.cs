using System;
using System.Reactive.Subjects;
using Svg.Editor.Events;
using Svg.Editor.Services;
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
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Changed)
            {
                var c = new ScaleEvent(ScaleStatus.Scaling, (float) r.Scale, 0f, 0f);
                _gestureSubject.OnNext(c);
            }
            else if( state == UIGestureRecognizerState.Cancelled ||
                state == UIGestureRecognizerState.Ended ||
                state ==UIGestureRecognizerState.Recognized)
            {
                var e = new ScaleEvent(ScaleStatus.End, (float)r.Scale, 0f, 0f);
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
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Changed)
            {
                _absoluteRotation += RadianToDegree(r.Rotation);
                var relativeRotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(relativeRotation, _absoluteRotation, RotateStatus.Rotating, (int)r.NumberOfTouches);
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Cancelled ||
                state == UIGestureRecognizerState.Ended ||
                state == UIGestureRecognizerState.Recognized)
            {
                _absoluteRotation += RadianToDegree(r.Rotation);
                var relativeRotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(relativeRotation, _absoluteRotation, RotateStatus.End, (int)r.NumberOfTouches);
                _gestureSubject.OnNext(s);
            }
        }
        private static float RadianToDegree(double angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public IObservable<UserInputEvent> DetectedGestures => _gestureSubject;

        internal void OnBegin(UITouch e)
        {
            var point = e.LocationInView(_owner);

            if (!_owner.Frame.Contains(point))
                return;


        }

        internal void OnMove(UITouch e)
        {

        }

        internal void OnEnd(UITouch e)
        {

        }

        internal void OnCancel(UITouch e)
        {

        }
    }
}
