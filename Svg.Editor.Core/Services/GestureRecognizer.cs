using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Svg.Core.Events;
using Svg.Core.Gestures;
using Svg.Interfaces;

namespace Svg.Core.Services
{
    public class GestureRecognizer : IDisposable
    {
        private readonly Subject<UserGesture> _recognizedGestures = new Subject<UserGesture>();
        private IObservable<UserInputEvent> DetectedInputEvents { get; }

        public IObservable<UserGesture> RecognizedGestures => _recognizedGestures.AsObservable();

        private readonly IDictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();

        public GestureRecognizer(IObservable<UserInputEvent> detectedInputEvents, IScheduler mainScheduler, IScheduler backgroundScheduler)
        {
            DetectedInputEvents = detectedInputEvents;

            var pointerEvents = DetectedInputEvents.OfType<PointerEvent>();
            var enterEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerDown);
            var exitEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerUp || pe.EventType == EventType.Cancel);
            var interactionWindows = pointerEvents.Window(enterEvents, _ => exitEvents);

            _recognizedGestures.Subscribe(ug => Debug.WriteLine(ug.Type));

            // tap gesture
            _subscriptions["tap"] = interactionWindows
            .SelectMany(window =>
            {
                return window
                .Where(
                    pe =>
                        pe.EventType == EventType.PointerUp &&
                        PositionEquals(pe.Pointer1Down, pe.Pointer1Position, TouchThreshold))
                .Buffer(TimeSpan.FromSeconds(TapTimeout), 1, backgroundScheduler)
                .Take(1);
            })
            .ObserveOn(mainScheduler)
            .Subscribe
            (
                l =>
                {
                    var pe = l.FirstOrDefault();
                    if (pe != null) _recognizedGestures.OnNext(new TapGesture(pe.Pointer1Position));
                },
                ex => Debug.WriteLine(ex.Message)
            );

            // double tap gesture
            //_subscriptions["doubletap"] = _recognizedGestures.OfType<TapGesture>()
            //.Window(_recognizedGestures.Where(g => g.Type == GestureType.Tap), _ => _recognizedGestures.Where(g => g.Type == GestureType.Tap).Skip(1))
            //.SelectMany(window =>
            //{
            //    return window
            //    .Aggregate((acc, current) => acc == null ? current : PositionEquals(acc.Position, current.Position, TouchThreshold) ? current : null)
            //    .Do(x => Debug.WriteLine(x?.Type), () => Debug.WriteLine("WINDOW COMPLETE"))
            //    .Buffer(TimeSpan.FromSeconds(DoubleTapTimeout), 1)
            //    .Take(1);
            //})
            //.ObserveOn(_mainScheduler)
            //.Subscribe(x =>
            //{
            //    var tg = x.FirstOrDefault();
            //    if (tg != null) _recognizedGestures.OnNext(new DoubleTapGesture(tg.Position));
            //});

            //_recognizedGestures.OfType<DoubleTapGesture>().Subscribe(dtg => Debug.WriteLine("Double Tap!"));

            _recognizedGestures
            .Timestamp()
            .Scan((acc, current) =>
            {
                var t1 = acc.Value as TapGesture;
                var t2 = current.Value as TapGesture;

                if (t1?.Type == GestureType.Tap && t2?.Type == GestureType.Tap
                    && current.Timestamp - acc.Timestamp <= TimeSpan.FromSeconds(DoubleTapTimeout)
                    && PositionEquals(t1.Position, t2.Position, TouchThreshold))
                    return
                        Timestamped.Create<UserGesture>(
                            new DoubleTapGesture(((TapGesture) current.Value).Position), current.Timestamp);
                return current;
            })
            .Select(ts => ts.Value)
            .OfType<DoubleTapGesture>()
            .Subscribe(dt => _recognizedGestures.OnNext(dt));
            
            // long press gesture
            _subscriptions["longpress"] = interactionWindows
            .Select(window =>
            {
                return window
                .Where(
                    pe =>
                        pe.EventType == EventType.PointerDown ||
                        pe.EventType == EventType.PointerUp && PositionEquals(pe.Pointer1Down, pe.Pointer1Position, TouchThreshold))
                .Buffer(TimeSpan.FromSeconds(LongPressDuration), 2)
                .Take(1);
            })
            .SelectMany(o => o)
            .ObserveOn(mainScheduler)
            .Subscribe
            (
                l =>
                {
                    var pe = l.LastOrDefault();
                    if (pe != null && pe.EventType != EventType.PointerUp) _recognizedGestures.OnNext(new LongPressGesture(pe.Pointer1Position));
                },
                ex => Debug.WriteLine(ex.Message)
            );

            // drag gesture
            _subscriptions["drag"] = interactionWindows.Subscribe(window =>
            {
                // create this subject for controlling lifetime of the subscription
                var dragLifetime = new Subject<Unit>();
                var dragSubscription = window
                    .Where(pe => pe.EventType == EventType.Move && pe.PointerCount == 1)
                    // if we get a window without move events, we want to dispose subscription entirely,
                    // else we would propagate an unneccessary DragGesture.Exit gesture
                    .DefaultIfEmpty(new PointerEvent(EventType.Cancel, PointF.Empty, PointF.Empty, PointF.Empty, 0))
                    .Select((pe, i) =>
                    {
                        if (i == 0 && pe.EventType != EventType.Cancel)
                            _recognizedGestures.OnNext(DragGesture.Enter(pe.Pointer1Down));
                        // if we had an empty window, it defaults to EventType.Cancel and we dispose subscription
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
                    );

                // bind completion of dragLifetime to disposing of our subscription
                Observable.Using(() => dragSubscription, _ => dragLifetime).Subscribe();
            });
        }

        private static bool PositionEquals(PointF start, PointF position, double threshold = 0)
        {
            var delta = position - start;
            return Math.Abs(delta.X) <= threshold && Math.Abs(delta.Y) <= threshold;
        }

        public double DragMinDistance { get; set; } = 10.0;

        public double LongPressDuration { get; set; } = 0.66;

        public double TouchThreshold { get; set; } = 10.0;

        public double TapTimeout { get; set; } = 0.33;

        public double DoubleTapTimeout { get; set; } = 0.5;

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
