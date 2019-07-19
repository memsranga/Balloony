using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using static Balloony.TouchEffect;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using System.IO;
using System.Reflection;

namespace Balloony
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            SliderSelectedPaint = new SKPaint
            {
                Color = Color.FromHex("#6844bf").ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = SliderHeight
            };

            SliderUnSelectedPaint = new SKPaint
            {
                Color = Color.FromHex("#f8f8f8").ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = SliderHeight
            };

            ThumbPaint = new SKPaint
            {
                Color = Color.FromHex("#6844bf").ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            ThumbSelectedPaint = new SKPaint
            {
                Color = Color.FromHex("#6844bf").ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = SelectedThumbThickness
            };
            ThumbSelectedSubtractPaint = new SKPaint
            {
                Color = Color.Transparent.ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0,
                BlendMode = SKBlendMode.Src
            };
            ThumbSubtractPaint = new SKPaint
            {
                Color = Color.Transparent.ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0,
                BlendMode = SKBlendMode.Src
            };
            balloon = new SKSvg();
            balloon.Load(Resolve("resource://Balloony.Resources.balloon.svg"));
        }

        SKSvg balloon;
        float SliderHeight = 5;
        float SelectedThumbThickness = 10;
        float ThumbSize = 50;
        SKPaint SliderSelectedPaint { get; set; }
        SKPaint SliderUnSelectedPaint { get; set; }
        SKPaint ThumbPaint { get; set; }
        SKPaint ThumbSubtractPaint { get; set; }
        SKPaint ThumbSelectedPaint { get; set; }
        SKPaint ThumbSelectedSubtractPaint { get; set; }



        private float _percent;

        public float Percent
        {
            get => _percent;
            private set
            {
                _percent = value;
                balloon_slider.InvalidateSurface();
            }
        }

        void Handle_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;

            canvas.Clear();

            var sliderThumbX = (float)(info.Width * Percent / 100);
            var sliderY = info.Height - 50;
            DrawSlider(canvas, info, Percent);
            DrawThumb(canvas, info, Percent, _touchType);
            DrawBalloon(canvas, info, Percent);
        }

        private void DrawThumb(SKCanvas canvas, SKImageInfo info, float percent, TouchActionType touchActionType)
        {
            var y = info.Height - ThumbSize - SelectedThumbThickness;
            var center = info.Width * percent / 100;

            if (touchActionType == TouchActionType.Pressed || touchActionType == TouchActionType.Moved)
            {
                // selected thumb
                var radius = ThumbSize * 0.5f; // 50% of size
                canvas.DrawCircle(center, y, radius, ThumbSelectedPaint);
                canvas.DrawCircle(center, y, radius, ThumbSelectedSubtractPaint);
                return;
            }

            //default thumb
            var startX = center - ThumbSize / 2;
            var startY = y - ThumbSize / 2;
            var cornerRadius = ThumbSize * 0.4f; // 40% of size
            var innerRadius = ThumbSize / 2 * .5f; // 50 % of side
            canvas.DrawRoundRect(startX, startY, ThumbSize, ThumbSize, cornerRadius, cornerRadius, ThumbPaint);
            canvas.DrawCircle(center, y, innerRadius, ThumbSubtractPaint);
        }

        private void DrawBalloon(SKCanvas canvas, SKImageInfo info, float percent)
        {
            var y = info.Height - ThumbSize - SelectedThumbThickness;
            var center = info.Width * percent / 100;
            var picture = balloon.Picture;
            var scale = 2;
            var balloonCenter = center - (scale * picture.CullRect.Width / 2);
            var balloonY = y - (scale * picture.CullRect.Height) - 50;

            var matrix = new SKMatrix();
            matrix.SetScaleTranslate(scale, scale, balloonCenter, balloonY);
            canvas.DrawPicture(balloon.Picture, ref matrix);
        }

        private TouchActionType _touchType;
        void Handle_TouchAction(object sender, Balloony.TouchEffect.TouchActionEventArgs args)
        {
            _touchType = args.Type;
            Percent = (float)((args.Location.X / balloon_slider.Width) * 100);
        }



        // Bindable Property - Selected Color - better naming
        // Bindable Property - UnSelected Color - better naming
        private void DrawSlider(SKCanvas canvas, SKImageInfo info, float percent)
        {
            var y = info.Height - ThumbSize - SelectedThumbThickness; // minus the thumb radius, minus thumb thickness
            var selectX = info.Width * percent / 100;
            canvas.DrawLine(0, y, selectX, y, SliderSelectedPaint);
            canvas.DrawLine(selectX, y, info.Width, y, SliderUnSelectedPaint);
        }

        private Stream Resolve(string identifier)
        {
            if (!identifier.StartsWith("resource://", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only resource:// scheme is supported");


            var uri = new Uri(identifier);
            Assembly assembly = null;

            var parts = uri.OriginalString.Substring(11).Split('?');
            var resourceName = parts.First();

            if (parts.Count() > 1)
            {
                var name = Uri.UnescapeDataString(uri.Query.Substring(10));
                var assemblyName = new AssemblyName(name);
                assembly = Assembly.Load(assemblyName);
            }

            if (assembly == null)
            {
                var callingAssemblyMethod = typeof(Assembly).GetTypeInfo().GetDeclaredMethod("GetCallingAssembly");
                assembly = (Assembly)callingAssemblyMethod.Invoke(null, new object[0]);
            }

            return assembly.GetManifestResourceStream(resourceName);
        }

    }
}
