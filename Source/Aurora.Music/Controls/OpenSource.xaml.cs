﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class OpenSource : ContentDialog
    {
        private const string form = "| {0} | {1} | {2} | {3} |\r\n";
        public OpenSource()
        {
            this.InitializeComponent();
            AssemblyName name = typeof(TagLib.Tag).Assembly.GetName();
            Main.Text += string.Format(form, "`taglib-sharp`", $"`{name.Version.ToVersionString()}`", "[`GNU LGPL v2.1`](https://github.com/mono/taglib-sharp/blob/master/COPYING)", "[`github.com/mono/taglib-sharp`](https://github.com/mono/taglib-sharp)");

            Main.Text += string.Format(form, "`SQLite for Universal Windows Platform`", $"`3.22.0`", "[`Public Domain`](http://www.sqlite.org/copyright.html)", "[`sqlite.org`](http://www.sqlite.org/)");

            name = typeof(SQLite.SQLiteConnection).Assembly.GetName();
            Main.Text += string.Format(form, "`SQLite - net`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/praeclarum/sqlite-net/blob/master/LICENSE.md)", "[`github.com/praeclarum/sqlite-net`](https://github.com/praeclarum/sqlite-net)");

            name = typeof(Microsoft.Toolkit.Uwp.UI.AdvancedCollectionView).Assembly.GetName();
            Main.Text += string.Format(form, "`Json.NET`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)", "[`newtonsoft.com`](https://www.newtonsoft.com/json)");

            name = typeof(ExpressionBuilder.ColorNode).Assembly.GetName();
            Main.Text += string.Format(form, "`ExpressionBuilder`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/Microsoft/WindowsUIDevLabs/blob/master/LICENSE.txt)", "[`github.com/Microsoft/ExpressionBuilder`](https://github.com/Microsoft/WindowsUIDevLabs/tree/master/ExpressionBuilder)");

            name = typeof(ColorThiefDotNet.ColorThief).Assembly.GetName();
            Main.Text += string.Format(form, "`Color Thief.NET`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/KSemenenko/ColorThief/blob/master/LICENSE)", "[`github.com/KSemenenko/ColorThief`](https://github.com/KSemenenko/ColorThief)");

            name = typeof(SmartFormat.Smart).Assembly.GetName();
            Main.Text += string.Format(form, "`SmartFormat`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/scottrippey/SmartFormat.NET/wiki/License)", "[`github.com/scottrippey/SmartFormat.NET`](https://github.com/scottrippey/SmartFormat.NET)");

            name = typeof(Windows.UI.Xaml.Media.Imaging.BitmapFactory).Assembly.GetName();
            Main.Text += string.Format(form, "`WriteableBitmapEx`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/teichgraf/WriteableBitmapEx/blob/master/LICENSE)", "[`github.com/teichgraf/WriteableBitmapEx/`](https://github.com/teichgraf/WriteableBitmapEx/)");

            
            Main.Text += string.Format(form, "`Win2D`", $"`1.21.0`", "[`MIT License`](https://github.com/Microsoft/Win2D/blob/master/LICENSE.txt)", "[`github.com/Microsoft/Win2D`](https://github.com/Microsoft/Win2D)");

            name = typeof(LrcParser.Lyric).Assembly.GetName();
            Main.Text += string.Format(form, "`LrcParser`", $"`{name.Version.ToVersionString()}`", "[`MIT License`](https://github.com/pkzxs/Aurora.Music/blob/master/LICENSE)", "[`github.com/pkzxs/LrcParser`](https://github.com/pkzxs/Aurora.Music/tree/master/Source/LrcParser)");
        }

        private async void Main_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Main.Width = Root.ActualWidth;
            this.SizeChanged += OpenSource_SizeChanged;
        }

        private void OpenSource_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Main.Width = Root.ActualWidth;
        }

        private void ContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OpenSource_SizeChanged;
        }
    }
}
