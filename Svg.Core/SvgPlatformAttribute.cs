using System;

namespace Svg
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class SvgPlatformAttribute : Attribute
    {
        public Type SvgPlatformSetupType { get; private set; }

        public SvgPlatformAttribute(Type svgPlatformSetupType)
        {
            SvgPlatformSetupType = svgPlatformSetupType;
        }
    }
}
