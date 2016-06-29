using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using NUnit.Framework;
using Plugin.Toasts;
using SkiaSharp;
using Svg;
using Svg.Platform;
using Xamarin.Forms;
using Bitmap = Android.Graphics.Bitmap;
using Color = Android.Graphics.Color;


namespace SvgW3CTestSuite.Droid
{
    [TestFixture]
    public class TestsSample
    {
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
                System.Diagnostics.Debug.Write($"starting test {svgPath}");
                // Arrange
                const int width = 480;
                const int height = 360;

                // Act
                using (var svgBitmap = RenderSvg(svgPath, width, height))
                {
                    // Assert
                    using (var pngStream = FileSourceProvider(pngPath).GetStream())
                    using (var png = BitmapFactory.DecodeStream(pngStream))
                    {
                        using (var c = ImageCompare(svgBitmap, png))
                        {
                            Assert.GreaterOrEqual(c.Similarity, 90, $"{svgPath}");
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
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(ToastNotificationType.Success, "Finished test", svgPath, TimeSpan.FromMilliseconds(500));
            });
        }

        private static void NotifyError(string svgPath)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(ToastNotificationType.Error, "Failed test", svgPath, TimeSpan.FromMilliseconds(500));
            });
        }

        private static Android.Graphics.Bitmap RenderSvg(string svgPath, int width, int height)
        {
            var src = FileSourceProvider(svgPath);

            var bitmap = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888);
            using (SvgDocument doc = SvgDocument.Open<SvgDocument>(src))
            {
                var canvas = new Canvas();
                try
                {
                    using (var surface = SKSurface.Create(width, height, SKColorType.Rgba_8888, SKAlphaType.Premul, bitmap.LockPixels(), width * 4))
                    {
                        doc.Draw(SvgRenderer.FromGraphics(new SkiaGraphics(surface)));
                    }
                }
                finally
                {
                    bitmap.UnlockPixels();
                }
                canvas.DrawBitmap(bitmap, 0, 0, null);
            }
            return bitmap;
        }

        private static ImageCompareResult ImageCompare(Android.Graphics.Bitmap i1, Android.Graphics.Bitmap i2)
        {
            float correctPixel = 0;
            float pixelAmount = i1.Height * i1.Width;
            var bitmap = Android.Graphics.Bitmap.CreateBitmap(i1.Width, i1.Height, Android.Graphics.Bitmap.Config.Rgb565);
            bitmap.EraseColor(Color.Red);

            for (var y = 0; y < i1.Height; ++y)
            {
                for (var x = 0; x < i1.Width; ++x)
                {
                    if (i1.GetPixel(x, y) == i2.GetPixel(x, y))
                    {
                        if (Color.GetAlphaComponent(i1.GetPixel(x, y)) != 0) // if pixel has alpha
                        {
                            pixelAmount--;
                            bitmap.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            correctPixel++;
                            bitmap.SetPixel(x, y, Color.White);
                        }
                    }
                }
            }

            return new ImageCompareResult((correctPixel / pixelAmount) * 100, bitmap);
        }

        private class ImageCompareResult : IDisposable
        {
            public ImageCompareResult(float similarity, Bitmap heatmap)
            {
                if (heatmap == null) throw new ArgumentNullException(nameof(heatmap));
                Similarity = similarity;
                Heatmap = heatmap;
            }

            public float Similarity { get; private set; }
            public Android.Graphics.Bitmap Heatmap { get; private set; }
            public void Dispose()
            {
                Heatmap?.Dispose();
            }
        }

    }
}