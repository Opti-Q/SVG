using System.Drawing;
using Android.Graphics;

namespace Svg.Platform
{
    public static class Extensions
    {
        public static Shader.TileMode ToTileMode(this WrapMode wrapMode)
        {
            Shader.TileMode tileMode = Shader.TileMode.Clamp;
            switch (wrapMode)
            {
                case WrapMode.Clamp:
                    tileMode = Shader.TileMode.Clamp;
                    break;
                case WrapMode.Tile:
                    tileMode = Shader.TileMode.Repeat;
                    break;
                case WrapMode.TileFlipX:
                case WrapMode.TileFlipXY:
                case WrapMode.TileFlipY:
                    tileMode = Shader.TileMode.Mirror;
                    break;
            }
            return tileMode;
        }

        public static Android.Graphics.TypefaceStyle ToTypefaceStyle(this FontStyle value)
        {
            var tfs = TypefaceStyle.Normal;

            if ((value & FontStyle.Bold) == FontStyle.Bold &&
                (value & FontStyle.Italic) == FontStyle.Italic)
                tfs = TypefaceStyle.BoldItalic;
            else if ((value & FontStyle.Bold) == FontStyle.Bold)
                tfs = TypefaceStyle.Bold;
            else if ((value & FontStyle.Italic) == FontStyle.Italic)
                tfs = TypefaceStyle.Italic;
            else if ((value & FontStyle.Regular) == FontStyle.Regular)
                tfs = TypefaceStyle.Normal;

            return tfs;
        }
    }
}