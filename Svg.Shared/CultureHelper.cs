using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Svg.Interfaces;

namespace Svg
{
    public class CultureHelper : ICultureHelper
    {
        public IDisposable UsingCulture(CultureInfo culture)
        {
            return new CultureHelperDisposable(culture);
        }

        private class CultureHelperDisposable : IDisposable
        {
            private CultureInfo _old;

            public CultureHelperDisposable(CultureInfo culture)
            {
                _old = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = culture;
            }

            public void Dispose()
            {
                Thread.CurrentThread.CurrentCulture = _old;
            }
        }
    }
}