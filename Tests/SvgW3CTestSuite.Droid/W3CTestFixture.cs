﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Plugin.Toasts;
using SkiaSharp;
using Svg;
using Svg.Platform;
using Xamarin.Forms;


namespace SvgW3CTestSuite.Droid
{
    [TestFixture]
    public class W3CTestFixture
    {
        private static int _testCount = 0;
        private static int _succeededCount = 0;

        [SetUp]
        public void Setup()
        {
            SvgPlatformSetup.Init(new SvgAndroidPlatformOptions() {EnableFastTextRendering = true});
        }
        
        public static object[] SvgTestCases = {};
        public static Func<string, SvgAssetSource> FileSourceProvider { get; set; }

        [Test, TestCaseSource(nameof(SvgTestCases))]
        public async Task W3CTestSuiteCompare(string svgPath, string pngPath)
        {
            await RunTest(() =>
            {
                // Arrange
                const int width = 480;
                const int height = 360;

                // Act
                using (var svgBitmap = RenderSvg(svgPath, width, height))
                {
                    // Assert
                    using (var stream = FileSourceProvider(pngPath).GetStream())
                    using (var pngStream = new SKManagedStream(stream))
                    {
                        using (SKBitmap pngBitmap = new SKBitmap())
                        {
                            SKImageDecoder.DecodeStream(pngStream, pngBitmap);
                            using (var c = ImageCompare(svgBitmap, pngBitmap))
                            {
                                Assert.GreaterOrEqual(c.Similarity, 90, $"{svgPath}");
                            }
                        }
                    }
                }
               
            }, svgPath);
        }

        private Task RunTest(Action test, string name, int timeout = 10000)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        Interlocked.Increment(ref _testCount);
                        System.Diagnostics.Debug.Write($"starting test #{_testCount} '{name}#'");
                        test();
                        

                        NotifySuccess(name);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception x)
                    {
                        NotifyError(name);
                        tcs.TrySetException(x);
                    }

                });
                thread.Start();
                Task.Run(async () =>
                {
                    await Task.Delay(timeout);
                    thread.Abort();
                    tcs.TrySetCanceled();
                }).ConfigureAwait(false);

            }
            catch (TaskCanceledException)
            {
                NotifyError(name);
                Assert.Fail($"test {name} took too much time");
            }
            catch (ThreadAbortException)
            {
                NotifyError(name);
                Assert.Fail($"test {name} took too much time");
            }
            return tcs.Task;
        }

        private static void NotifySuccess(string svgPath)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                Interlocked.Increment(ref _succeededCount);

                var message = $"{svgPath} succeeded ({_succeededCount}/ {_testCount})";
                System.Diagnostics.Debug.Write(message);
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(ToastNotificationType.Success, "Finished test", message, TimeSpan.FromMilliseconds(500));
            });
        }

        private static void NotifyError(string svgPath)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                var message = $"{svgPath} failed ({_succeededCount} / {_testCount})";
                System.Diagnostics.Debug.Write(message);
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(ToastNotificationType.Error, "Failed test", message, TimeSpan.FromMilliseconds(500));
            });
        }

        private static SKBitmap RenderSvg(string svgPath, int width, int height)
        {
            var src = FileSourceProvider(svgPath);

            using (SvgDocument doc = SvgDocument.Open<SvgDocument>(src))
            using (var surface = SKSurface.Create(width, height, SKColorType.Rgba_8888, SKAlphaType.Premul))
            {
                doc.Draw(SvgRenderer.FromGraphics(new SkiaGraphics(surface)));
                var img = surface.Snapshot();

                using (var s = new SKManagedStream(img.Encode().AsStream()))
                {
                    SKBitmap b = new SKBitmap();
                    SKImageDecoder.DecodeStream(s, b);
                    return b;
                }
            }
        }

        private static ImageCompareResult ImageCompare(SKBitmap i1, SKBitmap i2)
        {
            float correctPixel = 0;
            float pixelAmount = i1.Height * i1.Width;
            //var bitmap = Android.Graphics.Bitmap.CreateBitmap(i1.Width, i1.Height, Android.Graphics.Bitmap.Config.Rgb565);
            //bitmap.EraseColor(Color.Red);

            for (var y = 0; y < i1.Height; ++y)
            {
                for (var x = 0; x < i1.Width; ++x)
                {
                    var c1 = i1.GetPixel(x, y);
                    var c2 = i2.GetPixel(x, y);

                    if (object.Equals(c1.Alpha, c2.Alpha) &&
                        object.Equals(c1.Green, c2.Green) &&
                        object.Equals(c1.Blue, c2.Blue) &&
                        object.Equals(c1.Red, c2.Red))
                    {
                        if (c1.Alpha != 0) // if pixel has alpha
                        {
                            pixelAmount--;
                            //bitmap.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            correctPixel++;
                            //bitmap.SetPixel(x, y, Color.White);
                        }
                    }
                }
            }

            return new ImageCompareResult((correctPixel / pixelAmount) * 100, /*bitmap*/null);
        }

        private class ImageCompareResult : IDisposable
        {
            public ImageCompareResult(float similarity, SKBitmap heatmap)
            {
                Similarity = similarity;
                Heatmap = heatmap;
            }

            public float Similarity { get; private set; }
            public SKBitmap Heatmap { get; private set; }
            public void Dispose()
            {
                Heatmap?.Dispose();
            }
        }

    }
}