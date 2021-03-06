﻿using System;
using System.Linq;
using System.Windows.Input;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace StarButton
{
    public sealed class StarButton : Control
    {

        public StarButton()
        {
            this.DefaultStyleKey = typeof(StarButton);
            Loaded += StarButton_Loaded;
            Unloaded += StarButton_Unloaded;
        }

        private void StarButton_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= StarButton_Loaded;
            Unloaded -= StarButton_Unloaded;
        }

        private void StarButton_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private FontIcon Icon;
        private Grid Root;
        private Ellipse Bloom;

        private object lockable = new object();

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Icon = GetTemplateChild("Icon") as FontIcon;
            Root = GetTemplateChild("Root") as Grid;
            Icon.Glyph = string.IsNullOrWhiteSpace(IconSymbol) ? "\uE00B" : IconSymbol.First().ToString();
            Bloom = GetTemplateChild("Bloom") as Ellipse;

            Root.PointerEntered += Root_PointerEntered;
            Root.PointerExited += Root_PointerExited;
            Root.PointerCanceled += Root_PointerExited;
            Root.PointerCaptureLost += Root_PointerExited;
            Root.PointerReleased += Root_PointerReleased;
            Root.PointerPressed += Root_PointerPressed;
        }

        private void Root_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            lock (lockable)
            {
                (Root.Resources["Pressed"] as Storyboard).Begin();
            }
        }

        private void Root_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IsOn = !IsOn;
            lock (lockable)
            {
                if (IsOn)
                {
                    var brush = new SolidColorBrush((Foreground as SolidColorBrush).Color)
                    {
                        Opacity = 0.666666
                    };
                    ((Root.Resources["PointerOverOn"] as Storyboard).Children[0] as ObjectAnimationUsingKeyFrames).KeyFrames[0].Value = brush;
                    (Root.Resources["PointerOverOn"] as Storyboard).Begin();
                }
                else
                {
                    (Root.Resources["PointerOverOff"] as Storyboard).Begin();
                }
            }
            Command?.Execute(null);
        }

        private void Root_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            lock (lockable)
            {
                if (IsOn)
                {
                    ((Root.Resources["NormalOn"] as Storyboard).Children[0] as ObjectAnimationUsingKeyFrames).KeyFrames[0].Value = (Foreground as SolidColorBrush);
                    (Root.Resources["NormalOn"] as Storyboard).Begin();
                }
                else
                {
                    ((Root.Resources["NormalOff"] as Storyboard).Children[0] as ObjectAnimationUsingKeyFrames).KeyFrames[0].Value = (Background as SolidColorBrush);
                    (Root.Resources["NormalOff"] as Storyboard).Begin();
                }
            }
        }

        private void Root_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            lock (lockable)
            {
                if (IsOn)
                {
                    var brush = new SolidColorBrush((Foreground as SolidColorBrush).Color)
                    {
                        Opacity = 0.666666
                    };
                    ((Root.Resources["PointerOverOn"] as Storyboard).Children[0] as ObjectAnimationUsingKeyFrames).KeyFrames[0].Value = brush;
                    (Root.Resources["PointerOverOn"] as Storyboard).Begin();
                }
                else
                {
                    (Root.Resources["PointerOverOff"] as Storyboard).Begin();
                }
            }
        }

        /// <summary>
        /// Specified the Icon(Segoe MDL2 Assets FontIcon), only use the first char of <see cref="IconSymbol"/>
        /// </summary>
        public string IconSymbol
        {
            get { return (string)GetValue(IconSymbolProperty); }
            set { SetValue(IconSymbolProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IconSymbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconSymbolProperty =
            DependencyProperty.Register("IconSymbol", typeof(string), typeof(StarButton), new PropertyMetadata("\uE00B", OnIconSymbolChanged));
        private static void OnIconSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var btn = d as StarButton;
            if (btn.Icon == null)
            {
                return;
            }
            btn.Icon.Glyph = string.IsNullOrWhiteSpace(e.NewValue as string) ? "\uE00B" : (e.NewValue as string).First().ToString();
        }

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(StarButton), new PropertyMetadata(false, OnIsOnChanged));

        public ICommand Command { get; set; }

        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var btn = d as StarButton;
            if (btn.Icon == null)
            {
                return;
            }
            lock (btn.lockable)
            {
                if ((bool)e.NewValue)
                {
                    btn.BeginToOn();
                }
                else
                {
                    btn.BeginToOff();
                }
            }
        }

        private void BeginToOff()
        {
            var ani = Root.Resources["Off"] as Storyboard;
            var color = ani.Children[0] as ColorAnimation;
            color.To = (Background as SolidColorBrush).Color;
            ani.Begin();
        }

        public void BeginToOn()
        {
            var ani = Root.Resources["On"] as Storyboard;
            var iconColor = ani.Children.First(x =>
            {
                if (x is ColorAnimation c)
                {
                    return c.GetValue(Storyboard.TargetNameProperty) as string == "Icon";
                }
                return false;
            });
            (iconColor as ColorAnimation).From = (Background as SolidColorBrush).Color;
            (iconColor as ColorAnimation).To = (Foreground as SolidColorBrush).Color;

            var bloomColor = ani.Children.First(x =>
            {
                if (x is ColorAnimation c)
                {
                    return c.GetValue(Storyboard.TargetNameProperty) as string == "Bloom";
                }
                return false;
            });

            (bloomColor as ColorAnimation).From = (Foreground as SolidColorBrush).Color;

            ColorToHSV((Foreground as SolidColorBrush).Color, out var h, out var s, out var v);
            h -= 60;
            if (h < 0)
            {
                h += 360;
            }
            (bloomColor as ColorAnimation).To = ColorFromHSV(h, s, v);


            var stroke = ani.Children.First(x =>
            {
                if (x is DoubleAnimationUsingKeyFrames c)
                {
                    return c.GetValue(Storyboard.TargetNameProperty) as string == "Bloom";
                }
                return false;
            });
            var p = Icon.FontSize;
            (stroke as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = p / 2;
            (stroke as DoubleAnimationUsingKeyFrames).KeyFrames[1].Value = p / 2;
            Bloom.Width = p;
            Bloom.Height = p;

            var i = ani.Children.IndexOf(ani.Children.First(x =>
            {
                if (x is DoubleAnimation c)
                {
                    return c.GetValue(Storyboard.TargetNameProperty) as string == "L0Trans";
                }
                return false;
            }));

            var a = h - 18;
            var b = h - 36;
            if (a < 0)
            {
                a += 360;
            }
            if (b < 0)
            {
                b += 360;
            }

            for (var j = 0; j < 16; i++, j++)
            {
                var transX = ani.Children[i] as DoubleAnimation;
                var transY = ani.Children[i + 16] as DoubleAnimation;
                var tangle = 0.7853981633974483096156608465 * j;
                if (j < 8)
                {
                    // L
                    transX.To = p * 1.5 * Math.Sin(tangle - 0.0872664625997164788461845385);
                    transY.To = -p * 1.5 * Math.Cos(tangle - 0.0872664625997164788461845385);
                    var le = (GetTemplateChild($"L{j}") as Ellipse);
                    le.Width = p / 4d;
                    le.Height = p / 4d;

                    (le.Fill as SolidColorBrush).Color = ColorFromHSV(a, s, v);
                }
                else
                {
                    // R
                    transX.To = p * 1.7 * Math.Sin(tangle + 0.0872664625997164788461845385);
                    transY.To = -p * 1.7 * Math.Cos(tangle + 0.0872664625997164788461845385);
                    var re = (GetTemplateChild($"R{j - 8}") as Ellipse);
                    re.Width = p / 3d;
                    re.Height = p / 3d;

                    (re.Fill as SolidColorBrush).Color = ColorFromHSV(b, s, v);
                }
            }

            // TODO: adjust speed
            ani.SpeedRatio = 1.2;

            ani.Begin();
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            if (value > 255)
                value = 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            float hsbB = max / 255.0f;
            float hsbS = max == 0 ? 0 : (max - min) / (float)max;

            float hsbH = 0;
            if (max == color.R && color.G >= color.B)
            {
                hsbH = (color.G - color.B) * 60f / (max - min) + 0;
            }
            else if (max == color.R && color.G < color.B)
            {
                hsbH = (color.G - color.B) * 60f / (max - min) + 360;
            }
            else if (max == color.G)
            {
                hsbH = (color.B - color.R) * 60f / (max - min) + 120;
            }
            else if (max == color.B)
            {
                hsbH = (color.R - color.G) * 60f / (max - min) + 240;
            }
            hue = hsbH;
            saturation = hsbS;
            value = hsbB;
        }

    }
}
