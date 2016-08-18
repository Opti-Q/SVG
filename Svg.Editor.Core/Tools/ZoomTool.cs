using System;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Utils;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public class ZoomTool : ToolBase
    {
        private SvgDrawingCanvas _owner;
        private Brush _purpleBrush;
        private Pen _purplePen;
        private Brush _orangeBrush;
        private Pen _orangePen;
        private bool _focused;
        private float CurrentFocusX { get; set; }
        private float CurrentFocusY { get; set; }
        private Brush PurpleBrush => _purpleBrush ?? (_purpleBrush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 210, 80, 210)));
        private Pen PurplePen => _purplePen ?? (_purplePen = Engine.Factory.CreatePen(PurpleBrush, 5));
        private Brush OrangeBrush => _orangeBrush ?? (_orangeBrush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 220, 160, 60)));
        private Pen OrangePen => _orangePen ?? (_orangePen = Engine.Factory.CreatePen(OrangeBrush, 5));

        public ZoomTool(float minScale = 0.5f, float maxScale = 5.0f)
            : base("Zoom")
        {
            MinScale = minScale;
            MaxScale = maxScale;
            IconName = "ic_zoom_white_48dp.png";
        }

        public float MinScale { get; set; }

        public float MaxScale { get; set; }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            _owner = ws;

            Commands = new[]
            {
                new ToolCommand(this, "Zoom in +", (x) =>
                {
                    var f =_owner.ZoomFactor + 0.25f;
                    _owner.ZoomFactor = ws.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    _owner.FireInvalidateCanvas();
                }, iconName:"ic_zoom_in_white_48dp.png", sortFunc:(x) => 1500),
                new ToolCommand(this, "Zoom out -", (x) =>
                {
                    var f =_owner.ZoomFactor - 0.25f;
                    _owner.ZoomFactor = ws.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    _owner.FireInvalidateCanvas();
                }, iconName:"ic_zoom_out_white_48dp.png", sortFunc:(x) => 1550),
                new ToolCommand(this, "100 %", (x) =>
                {
                    _owner.ZoomFactor = ws.ZoomFactor = Math.Max(MinScale, Math.Min(1, MaxScale));
                    _owner.FireInvalidateCanvas();
                }, iconName:"ic_zoom_100_white_48dp.png", sortFunc:(x) => 1600),
                new ToolCommand(this, "200 %", (x) =>
                {
                    _owner.ZoomFactor = ws.ZoomFactor = Math.Max(MinScale, Math.Min(2, MaxScale));
                    _owner.FireInvalidateCanvas();
                }, iconName:"ic_zoom_200_white_48dp.png", sortFunc:(x) => 1650),
            };

            return Task.FromResult(true);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return Task.FromResult(true);

            var se = @event as ScaleEvent;
            if (se == null)
                return Task.FromResult(true);

            switch (se.Status)
            {
                case ScaleStatus.Scaling:
                    CurrentFocusX = se.FocusX;
                    CurrentFocusY = se.FocusY;
                    if (!_focused)
                    {
                        var canvasFocus = ws.ScreenToCanvas(CurrentFocusX, CurrentFocusY);
                        ws.Translate -= (ws.ZoomFocus - canvasFocus) * (ws.ZoomFactor - 1); 
                        ws.ZoomFocus = canvasFocus;
                        _focused = true;
                    }
                    // Don't let the object get too small or too large.
                    ws.ZoomFactor = GetBoundedZoomFactor(se, ws);
                    ws.FireInvalidateCanvas();
                    break;
                case ScaleStatus.Start:
                    _focused = false;
                    break;
            }

            return Task.FromResult(true);
        }

        private float GetBoundedZoomFactor(ScaleEvent se, SvgDrawingCanvas ws)
        {
            var newZoomFactor = ws.ZoomFactor * se.ScaleFactor;
            return Math.Max(MinScale, Math.Min(newZoomFactor, MaxScale));
        }

        public override async Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            await base.OnDraw(renderer, ws);

            renderer.Graphics.Save();

            var canvasCurrentFocus = ws.ScreenToCanvas(CurrentFocusX, CurrentFocusY);
            renderer.DrawCircle(canvasCurrentFocus.X, canvasCurrentFocus.Y, 18, PurplePen);
            renderer.DrawCircle(ws.ZoomFocus.X, ws.ZoomFocus.Y, 22, OrangePen);

            renderer.Graphics.Restore();
        }
    }
}
