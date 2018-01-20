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
    public enum DescriptionStackingMode
    {
        Hidden,
        BelowHeader,
        BelowContent
    }

    public sealed class OptionPresenter : ContentControl
    {
        public DescriptionStackingMode DescriptionStackingMode
        {
            get { return (DescriptionStackingMode)GetValue(DescriptionStackingModeProperty); }
            set { SetValue(DescriptionStackingModeProperty, value); }
        }

        public static readonly DependencyProperty DescriptionStackingModeProperty =
            DependencyProperty.Register(nameof(DescriptionStackingMode), typeof(DescriptionStackingMode), typeof(OptionPresenter), new PropertyMetadata(DescriptionStackingMode.BelowHeader, (d, e) =>
        {
            ((OptionPresenter)d).UpdateStates();
        }));
        

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(OptionPresenter), new PropertyMetadata(null));


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(OptionPresenter), new PropertyMetadata(null));




        public OptionPresenter()
        {
            this.DefaultStyleKey = typeof(OptionPresenter);
            UpdateStates();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateStates();
        }

        private void UpdateStates()
        {
            if (DescriptionStackingMode == DescriptionStackingMode.Hidden)
                VisualStateManager.GoToState(this, "DescriptionHiddenState", false);
            else if (DescriptionStackingMode == DescriptionStackingMode.BelowHeader)
                VisualStateManager.GoToState(this, "DescriptionBelowHeaderState", false);
            else
                VisualStateManager.GoToState(this, "DescriptionBelowContentState", false);
        }
    }
}
