using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Svg.Droid.Editor.Interfaces;
using Svg.Droid.Editor.Tools;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
{
    public class SelectionService : ISelectionService
    {
        // Size of the invisible box where the user may click inside an object to select it
        private const float SelectionSize = 1f;

        public SelectableAndroidBitmap SelectedItem { get; set; }

        //-----------------------------------------
        // POSITION
        //-----------------------------------------
        public SelectableAndroidBitmap SelectNewItem(IEnumerable<SelectableAndroidBitmap> allBitmaps, int pointerX, int pointerY)
        {
            var point = new Point(pointerX, pointerY);
            return allBitmaps.FirstOrDefault(bitmap =>
            {
                /*
                 * Paul Zieger:
                 * Hauptproblem um eine erste Zeichnung zu machen sehe ich:
                 * in der �berlagerung der Bilder und da man dadurch keine feinen Positionierungen machen kann.
                 * W�re es vielleicht m�glich die Aktivierungsfl�che jede Elements nur auf die Mitte zu reduzieren?
                 */

                var rect = new Rectangle();

                var centerX = bitmap.X + bitmap.Width / 2;
                var centerY = bitmap.Y + bitmap.Height / 2;

                rect.X = (int) (centerX - ((bitmap.Width * ZoomTool.ScaleFactor * SelectionSize) / 2));
                rect.Y = (int) (centerY - ((bitmap.Height * ZoomTool.ScaleFactor * SelectionSize) / 2));
                rect.Width = (int) (bitmap.Width * ZoomTool.ScaleFactor * SelectionSize);
                rect.Height = (int) (bitmap.Height * ZoomTool.ScaleFactor * SelectionSize);

                return rect.Contains(point);
            });
        }

        public bool IsInRangeOfSelected(SelectableAndroidBitmap selected, int pointerX, int pointerY)
        {
            var point = new Point(pointerX, pointerY);

            var rect = selected.Rect;

            rect.X -= SelectionTool.SelectionBoxPadding;
            rect.Y -= SelectionTool.SelectionBoxPadding;
            rect.Width += 2 * SelectionTool.SelectionBoxPadding;
            rect.Height += 2 * SelectionTool.SelectionBoxPadding;

            return rect.Contains(point);
        }
    }
}