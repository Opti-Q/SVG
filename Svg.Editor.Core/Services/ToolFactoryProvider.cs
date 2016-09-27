using System;
using System.Collections.Generic;
using Svg.Core.Tools;

namespace Svg.Core.Services
{
    public class ToolFactoryProvider
    {
        public IEnumerable<Func<ITool>> ToolFactories { get; }

        public ToolFactoryProvider(IEnumerable<Func<ITool>> toolFactories)
        {
            ToolFactories = toolFactories;
        }
    }
}