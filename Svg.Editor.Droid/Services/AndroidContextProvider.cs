using Android.Content;
using Svg;
using Svg.Droid.Editor.Services;

[assembly:SvgService(typeof(IContextProvider), typeof(AndroidContextProvider))]
namespace Svg.Droid.Editor.Services
{
    public interface IContextProvider
    {
        Context Context { get; }
    }

    public class AndroidContextProvider : IContextProvider
    {
        internal static Context _context;

        public Context Context
        {
            get { return _context; }
        }
    }
}