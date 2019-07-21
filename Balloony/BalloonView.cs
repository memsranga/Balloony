using System;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;

namespace Balloony
{
    public class BalloonView : Grid
    {
        static Label progressLabel;
        public BalloonView()
        {
            progressLabel = new Label
            {
                FontSize = 26,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.White,
                Margin = new Thickness(30, 40, 30, 0),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center
            };
            Children.Add(new SvgCachedImage
            {
                Source = "resource://Balloony.Resources.balloon.svg"
            });
            Children.Add(progressLabel);

            HorizontalOptions = LayoutOptions.Start;
            VerticalOptions = LayoutOptions.Start;
            HeightRequest = 150;
            WidthRequest = 150;
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(TextProperty), typeof(string), typeof(BalloonView), string.Empty, propertyChanged: HandlePropertyChanged);

        private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            progressLabel.Text = newValue?.ToString();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}

