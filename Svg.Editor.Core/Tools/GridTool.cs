using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class GridTool : ITool
    {
        private readonly ICanInvalidateCanvas _canvas;
        public const float StepSize = 40;
        private double _length = 0;
        private const float MaxZoom = 1f;//ZoomTool.MaxScale;
        private static double A;
        private static double B;
        private static double C;
        private static float StepSizeX;
        private const double Alpha = 27.3f;
        private const double Gamma = 90f;
        private static double Beta;
        private Pen _pen;

        private readonly List<IToolCommand> _commands = new List<IToolCommand>();

        public GridTool(ICanInvalidateCanvas canvas)
        {
            _canvas = canvas;
            // using triangle calculation to determine the x and y steps based on stepsize (y) and angle (alpha)
            // http://www.arndt-bruenner.de/mathe/scripts/Dreiecksberechnung.htm
            A = StepSize;
            Beta = 180f - (Alpha + Gamma);
            B = (A * SinDegree(Beta)) / SinDegree(Alpha);
            C = (A * SinDegree(Gamma)) / SinDegree(Alpha);
            StepSizeX = (float)B;
            
            // add tool commands
            _commands.Add(new ToolCommand(this, "Toggle Grid", (obj) =>
            {
                IsVisible = !IsVisible;
                _canvas.InvalidateCanvas();
            }, (obj) => true));
        }

        public bool IsVisible { get; set; } = true;
        public bool IsActive { get; set; }
        private Pen Pen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 210, 210, 210)), 1));

        public void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            //--------------------------------------------------
            // TODO DO THIS ONLY ONCE; NOT FOR EVERY ON DRAW
            //--------------------------------------------------

            if (!IsVisible)
                return;

            if(_length <= 0) // compute this only once
                _length = Math.Sqrt((renderer.Width * renderer.Width) + (renderer.Height * renderer.Height)) * MaxZoom * 2;


            var canvasx = -ws.Translate.X;
            var canvasy = -ws.Translate.Y;

            //for (var i = -canvas.Width * MaxZoom; i <= canvas.Width * MaxZoom; i += StepSize - 2.5f)
            //    DrawTopDownIsoLine(canvas, i, canvasx, canvasy);      /* | */

            var relativeCanvasTranslationX = (canvasx) % StepSizeX;
            var relativeCanvasTranslationY = (canvasy) % StepSize;

            var dist = Math.Max(renderer.Width, renderer.Height)*MaxZoom*2;
            var stepSize = (int) Math.Round(StepSize, 0);

            for (var i = -dist; i <= dist; i += stepSize)
            {
                DrawLineLeftToTop(renderer, i, canvasx - relativeCanvasTranslationX, canvasy - relativeCanvasTranslationY);       /* / */
                DrawLineLeftToBottom(renderer, i, canvasx - relativeCanvasTranslationX, canvasy - relativeCanvasTranslationY);    /* \ */
            }

            //canvas.DrawCircle(0, 0, 200, Paint2);
            //canvas.DrawCircle(canvasx, canvasy, 100, Paint2);
            //canvas.DrawCircle((-canvasx)+canvas.Width, (-canvasy)+canvas.Height, 100, Paint2);
        }
        public void OnTouch(UserInputEvent userInputEvent, SvgDrawingCanvas ws)
        {
            // You know nothing Jon Snow
        }


        // line looks like this -> /
        private void DrawLineLeftToTop(IRenderer renderer, float y, float canvasX, float canvasY)
        {
            var startX = -(renderer.Width * MaxZoom) + canvasX;
            var startY = y + canvasY;
            var stopX = (-(renderer.Width * MaxZoom) + ((float)(_length * Math.Cos(Alpha * (Math.PI / 180))))) + canvasX;
            var stopY = (y - (float)(_length * Math.Sin(Alpha * (Math.PI / 180)))) + canvasY;

            renderer.DrawLine(
                startX,
                startY,
                stopX,
                stopY,
                Pen);
        }

        // line looks like this -> \
        private void DrawLineLeftToBottom(IRenderer renderer, float y, float canvasX, float canvasY)
        {
            var startX = (-(renderer.Width * MaxZoom)) + canvasX;
            var startY = y + canvasY;
            var endX = (-(renderer.Width * MaxZoom) + ((float)(_length * Math.Cos(Alpha * (Math.PI / 180))))) + canvasX;
            var endY = (y + (float)(_length * Math.Sin(Alpha * (Math.PI / 180)))) + canvasY;

            renderer.DrawLine(
                startX,
                startY,
                endX,
                endY,
                Pen);
        }

        //// line looks like this -> |
        //private void DrawTopDownIsoLine(ICanvas canvas, float y)
        //{
        //    if(ZoomTool.ScaleFactor < 0.85f)
        //        return;

        //    canvas.DrawLine(
        //        (y),
        //        (-(canvas.Height * MaxZoom)),
        //        (y),
        //        (canvas.Height * MaxZoom),
        //        Paint);
        //}

        public void Reset()
        {
            // You know nothing Jon Snow
        }

        public IEnumerable<IToolCommand> Commands => _commands;

        public string Name => "Grid";

        public void Dispose()
        {
            Pen.Dispose();
        }

        private static double SinDegree(double value)
        {
            return RadianToDegree(Math.Sin(DegreeToRadian(value)));
        }
        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }
}