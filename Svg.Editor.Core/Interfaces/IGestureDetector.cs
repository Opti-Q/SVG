using System;
using Svg.Editor.Events;

namespace Svg.Editor.Interfaces
{
    public interface IGestureDetector
    {
        IObservable<UserInputEvent> DetectedGestures { get; }
    }
}