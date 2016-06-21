using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Svg
{
    /// <summary>
    /// Converts string representations of colours into <see cref="Color"/> objects.
    /// </summary>
    public class SvgColourConverter : System.Drawing.ColorConverter
    {
        /// <summary>
        /// Converts the given object to the converter's native type.
        /// </summary>
        /// <param name="context">A <see cref="T:System.ComponentModel.TypeDescriptor"/> that provides a format context. You can use this object to get additional information about the environment from which this converter is being invoked.</param>
        /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/> that specifies the culture to represent the color.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> representing the converted value.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The conversion cannot be performed.</exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// </PermissionSet>
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string colour = value as string;

            if (colour != null)
            {
                var oldCulture = Thread.CurrentThread.CurrentCulture;
                try 
                { 
            	    Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            	
                    colour = colour.Trim();

                    if (colour.StartsWith("rgb"))
                    {
                        try
                        {
                            int start = colour.IndexOf("(") + 1;
                        
						    //get the values from the RGB string
						    string[] values = colour.Substring(start, colour.IndexOf(")") - start).Split(new char[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);

						    //determine the alpha value if this is an RGBA (it will be the 4th value if there is one)
						    int alphaValue = 255;
						    if (values.Length > 3)
						    {
							    //the alpha portion of the rgba is not an int 0-255 it is a decimal between 0 and 1
							    //so we have to determine the corosponding byte value
							    var alphastring = values[3];
							    if(alphastring.StartsWith("."))
							    {
								    alphastring = "0" + alphastring;
							    }

							    var alphaDecimal = decimal.Parse(alphastring);

							    if(alphaDecimal <= 1)
							    {
								    alphaValue = (int)(alphaDecimal * 255);
							    }
							    else
							    {
								    alphaValue = (int)alphaDecimal;
							    }
						    }

                            Svg.Interfaces.Color colorpart;
                            if (values[0].Trim().EndsWith("%"))
                            {
                                colorpart = Engine.Factory.CreateColorFromArgb(alphaValue, (int)(255 * float.Parse(values[0].Trim().TrimEnd('%')) / 100f),
                                                                                      (int)(255 * float.Parse(values[1].Trim().TrimEnd('%')) / 100f),
                                                                                      (int)(255 * float.Parse(values[2].Trim().TrimEnd('%')) / 100f));
                            }
                            else
                            {
                                colorpart = Engine.Factory.CreateColorFromArgb(alphaValue, int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
                            }

						    return colorpart;
                        }
                        catch
                        {
                            throw new SvgException("Colour is in an invalid format: '" + colour + "'");
                        }
                    }
                    else if (colour.StartsWith("#") && colour.Length == 4)
                    {
                        colour = string.Format(culture, "#{0}{0}{1}{1}{2}{2}", colour[1], colour[2], colour[3]);
                        return CreateColor((System.Drawing.Color)base.ConvertFrom(context, culture, colour));
                    }
                
                    switch (colour.ToLowerInvariant())
                    {
                        case "activeborder": return CreateColor(SystemColors.ActiveBorder);
                        case "activecaption": return CreateColor(SystemColors.ActiveCaption);
                        case "appworkspace": return CreateColor(SystemColors.AppWorkspace);
                        case "background": return CreateColor(SystemColors.Desktop);
                        case "buttonface": return CreateColor(SystemColors.Control);
                        case "buttonhighlight": return CreateColor(SystemColors.ControlLightLight);
                        case "buttonshadow": return CreateColor(SystemColors.ControlDark);
                        case "buttontext": return CreateColor(SystemColors.ControlText);
                        case "captiontext": return CreateColor(SystemColors.ActiveCaptionText);
                        case "graytext": return CreateColor(SystemColors.GrayText);
                        case "highlight": return CreateColor(SystemColors.Highlight);
                        case "highlighttext": return CreateColor(SystemColors.HighlightText);
                        case "inactiveborder": return CreateColor(SystemColors.InactiveBorder);
                        case "inactivecaption": return CreateColor(SystemColors.InactiveCaption);
                        case "inactivecaptiontext": return CreateColor(SystemColors.InactiveCaptionText);
                        case "infobackground": return CreateColor(SystemColors.Info);
                        case "infotext": return CreateColor(SystemColors.InfoText);
                        case "menu": return CreateColor(SystemColors.Menu);
                        case "menutext": return CreateColor(SystemColors.MenuText);
                        case "scrollbar": return CreateColor(SystemColors.ScrollBar);
                        case "threeddarkshadow": return CreateColor(SystemColors.ControlDarkDark);
                        case "threedface": return CreateColor(SystemColors.Control);
                        case "threedhighlight": return CreateColor(SystemColors.ControlLight);
                        case "threedlightshadow": return CreateColor(SystemColors.ControlLightLight);
                        case "window": return CreateColor(SystemColors.Window);
                        case "windowframe": return CreateColor(SystemColors.WindowFrame);
                        case "windowtext": return CreateColor(SystemColors.WindowText);
                    }

                    if (!colour.StartsWith("#"))
                    {
                        return CreateColor(System.Drawing.Color.FromName(colour.ToLowerInvariant()));
                    }
                }
                finally 
                { 
                    Thread.CurrentThread.CurrentCulture = oldCulture;
                }
            }

            return CreateColor((System.Drawing.Color)base.ConvertFrom(context, culture, value));
        }

        private Svg.Interfaces.Color CreateColor(System.Drawing.Color c)
        {
            return Engine.Factory.CreateColorFromArgb(c.A, c.R, c.G, c.B);
        }

        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var colour = (Svg.Interfaces.Color)value;
                return "#" + colour.R.ToString("X2", null) + colour.G.ToString("X2", null) + colour.B.ToString("X2", null);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}