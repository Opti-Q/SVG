using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaColors : Colors
    {
        private Color _black;
        private Color _transparent;
        private Color _white;

        private readonly Dictionary<string, Color> _colorsByName;

        public SkiaColors()
        {
            _colorsByName = this.GetType()
                .GetTypeInfo()
                .DeclaredProperties.Where(p => p.PropertyType == typeof(Color))
                .ToDictionary(p => p.Name.ToLowerInvariant(), p => (Color)p.GetValue(this));
        }

        private static SkiaColor From(Color c)
        {
            return new SkiaColor(c.A, c.R, c.G, c.B);
        }

        public Color Lime { get; } = From(SystemColors.Lime);
        public Color Black { get; } = From(SystemColors.Black);
        public Color LimeGreen { get; } = From(SystemColors.LimeGreen);
        public Color BlanchedAlmond { get; } = From(SystemColors.BlanchedAlmond);
        public Color Linen { get; } = From(SystemColors.Linen);
        public Color Blue { get; } = From(SystemColors.Blue);
        public Color Magenta { get; } = From(SystemColors.Magenta);
        public Color BlueViolet { get; } = From(SystemColors.BlueViolet);
        public Color Maroon { get; } = From(SystemColors.Maroon);
        public Color Brown { get; } = From(SystemColors.Brown);
        public Color MediumAquamarine { get; } = From(SystemColors.MediumAquamarine);
        public Color BurlyWood { get; } = From(SystemColors.BurlyWood);
        public Color MediumBlue { get; } = From(SystemColors.MediumBlue);
        public Color CadetBlue { get; } = From(SystemColors.CadetBlue);
        public Color MediumOrchid { get; } = From(SystemColors.MediumOrchid);
        public Color Chartreuse { get; } = From(SystemColors.Chartreuse);
        public Color MediumPurple { get; } = From(SystemColors.MediumPurple);
        public Color Chocolate { get; } = From(SystemColors.Chocolate);
        public Color MediumSeaGreen { get; } = From(SystemColors.MediumSeaGreen);
        public Color Coral { get; } = From(SystemColors.Coral);
        public Color MediumSlateBlue { get; } = From(SystemColors.MediumSlateBlue);
        public Color CornflowerBlue { get; } = From(SystemColors.CornflowerBlue);
        public Color MediumSpringGreen { get; } = From(SystemColors.MediumSpringGreen);
        public Color Cornsilk { get; } = From(SystemColors.Cornsilk);
        public Color MediumTurquoise { get; } = From(SystemColors.MediumTurquoise);
        public Color Crimson { get; } = From(SystemColors.Crimson);
        public Color MediumVioletRed { get; } = From(SystemColors.MediumVioletRed);
        public Color Cyan { get; } = From(SystemColors.Cyan);
        public Color MidnightBlue { get; } = From(SystemColors.MidnightBlue);
        public Color DarkBlue { get; } = From(SystemColors.DarkBlue);
        public Color MintCream { get; } = From(SystemColors.MintCream);
        public Color DarkCyan { get; } = From(SystemColors.DarkCyan);
        public Color MistyRose { get; } = From(SystemColors.MistyRose);
        public Color DarkGoldenrod { get; } = From(SystemColors.DarkGoldenrod);
        public Color Moccasin { get; } = From(SystemColors.Moccasin);
        public Color DarkGray { get; } = From(SystemColors.DarkGray);
        public Color NavajoWhite { get; } = From(SystemColors.NavajoWhite);
        public Color DarkGreen { get; } = From(SystemColors.DarkGreen);
        public Color Navy { get; } = From(SystemColors.Navy);
        public Color DarkKhaki { get; } = From(SystemColors.DarkKhaki);
        public Color OldLace { get; } = From(SystemColors.OldLace);
        public Color DarkMagena { get; } = From(SystemColors.DarkMagena);
        public Color Olive { get; } = From(SystemColors.Olive);
        public Color DarkOliveGreen { get; } = From(SystemColors.DarkOliveGreen);
        public Color OliveDrab { get; } = From(SystemColors.OliveDrab);
        public Color DarkOrange { get; } = From(SystemColors.DarkOrange);
        public Color Orange { get; } = From(SystemColors.Orange);
        public Color DarkOrchid { get; } = From(SystemColors.DarkOrchid);
        public Color OrangeRed { get; } = From(SystemColors.OrangeRed);
        public Color DarkRed { get; } = From(SystemColors.DarkRed);
        public Color Orchid { get; } = From(SystemColors.Orchid);
        public Color DarkSalmon { get; } = From(SystemColors.DarkSalmon);
        public Color PaleGoldenrod { get; } = From(SystemColors.PaleGoldenrod);
        public Color DarkSeaGreen { get; } = From(SystemColors.DarkSeaGreen);
        public Color PaleGreen { get; } = From(SystemColors.PaleGreen);
        public Color DarkSlateBlue { get; } = From(SystemColors.DarkSlateBlue);
        public Color PaleTurquoise { get; } = From(SystemColors.PaleTurquoise);
        public Color DarkSlateGray { get; } = From(SystemColors.DarkSlateGray);
        public Color PaleVioletRed { get; } = From(SystemColors.PaleVioletRed);
        public Color DarkTurquoise { get; } = From(SystemColors.DarkTurquoise);
        public Color PapayaWhip { get; } = From(SystemColors.PapayaWhip);
        public Color DarkViolet { get; } = From(SystemColors.DarkViolet);
        public Color PeachPuff { get; } = From(SystemColors.PeachPuff);
        public Color DeepPink { get; } = From(SystemColors.DeepPink);
        public Color Peru { get; } = From(SystemColors.Peru);
        public Color DeepSkyBlue { get; } = From(SystemColors.DeepSkyBlue);
        public Color Pink { get; } = From(SystemColors.Pink);
        public Color DimGray { get; } = From(SystemColors.DimGray);
        public Color Plum { get; } = From(SystemColors.Plum);
        public Color DodgerBlue { get; } = From(SystemColors.DodgerBlue);
        public Color PowderBlue { get; } = From(SystemColors.PowderBlue);
        public Color Firebrick { get; } = From(SystemColors.Firebrick);
        public Color Purple { get; } = From(SystemColors.Purple);
        public Color FloralWhite { get; } = From(SystemColors.FloralWhite);
        public Color Red { get; } = From(SystemColors.Red);
        public Color ForestGreen { get; } = From(SystemColors.ForestGreen);
        public Color RosyBrown { get; } = From(SystemColors.RosyBrown);
        public Color Fuschia { get; } = From(SystemColors.Fuschia);
        public Color RoyalBlue { get; } = From(SystemColors.RoyalBlue);
        public Color Gainsboro { get; } = From(SystemColors.Gainsboro);
        public Color SaddleBrown { get; } = From(SystemColors.SaddleBrown);
        public Color GhostWhite { get; } = From(SystemColors.GhostWhite);
        public Color Salmon { get; } = From(SystemColors.Salmon);
        public Color Gold { get; } = From(SystemColors.Gold);
        public Color SandyBrown { get; } = From(SystemColors.SandyBrown);
        public Color Goldenrod { get; } = From(SystemColors.Goldenrod);
        public Color SeaGreen { get; } = From(SystemColors.SeaGreen);
        public Color Gray { get; } = From(SystemColors.Gray);
        public Color Seashell { get; } = From(SystemColors.Seashell);
        public Color Green { get; } = From(SystemColors.Green);
        public Color Sienna { get; } = From(SystemColors.Sienna);
        public Color GreenYellow { get; } = From(SystemColors.GreenYellow);
        public Color Silver { get; } = From(SystemColors.Silver);
        public Color Honeydew { get; } = From(SystemColors.Honeydew);
        public Color SkyBlue { get; } = From(SystemColors.SkyBlue);
        public Color HotPink { get; } = From(SystemColors.HotPink);
        public Color SlateBlue { get; } = From(SystemColors.SlateBlue);
        public Color IndianRed { get; } = From(SystemColors.IndianRed);
        public Color SlateGray { get; } = From(SystemColors.SlateGray);
        public Color Indigo { get; } = From(SystemColors.Indigo);
        public Color Snow { get; } = From(SystemColors.Snow);
        public Color Ivory { get; } = From(SystemColors.Ivory);
        public Color SpringGreen { get; } = From(SystemColors.SpringGreen);
        public Color Khaki { get; } = From(SystemColors.Khaki);
        public Color SteelBlue { get; } = From(SystemColors.SteelBlue);
        public Color Lavender { get; } = From(SystemColors.Lavender);
        public Color Tan { get; } = From(SystemColors.Tan);
        public Color LavenderBlush { get; } = From(SystemColors.LavenderBlush);
        public Color Teal { get; } = From(SystemColors.Teal);
        public Color LawnGreen { get; } = From(SystemColors.LawnGreen);
        public Color Thistle { get; } = From(SystemColors.Thistle);
        public Color LemonChiffon { get; } = From(SystemColors.LemonChiffon);
        public Color Tomato { get; } = From(SystemColors.Tomato);
        public Color LightBlue { get; } = From(SystemColors.LightBlue);
        public Color Turquoise { get; } = From(SystemColors.Turquoise);
        public Color LightCoral { get; } = From(SystemColors.LightCoral);
        public Color Violet { get; } = From(SystemColors.Violet);
        public Color LightCyan { get; } = From(SystemColors.LightCyan);
        public Color Wheat { get; } = From(SystemColors.Wheat);
        public Color LightGoldenrodYellow { get; } = From(SystemColors.LightGoldenrodYellow);
        public Color Transparent { get; } = From(SystemColors.Transparent); 
        public Color AliceBlue { get; } = From(SystemColors.AliceBlue);
        public Color LightSalmon { get; } = From(SystemColors.LightSalmon);
        public Color AntiqueWhite { get; } = From(SystemColors.AntiqueWhite);
        public Color LightSeaGreen { get; } = From(SystemColors.LightSeaGreen);
        public Color Aqua { get; } = From(SystemColors.Aqua);
        public Color LightSkyBlue { get; } = From(SystemColors.LightSkyBlue);
        public Color Aquamarine { get; } = From(SystemColors.Aquamarine);
        public Color LightSlateGray { get; } = From(SystemColors.LightSlateGray);
        public Color Azure { get; } = From(SystemColors.Azure);
        public Color LightSteelBlue { get; } = From(SystemColors.LightSteelBlue);
        public Color Beige { get; } = From(SystemColors.Beige);
        public Color LightYellow { get; } = From(SystemColors.LightYellow);
        public Color Bisque { get; } = From(SystemColors.Bisque);
        public Color White { get; } = From(SystemColors.White);
        public Color LightGreen { get; } = From(SystemColors.LightGreen);
        public Color WhiteSmoke { get; } = From(SystemColors.WhiteSmoke);
        public Color LightGray { get; } = From(SystemColors.LightGray);
        public Color Yellow { get; } = From(SystemColors.Yellow);
        public Color LightPink { get; } = From(SystemColors.LightPink);
        public Color YellowGreen { get; } = From(SystemColors.YellowGreen);

        public Color FromName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _colorsByName[name.ToLowerInvariant()];
        }
    }
}