using System;
using System.Drawing;

namespace Svg.Platform
{
    public class AndroidStringFormat : StringFormat
    {
        public AndroidStringFormat()
        {
        }

        public StringFormatFlags FormatFlags { get; set; }
        public void SetMeasurableCharacterRanges(CharacterRange[] characterRanges)
        {
            throw new NotImplementedException();
        }
    }
}