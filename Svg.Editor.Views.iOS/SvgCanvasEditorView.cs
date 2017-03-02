using System;
using System.Collections.Generic;
using Foundation;
using SkiaSharp.Views.iOS;
using Svg.Editor.Shared;
using UIKit;

namespace Svg.Editor.iOS
{
    [Register(nameof(SvgCanvasEditorView))]
    public class SvgCanvasEditorView
        : SKCanvasView, IPaintSurface
    {
        private TouchGestureDetector _detector;

        public SvgCanvasEditorView()
        {
            _detector = new TouchGestureDetector(this);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnBegin(arr));
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnMove(arr));
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnEnd(arr));
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnCancel(arr));
        }

        private static void HandleTouches(NSSet touches, Action<UITouch[]> action)
        {
            if (touches.Count == 1)
            {
                UITouch touch = touches.AnyObject as UITouch;

                if (touch != null)
                {
                    var arr = new[] { touch };
                    action(arr);
                }
            }
            else if (touches.Count > 1)
            {
                var touchesList = new List<UITouch>();
                foreach (var touch in touches)
                {
                    if (touch != null)
                    {
                        touchesList.Add((UITouch)touch);
                    }
                }
                action(touchesList.ToArray());
            }
        }

    }
}
