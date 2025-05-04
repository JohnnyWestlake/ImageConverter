namespace ImageConverter.Views;

public class TransformViewModel : BindableBase
{
    public bool ConstrainWidth { get => GetV(false); set => Set(value); }
    public bool ConstrainHeight { get => GetV(false); set => Set(value); }

    public string WidthConstraintText { get => Get<string>(); set => Set(value); }
    public string HeightConstraintText { get => Get<string>(); set => Set(value); }


    public void ApplyTo(BitmapConversionSettings settings)
    {
        if (ConstrainWidth && !string.IsNullOrWhiteSpace(WidthConstraintText))
        {
            int width = Convert.ToInt32(WidthConstraintText, 10);
            if (width > 0)
                settings.ScaledWidth = width;
        }

        if (ConstrainHeight && !string.IsNullOrWhiteSpace(HeightConstraintText))
        {
            int height = Convert.ToInt32(HeightConstraintText, 10);
            if (height > 0)
                settings.ScaledHeight = height;
        }
    }
}
