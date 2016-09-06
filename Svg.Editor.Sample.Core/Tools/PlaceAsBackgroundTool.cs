using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class PlaceAsBackgroundTool : UndoableToolBase
    {
        public PlaceAsBackgroundTool(string jsonProperties, IUndoRedoService undoRedoService) : base("Background", jsonProperties, undoRedoService)
        {
            IconName = "ic_insert_photo_white_48dp.png";
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Place image", async o =>
                {
                    var path = await new AndroidPickImageService().PickImagePath(Canvas.ScreenWidth);
                    try
                    {
                        var children = Canvas.Document.Children;
                        // insert the background before the first visible element
                        var index = children.IndexOf(children.FirstOrDefault(x => x is SvgVisualElement));
                        // if there are no visual elements, we want to add it to the end of the list
                        if (index == -1) index = children.Count;
                        var image = new SvgImage
                        {
                            Href = new Uri($"file://{path}", UriKind.Absolute),
                            //Width = new SvgUnit(SvgUnitType.Percentage, 100),
                            //Height = new SvgUnit(SvgUnitType.Percentage, 100),
                            //AspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid)
                        };
                        image.CustomAttributes.Add("iclbackground", "");

                        // remove already placed background
                        var formerBackground = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey("iclbackground"));
                        if (formerBackground != null)
                        {
                            children.Remove(formerBackground);
                            formerBackground.Dispose();
                        }

                        children.Insert(index, image);

                        Canvas.FireInvalidateCanvas();
                    }
                    catch (IOException)
                    {
                        Debugger.Break();
                    }
                }, iconName: "ic_insert_photo_white_48dp.png"),
                new ToolCommand(this, "Remove image", o =>
                {
                    var children = Canvas.Document.Children;
                    var background = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey("iclbackground"));
                    if (background != null)
                    {
                        children.Remove(background);
                        background.Dispose();
                        Canvas.FireInvalidateCanvas();
                    }
                }, o => Canvas.Document.Children.Any(x => x.CustomAttributes.ContainsKey("iclbackground")), iconName: "ic_delete_white_48dp.png")
            };

            return base.Initialize(ws);
        }

        public string ImagePath
        {
            get
            {
                object imagePath;
                Properties.TryGetValue("imagepath", out imagePath);
                return imagePath as string;
            }
            set { Properties["imagepath"] = value; }
        }
    }
}
