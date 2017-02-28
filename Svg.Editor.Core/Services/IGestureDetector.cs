using System;
using Svg.Editor.Events;

namespace Svg.Editor.Services
{
    public interface IGestureDetector
    {
        IObservable<UserInputEvent> DetectedGestures { get; }
    }
}