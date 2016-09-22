using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Svg.Core.Events;
using Svg.Core.Gestures;
using Svg.Interfaces;

namespace Svg.Core.Services
{
    public class GestureRecognizer : IDisposable
    {
        private readonly Subject<UserGesture> _recognizedGestures = new Subject<UserGesture>();
        private IObservable<UserInputEvent> DetectedGestures { get; }

        public IObservable<UserGesture> RecognizedGestures => _recognizedGestures.AsObservable();

        private readonly IDictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();
        private readonly IScheduler _mainScheduler;
        private readonly IScheduler _backgroundScheduler;

        public GestureRecognizer(IObservable<UserInputEvent> detectedGestures, IScheduler mainScheduler, IScheduler backgroundScheduler)
        {
            DetectedGestures = detectedGestures;

            _mainScheduler = mainScheduler;
            _backgroundScheduler = backgroundScheduler;

            var pointerEvents = DetectedGestures.OfType<PointerEvent>();
            var enterEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerDown);
            var exitEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerUp || pe.EventType == EventType.Cancel);
            var interactionWindows = pointerEvents.Window(enterEvents, _ => exitEvents);

            // tap gesture
            _subscriptions["tap"] = interactionWindows.Subscribe(window =>
            {
                window.Timeout(TimeSpan.FromSeconds(TapTimeout), _backgroundScheduler)
                .Aggregate
                (
                    (acc, current) =>
                    {
                        var delta = current.Pointer1Position - current.Pointer1Down;
                        if (current.EventType != EventType.PointerDown
                                && Math.Abs(delta.X) > TouchThreshold && Math.Abs(delta.Y) > TouchThreshold)
                            throw new Exception("Moved too far.");
                        return current;
                    }
                )
                // TODO: add throttle for double tap
                .Subscribe
                (
                    pe => _recognizedGestures.OnNext(new TapGesture(pe.Pointer1Position)),
                    ex => { }
                );
            });

            // long press gesture
            _subscriptions["longpress"] = interactionWindows.Subscribe(window =>
            {
                Observable.When
                (
                    window
                        .Scan((acc, current) =>
                        {
                            var delta = current.Pointer1Position - current.Pointer1Down;
                            if (current.EventType == EventType.PointerUp) throw new Exception("Pointer exited.");
                            if (current.EventType != EventType.PointerDown && Math.Abs(delta.X) > TouchThreshold &&
                                Math.Abs(delta.Y) > TouchThreshold) throw new Exception("Moved too far.");
                            return current;
                        })
                        .And(Observable.Timer(TimeSpan.FromSeconds(LongPressDuration), _backgroundScheduler))
                        .Then((pe, __) => pe)
                )
                .ObserveOn(_mainScheduler)
                .Subscribe
                (
                    pe => _recognizedGestures.OnNext(new LongPressGesture(pe.Pointer1Position)),
                    ex => { }
                );
            });

            // drag gesture
            _subscriptions["drag"] = interactionWindows.Subscribe(window =>
            {
                var dragLifetime = new Subject<Unit>();
                dragLifetime.Subscribe(_ => { }, Debugger.Break);
                Observable.Using(() => window
                        .Where(pe => pe.EventType == EventType.Move && pe.PointerCount == 1)
                        .DefaultIfEmpty(new PointerEvent(EventType.Cancel, PointF.Empty, PointF.Empty, PointF.Empty, 0))
                        .Select((pe, i) =>
                        {
                            if (i == 0 && pe.EventType != EventType.Cancel) _recognizedGestures.OnNext(DragGesture.Enter(pe.Pointer1Down));
                            if (pe.EventType == EventType.Cancel) dragLifetime.OnCompleted();
                            return pe;
                        })
                        .Subscribe
                        (
                            pe =>
                            {
                                var deltaPoint = pe.Pointer1Position - pe.Pointer1Down;
                                var delta = SizeF.Create(deltaPoint.X, deltaPoint.Y);

                                // selection only counts if width and height are not too small
                                var dist = Math.Sqrt(Math.Pow(delta.Width, 2) + Math.Pow(delta.Height, 2));

                                if (dist > DragMinDistance)
                                {
                                    _recognizedGestures.OnNext(new DragGesture(pe.Pointer1Position, pe.Pointer1Down,
                                        delta, dist));
                                }
                            },
                            ex => { },
                            () => _recognizedGestures.OnNext(DragGesture.Exit)
                        ),
                    _ => dragLifetime)
                    .Subscribe();
            });
        }

        public double DragMinDistance { get; set; } = 10.0;

        public double LongPressDuration { get; set; } = 0.66;

        public double TouchThreshold { get; set; } = 10.0;

        public double TapTimeout { get; set; } = 0.33;

        public void Dispose()
        {
            foreach (var disposable in _subscriptions.Values) disposable.Dispose();
        }
    }

    public interface IGestureDetector
    {
        IObservable<UserInputEvent> DetectedGestures { get; }
    }
}
