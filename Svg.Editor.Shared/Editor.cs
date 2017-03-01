using System.Reactive.Concurrency;
using System.Threading;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor
{
    public static class Editor
    {
        public static void Init()
        {
            // register base services

            Engine.RegisterSingleton<IMarshal, SvgMarshal>(() => new SvgMarshal());
            Engine.RegisterSingleton<ISvgElementAttributeProvider, SvgElementAttributeProvider>(() => new SvgElementAttributeProvider());
            Engine.RegisterSingleton<ICultureHelper, CultureHelper>(() => new CultureHelper());
            Engine.RegisterSingleton<ILogger, DefaultLogger>(() => new DefaultLogger());
            Engine.RegisterSingleton<ICharConverter, SvgCharConverter>(() => new SvgCharConverter());
            Engine.Register<IWebRequest, WebRequestSvc>(() => new WebRequestSvc());
            Engine.RegisterSingleton<IFileSystem, FileSystem>(() => new FileSystem());

            // register platform specific services

            Engine.Register<IFactory, IFactory>(() => new SKFactory());

            var context = SynchronizationContext.Current;
            if (context != null)
            {
                var mainScheduler = new SynchronizationContextScheduler(context);
                var schedulerProvider = new SchedulerProvider(mainScheduler, NewThreadScheduler.Default);
                Engine.Register<ISchedulerProvider, SchedulerProvider>(() => schedulerProvider);
                Engine.RegisterSingleton<IGestureRecognizer, GestureRecognizer>(() => new GestureRecognizer(schedulerProvider));
            }

            Engine.RegisterSingleton<IUndoRedoService, UndoRedoService>(() => new UndoRedoService());
        }
    }
}
