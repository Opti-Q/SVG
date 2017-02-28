using System;
using Svg.Editor.Gestures;

namespace Svg.Editor.Interfaces
{
    public interface IGestureRecognizer
    {
        /// <summary>
        /// Observable for recognized gestures.
        /// </summary>
        IObservable<UserGesture> RecognizedGestures { get; }
    }
}