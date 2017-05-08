﻿using System.Threading.Tasks;
using Svg.Editor.Gestures;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Forms.Tools
{
	public class AddItemTool : ToolBase
	{
		public AddItemTool() : base("Add item", null)
		{
		}

		protected override async Task OnLongPress(LongPressGesture longPress)
		{
			await base.OnLongPress(longPress);

			await Canvas.AddItemInScreenCenter(new SvgCircle { Radius = 10 });
		}
	}
}
