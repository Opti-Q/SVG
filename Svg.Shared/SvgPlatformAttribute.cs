using System;

namespace Svg
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public class SvgPlatformAttribute : Attribute
    {
        public Type PlatformSetup { get; private set; }

        public SvgPlatformAttribute(Type platformSetup)
        {
            PlatformSetup = platformSetup;
        }
    }
}
