using Foundation;
using SkiaSharp.Views.iOS;
using UIKit;

namespace Svg.Editor.iOS
{
    [Register(nameof(SvgCanvasEditorView))]
    public class SvgCanvasEditorView
        : SKCanvasView
    {
        private TouchGestureDetector _detector;

        public SvgCanvasEditorView()
        {
            _detector = new TouchGestureDetector(this);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if (touches.Count == 1)
            {
                UITouch touch = touches.AnyObject as UITouch;

                if (touch != null)
                {
                    _detector.OnBegin(touch);
                }
            }
            else if (touches.Count > 1)
            {
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                _detector.OnMove(touch);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                _detector.OnEnd(touch);
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                _detector.OnCancel(touch);
            }
        }
    }
}
