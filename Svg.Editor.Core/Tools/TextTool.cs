using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public interface ITextInputService
    {
        Task<string> GetUserInput(string title, string textValue);
    }

    public class TextTool : ToolBase
    {
        // if user moves cursor, she does not want to add/edit text
        private bool _moveEventWasRegistered;

        public TextTool() : base("Text")
        {
        }

        private ITextInputService TextInputService => Engine.Resolve<ITextInputService>();

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Text", (obj) =>
                {
                    this.IsActive = !this.IsActive;
                })
            };

            this.IsActive = false;

            return Task.FromResult(true);
        }

        public override async Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!this.IsActive)
                return;

            // if user moves cursor, she does not want to add/edit text
            var me = @event as MoveEvent;
            if (me != null)
            {
                _moveEventWasRegistered = true;
            }

            var pe = @event as PointerEvent;
            if (pe != null && pe.EventType == EventType.PointerDown)
            {
                _moveEventWasRegistered = false;
            }
            else if (pe != null && pe.EventType == EventType.PointerUp)
            {
                if (_moveEventWasRegistered)
                {
                    return;
                }
                
                                
                var dX = pe.Pointer1Position.X - pe.Pointer1Down.X;
                var dY = pe.Pointer1Position.Y - pe.Pointer1Down.Y;

                // if Point-Down and Point-Up are merely the same
                if (dX < 20 && dY < 20)
                {
                    // if there is text below the pointer, edit it
                    var e = ws.GetElementsUnderPointer(pe.Pointer1Position).OfType<SvgText>().FirstOrDefault();

                    if (e != null)
                    {
                        var txt = await TextInputService.GetUserInput("Edit text", e.Text);
                        // if text was removed, remove element
                        if (string.IsNullOrWhiteSpace(txt))
                        {
                            e.Parent.Children.Remove(e);
                        }
                        else if(!string.Equals(e.Text, txt))
                        {
                            e.Text = txt;
                        }
                    }
                    // else add new text   
                    else
                    {
                        var txt = await TextInputService.GetUserInput("Add text", null);
                        // only add if user really entered text.
                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            var t = new SvgText(txt);
                            t.FontSize = new SvgUnit(SvgUnitType.Pixel, 20);
                            t.Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0));
                            t.Fill = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0));
                            
                            var z = ws.ZoomFactor;
                            var halfRelWidth = pe.Pointer1Position.X / z;
                            var halfRelHeight = pe.Pointer1Position.Y / z;
                            var childBounds = t.Bounds;
                            var halfRelChildWidth = childBounds.Width / 2;
                            var halfRelChildHeight = childBounds.Height / 2;

                            var x = -ws.RelativeTranslate.X + halfRelWidth - halfRelChildWidth;
                            var y = -ws.RelativeTranslate.Y + halfRelHeight - halfRelChildHeight;
                            t.X = new SvgUnitCollection {new SvgUnit(SvgUnitType.Pixel, x)};
                            t.Y = new SvgUnitCollection { new SvgUnit(SvgUnitType.Pixel, y) };

                            ws.Document.Children.Add(t);
                        }
                    }

                    ws.FireInvalidateCanvas();
                }
            }
        }


    }
}
