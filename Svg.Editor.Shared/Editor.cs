﻿using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor
{
    public static class Editor
    {
        private static bool _initialized = false;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        public static void Init()
        {
            if (_initialized)
                return;

            try
            {
                _lock.Wait();

                if (_initialized)
                    return;

                //await SvgPlatform.InitializeAsync();

                var context = SynchronizationContext.Current;
                if (context != null)
                {
                    var mainScheduler = new SynchronizationContextScheduler(context);
                    var schedulerProvider = new SchedulerProvider(mainScheduler, NewThreadScheduler.Default);
                    Engine.Register<ISchedulerProvider, SchedulerProvider>(() => schedulerProvider);
                    Engine.RegisterSingleton<IGestureRecognizer, GestureRecognizer>(() => new GestureRecognizer(schedulerProvider));
                }

                Engine.RegisterSingleton<IUndoRedoService, UndoRedoService>(() => new UndoRedoService());

                _initialized = true;
            }
            finally
            {
                _lock.Release();
            }

            return;
        }
    }
}
