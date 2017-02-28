using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.UI.Xaml.Input;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;

namespace Svg.Editor.UWP
{
    public class UwpGestureDetector : IGestureDetector
    {
        private readonly Subject<UserInputEvent> _gesturesSubject = new Subject<UserInputEvent>();
        public IObservable<UserInputEvent> DetectedGestures => _gesturesSubject.AsObservable();

        public void OnTouch(PointerRoutedEventArgs args)
        {
            
        }
    }
}
