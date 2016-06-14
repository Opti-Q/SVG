using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class GridTool : ToolBase
    {
        private readonly ICanInvalidateCanvas _canvas;
        public const float StepSize = 40;
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
            : base("Grid")
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
            Commands = _commands;
        }

        public bool IsVisible { get; set; } = true;

        private Pen Pen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 210, 210, 210)), 1));

        public override void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            //--------------------------------------------------
            // TODO DO THIS ONLY ONCE; NOT FOR EVERY ON DRAW
            //--------------------------------------------------

            if (!IsVisible)
                return;

            var canvasx = -ws.Translate.X;
            var canvasy = -ws.Translate.Y;

            //for (var i = -canvas.Width * MaxZoom; i <= canvas.Width * MaxZoom; i += StepSize - 2.5f)
            //    DrawTopDownIsoLine(canvas, i, canvasx, canvasy);      /* | */

            var relativeCanvasTranslationX = (canvasx) % StepSizeX;
            var relativeCanvasTranslationY = (canvasy) % StepSize;
            
            var gridLength = (Math.Max(renderer.Width, renderer.Height)/ws.ZoomFactor);
            var stepSize = (int) Math.Round(StepSize, 0);


            var x = canvasx - relativeCanvasTranslationX - (stepSize*2); // subtract 2x stepsize so gridlines always start from "out of sight" and lines do not start from a visible x-border
            var y = canvasy - relativeCanvasTranslationY;
            var lineLength = Math.Sqrt(Math.Pow(renderer.Width / ws.ZoomFactor, 2) + Math.Pow(renderer.Height / ws.ZoomFactor, 2)) * 1.2f; // multiply by 1.2f as we later also start drawing from a minus x coordinate (- 2*stepsize)
            
            for (var i = -gridLength; i <= gridLength; i += stepSize)
            {
                DrawLineLeftToTop(renderer, i, x, y, lineLength);       /* / */
                DrawLineLeftToBottom(renderer, i, x, y, lineLength);    /* \ */
            }

            //renderer.DrawCircle(0, 0, 200, Pen); // point should remain in top left corner on screen
            //renderer.DrawCircle(canvasx, canvasy, 100, Pen); // point on canvas - should move along
        }

        public override void OnTouch(UserInputEvent userInputEvent, SvgDrawingCanvas ws)
        {
            // You know nothing Jon Snow
        }
        
        // line looks like this -> /
        private void DrawLineLeftToTop(IRenderer renderer, float y, float canvasX, float canvasY, double lineLength)
        {
            var startX = canvasX;
            var startY = y + canvasY;
            var stopX = ((float)(lineLength * Math.Cos(Alpha * (Math.PI / 180)))) + canvasX;
            var stopY = (y - (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180)))) + canvasY;
            

            renderer.DrawLine(
                startX,
                startY,
                stopX,
                stopY,
                Pen);
        }

        // line looks like this -> \
        private void DrawLineLeftToBottom(IRenderer renderer, float y, float canvasX, float canvasY, double lineLength)
        {
            var startX = canvasX;
            var startY = y + canvasY;
            var endX = ((float)(lineLength * Math.Cos(Alpha * (Math.PI / 180)))) + canvasX;
            var endY = (y + (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180)))) + canvasY;

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

        public override void Dispose()
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