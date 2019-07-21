using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using static Balloony.TouchEffect;

namespace Balloony
{
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

            Percent = 50;
        }

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
                TranslateBalloon(Percent);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            balloonSvg.AnchorY = 1;
            balloonSvg.TranslationX = (balloon_slider.Width * Percent / 100) - balloonSvg.Width / 2;
            balloonSvg.Scale = 0;
            balloonSvg.TranslationY = balloon_slider.Height - balloonSvg.Height;
        }

        void Handle_Slider_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;

            canvas.Clear();

            DrawSlider(canvas, info, Percent);
            DrawThumb(canvas, info, Percent, _touchType);
        }

        private void TranslateBalloon(float percent)
        {
            if (this.AnimationIsRunning("TranslationAnimation"))
            {
                return;
            }

            var oldX = balloonSvg.TranslationX;
            var newX = balloon_slider.Width * percent / 100 - balloonSvg.Width / 2;


            var translation = new Animation();
            translation.Add(0, 1, new Animation((s) =>
            {
                if (oldX > newX)
                {
                    var delta = oldX - s * Math.Abs(oldX - newX);
                    balloonSvg.TranslationX = delta;
                }
                else
                {
                    var delta = oldX + s * Math.Abs(oldX - newX);
                    balloonSvg.TranslationX = delta;
                }
            }, 0, 1));
            translation.Add(0, 1, new Animation(s =>
            {
                if (oldX > newX)
                {
                    var delta = oldX - s * Math.Abs(oldX - newX);
                    var angle = Math.Abs(oldX - newX) > 0.001 ? Math.Tanh((oldX - newX) / balloon_slider.Width) : 0;
                    balloonSvg.Rotation = angle * 180;
                }
                else
                {
                    var delta = oldX + s * Math.Abs(oldX - newX);
                    var angle = Math.Abs(oldX - newX) > 0.001 ? Math.Tanh((oldX - newX) / balloon_slider.Width) : 0;
                    balloonSvg.Rotation = angle * 180;
                }
            }, 0, 1, finished: () =>
            {
                balloonSvg.RelRotateTo(-balloonSvg.Rotation, 500);
            }));
            translation.Commit(balloonSvg, "TranslationAnimation", length: 100);
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

        private TouchActionType _touchType;

        void Handle_TouchAction(object sender, Balloony.TouchEffect.TouchActionEventArgs args)
        {
            _touchType = args.Type;
            if(this.AnimationIsRunning("FloatAnimation") || this.AnimationIsRunning("DropAnimation"))
            {
                return;
            }

            if (_touchType == TouchActionType.Pressed || _touchType == TouchActionType.Entered)
            {
                var floatAnimation = new Animation();
                floatAnimation.Add(0, 1, new Animation((s) =>
                {
                    balloonSvg.Scale = s;
                    balloonSvg.TranslationY = (balloon_slider.Height - balloonSvg.Height) - s * 100;
                }, 0, 1));
                floatAnimation.Commit(balloonSvg, "FloatAnimation");
            }
            else if (_touchType == TouchActionType.Released || _touchType == TouchActionType.Exited)
            {
                var dropAnimation = new Animation();
                dropAnimation.Add(0, 1, new Animation((s) =>
                {
                    balloonSvg.Scale = s;
                    balloonSvg.TranslationY = (balloon_slider.Height - balloonSvg.Height) - s * 100;
                }, 1, 0));
                dropAnimation.Commit(balloonSvg, "DropAnimation");
            }
            Percent = (float)((args.Location.X / balloon_slider.Width) * 100);
        }

        private void DrawSlider(SKCanvas canvas, SKImageInfo info, float percent)
        {
            var y = info.Height - ThumbSize - SelectedThumbThickness; // minus the thumb radius, minus thumb thickness
            var selectX = info.Width * percent / 100;
            canvas.DrawLine(0, y, selectX, y, SliderSelectedPaint);
            canvas.DrawLine(selectX, y, info.Width, y, SliderUnSelectedPaint);
        }
    }
}
