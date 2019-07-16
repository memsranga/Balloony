using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using static Balloony.TouchEffect;

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
        }

        SKPaint sliderSelectedPaint = new SKPaint
        {
            Color = Color.FromHex("#6844bf").ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5
        };
        SKPaint sliderUnSelectedPaint = new SKPaint
        {
            Color = Color.FromHex("#f8f8f8").ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5
        };

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
            canvas.DrawLine(0, sliderY, sliderThumbX, sliderY, sliderSelectedPaint);
            canvas.DrawLine(sliderThumbX, sliderY, info.Width, sliderY, sliderUnSelectedPaint);

            if (_touchType == TouchActionType.Pressed || _touchType == TouchActionType.Moved)
            {
                DrawSelectedThumb(canvas, sliderThumbX, sliderY);
            }
            else
            {
                DrawUnSelectedThumb(canvas, sliderThumbX, sliderY);
            }
        }

        private void DrawUnSelectedThumb(SKCanvas canvas, float sliderThumbX, int sliderY)
        {
            using (var circlePaint = new SKPaint())
            {
                circlePaint.Color = Color.FromHex("#6844bf").ToSKColor();
                circlePaint.IsAntialias = true;
                circlePaint.Style = SKPaintStyle.Fill;
                canvas.DrawRoundRect(sliderThumbX - 25, sliderY - 25, 50, 50, 10, 10, circlePaint);
                circlePaint.Color = Color.White.ToSKColor();
                circlePaint.Style = SKPaintStyle.Fill;
                canvas.DrawRect(sliderThumbX - 10, sliderY - 10, 20, 20, circlePaint);
            }
        }

        private void DrawSelectedThumb(SKCanvas canvas, float sliderThumbX, float sliderY)
        {
            using (var circlePaint = new SKPaint())
            {
                circlePaint.Color = Color.FromHex("#6844bf").ToSKColor();
                circlePaint.IsAntialias = true;
                circlePaint.Style = SKPaintStyle.Stroke;
                circlePaint.StrokeWidth = 10;
                canvas.DrawCircle(new SKPoint(sliderThumbX - 10, sliderY), 20, circlePaint);
                circlePaint.Color = Color.White.ToSKColor();
                circlePaint.Style = SKPaintStyle.Fill;
                canvas.DrawCircle(new SKPoint(sliderThumbX - 10, sliderY), 20, circlePaint);
            }
        }

        private TouchActionType _touchType;
        void Handle_TouchAction(object sender, Balloony.TouchEffect.TouchActionEventArgs args)
        {
            _touchType = args.Type;
            Percent = (float)((args.Location.X / balloon_slider.Width) * 100);
        }
    }
}
