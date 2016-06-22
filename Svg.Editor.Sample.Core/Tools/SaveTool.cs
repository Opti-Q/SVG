using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Plugins.Email;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class SaveTool : ToolBase
    {
        private readonly bool _autoLoad;
        private readonly Func<string> _fileName;

        public SaveTool(bool autoLoad = true, Func<string> fileName = null ) : base("Save/Load")
        {
            _autoLoad = autoLoad;
            _fileName = fileName ?? (() => "svg_image.svg");
            IconName = "ic_save_white_48dp.png";
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            Commands = new[]
            {
                new ToolCommand(this, "Save", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                        fs.DeleteFile(storagePath);
                    }
                    
                    var documentSize = ws.Document.CalculateDocumentBounds();
                    ws.Document.Width = new SvgUnit(SvgUnitType.Pixel, documentSize.Width);
                    ws.Document.Height = new SvgUnit(SvgUnitType.Pixel, documentSize.Height);

                    ws.Document.Write(storagePath);

                    ws.FireToolCommandsChanged();

                }, 
                (obj) => ws.Document != null),
                new ToolCommand(this, "Load", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                       ws.Document = Svg.SvgDocument.Open<SvgDocument>(storagePath);
                    }
                    ws.FireToolCommandsChanged();

                }, 
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share SVG", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                        using (var stream = fs.OpenRead(storagePath))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/svg+xml", FileName = "svg_file.svg"} });
                        }
                    }

                }, 
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG", (obj) =>
                {
                    var fs = Engine.Resolve<IFileSystem>();
                    var storer = Engine.Resolve<IImageStorer>();

                    //using (var bmp = ws.GetOrCreate(ws.ScreenWidth, ws.ScreenHeight))
                    using (var bmp = ws.Document.DrawAllContents(Engine.Factory.Colors.White)) // 2MP (see https://de.wikipedia.org/wiki/Bildaufl%C3%B6sungen_in_der_Digitalfotografie)
                    {
                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDownloadFolder(), "svg_image.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            storer.SaveAsPng(bmp, stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG thumb", (obj) =>
                {
                    var fs = Engine.Resolve<IFileSystem>();
                    var storer = Engine.Resolve<IImageStorer>();

                    using (var bmp = ws.Document.DrawAllContents(160, 160, Engine.Factory.Colors.White))
                    {
                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDownloadFolder(), "svg_image_thumb.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            storer.SaveAsPng(bmp, stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG XL", (obj) =>
                {
                    var fs = Engine.Resolve<IFileSystem>();
                    var storer = Engine.Resolve<IImageStorer>();

                    using (var bmp = ws.Document.DrawAllContents(2048, 1920, Engine.Factory.Colors.White))
                    {
                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDownloadFolder(), "svg_image_XL.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            storer.SaveAsPng(bmp, stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Clear", (obj) =>
                {
                    ws.Document = new SvgDocument();
                })

            };

            if (_autoLoad)
            {
                Commands.Single(t => t.Name == "Load").Execute(null);
            }

            return Task.FromResult(true);
        }
    }
}
