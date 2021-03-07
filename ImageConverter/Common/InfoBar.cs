using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace ImageConverter.Common
{
    public sealed class InfoBar : ContentControl
    {
        public FrameworkElement Icon
        {
            get { return (FrameworkElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(FrameworkElement), typeof(InfoBar), new PropertyMetadata(null));

        public bool IsCloseButtonVisible
        {
            get { return (bool)GetValue(IsCloseButtonVisibleProperty); }
            set { SetValue(IsCloseButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsCloseButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsCloseButtonVisible), typeof(bool), typeof(InfoBar), new PropertyMetadata(true, (d,e) =>
            {
                ((InfoBar)d).UpdateCloseStates(true);
            }));


        private Button _closeButton = null;

        public InfoBar()
        {
            this.DefaultStyleKey = typeof(InfoBar);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _closeButton = this.GetTemplateChild("CloseButton") as Button;
            if (_closeButton != null)
            {
                _closeButton.Click -= _closeButton_Click;
                _closeButton.Click += _closeButton_Click;
            }

            UpdateCloseStates(false);
        }

        void UpdateCloseStates(bool animate)
        {
            VisualStateManager.GoToState(this,
                IsCloseButtonVisible ? "CloseButtonVisibleState" : "CloseButtonHiddenState", animate);
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
