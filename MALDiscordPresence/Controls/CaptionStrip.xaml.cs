using System.Windows;
using System.Windows.Controls;

namespace MALDiscordPresence.Controls;

public partial class CaptionStrip : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), 
            typeof(string), 
            typeof(CaptionStrip), 
            new PropertyMetadata(string.Empty, OnTitleChanged));

    public static readonly DependencyProperty DisplayTitleProperty =
        DependencyProperty.Register(
            nameof(DisplayTitle), 
            typeof(string), 
            typeof(CaptionStrip), 
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string DisplayTitle
    {
        get => (string)GetValue(DisplayTitleProperty);
        private set => SetValue(DisplayTitleProperty, value);
    }

    public CaptionStrip()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CaptionStrip control)
        {
            control.DisplayTitle = e.NewValue?.ToString()?.ToUpperInvariant() ?? string.Empty;
        }
    }
}
