﻿using Windows.UI.Xaml.Controls;
using Svg;
using Svg.Editor;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample.Forms.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            SvgPlatform.Init();
            SvgEditor.Init();

            LoadApplication(new Svg.Editor.Sample.Forms.App());
        }
    }
}
