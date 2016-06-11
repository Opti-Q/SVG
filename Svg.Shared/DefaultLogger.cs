using System;
using System.Diagnostics;
using System.Net;
using Svg.Interfaces;

namespace Svg
{
    public class DefaultLogger : ILogger
    {
        public void Info(string txt)
        {
            Trace.Write(txt);
        }

        public void Warn(string txt)
        {
            Trace.Write(txt);
        }

        public void Error(string txt)
        {
            Trace.TraceError(txt);
        }

        public void Fatal(string txt)
        {
            Trace.TraceError(txt);
        }
    }
}