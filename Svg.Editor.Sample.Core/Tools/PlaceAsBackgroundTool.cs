using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class PlaceAsBackgroundTool : UndoableToolBase
    {
        public PlaceAsBackgroundTool(string jsonProperties, IUndoRedoService undoRedoService) : base("PlaceAsBackground", jsonProperties, undoRedoService)
        {
            IconName = "ic_insert_photo_white_48dp.png";
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Place background image", async o =>
                {
                    //var fs = Engine.Resolve<IFileSystem>();
                    //var svgCachingService = Engine.Resolve<ISvgCachingService>();
                    //var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
                    //var selectedColor = colorTool.SelectedColor;
                    //var path = svgCachingService.GetCachedPngPath(colorTool.IconName, $"{selectedColor.R}_{selectedColor.G}_{selectedColor.B}", fs);
                    var path = await new AndroidPickImageService().PickImagePath();
                    try
                    {
                        Canvas.Document.Children.Insert(0, new SvgImage
                        {
                            Href = new Uri($"file://{path}", UriKind.Absolute)
                        });
                    }
                    catch (IOException)
                    {
                        Debugger.Break();
                    }
                })
            };

            return base.Initialize(ws);
        }
    }
}
