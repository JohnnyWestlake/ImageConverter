using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace ImageConverter.Common
{
    public class BlurBrush : XamlCompositionBrushBase
    {
        #region Dependency Properties

        #region BlurAmount

        public double BlurAmount
        {
            get { return (double)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }

        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(BlurBrush), new PropertyMetadata(3d, (d, e) =>
            {
                ((BlurBrush)d).UpdateBlur();
            }));

        #endregion

        #region TintColor

        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }

        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.Register(nameof(TintColor), typeof(Color), typeof(BlurBrush), new PropertyMetadata(Colors.Black, (d, e) =>
            {
                ((BlurBrush)d).UpdateColor();
            }));

        #endregion

        #region TintOpacity

        public double TintOpacity
        {
            get { return (double)GetValue(TintOpacityProperty); }
            set { SetValue(TintOpacityProperty, value); }
        }

        public static readonly DependencyProperty TintOpacityProperty =
            DependencyProperty.Register(nameof(TintOpacity), typeof(double), typeof(BlurBrush), new PropertyMetadata(0.8d, (d, e) =>
            {
                ((BlurBrush)d).UpdateColor();
            }));

        #endregion

        #endregion

        Compositor _compositor => Window.Current.Compositor;

        static CompositionEffectFactory _effectFactory = null;

        protected override void OnConnected()
        {
            base.OnConnected();
            Create();
        }

        private void Create()
        {
            if (_effectFactory == null)
            {
                var color = new ColorSourceEffect
                {
                    Name = "Colour",
                    Color = this.TintColor
                };

                var opacity = new OpacityEffect
                {
                    Name = "Opacity",
                    Opacity = 0f,
                    Source = color
                };

                var blurEffect = new GaussianBlurEffect
                {
                    Name = "Blur",
                    BlurAmount = 0,
                    Optimization = EffectOptimization.Speed,
                    BorderMode = EffectBorderMode.Hard,
                    BufferPrecision = Microsoft.Graphics.Canvas.CanvasBufferPrecision.Precision16Float,
                    Source = new CompositionEffectSourceParameter("Source")
                };

                var blend = new BlendEffect
                {
                    Background = blurEffect,
                    Foreground = opacity,
                    Mode = BlendEffectMode.SoftLight,
                };

                _effectFactory = _compositor.CreateEffectFactory(
                    blend, new string[] { "Blur.BlurAmount", "Colour.Color", "Opacity.Opacity" });
            }

            var brush = _effectFactory.CreateBrush();
            brush.SetSourceParameter("Source", _compositor.CreateHostBackdropBrush());
            this.CompositionBrush = brush;
            UpdateBlur();
            UpdateColor();
        }

        private void UpdateBlur()
        {
            CompositionBrush?.Properties.InsertScalar("Blur.BlurAmount", (float)BlurAmount);
        }

        private void UpdateColor()
        {
            CompositionBrush?.Properties.InsertColor("Colour.Color", TintColor);
            CompositionBrush?.Properties.InsertScalar("Opacity.Opacity", (float)TintOpacity);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.CompositionBrush?.Dispose();
        }
    }
}
