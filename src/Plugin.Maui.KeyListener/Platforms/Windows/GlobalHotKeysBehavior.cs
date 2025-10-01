namespace Plugin.Maui.KeyListener;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Application = Application;
using Window = Window;

public partial class GlobalHotKeysBehavior : PlatformBehavior<VisualElement>
{
    // Tracks how many behaviors are attached per root element
    private static readonly Dictionary<FrameworkElement, int> SubscriptionCounts = new();

    protected override void OnAttachedTo(VisualElement bindable, FrameworkElement platformView)
    {
        base.OnAttachedTo(bindable, platformView);

        ScopedElement = bindable;

        foreach (Window mauiWindow in Application.Current?.Windows ?? Enumerable.Empty<Window>())
        {
            Microsoft.UI.Xaml.Window nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            FrameworkElement? rootElement = nativeWindow?.Content as FrameworkElement;
            if (rootElement != null)
            {
                if (!SubscriptionCounts.TryGetValue(rootElement, out int count) || count == 0)
                {
                    rootElement.KeyDown += OnKeyDown;
                    rootElement.KeyUp += OnKeyUp;
                }
                SubscriptionCounts[rootElement] = count + 1;
            }
        }
    }

    protected override void OnDetachedFrom(VisualElement bindable, FrameworkElement platformView)
    {
        base.OnDetachedFrom(bindable, platformView);

        foreach (Window mauiWindow in Application.Current?.Windows ?? Enumerable.Empty<Window>())
        {
            Microsoft.UI.Xaml.Window nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            FrameworkElement? rootElement = nativeWindow?.Content as FrameworkElement;
            if (rootElement != null && SubscriptionCounts.TryGetValue(rootElement, out int count))
            {
                if (count <= 1)
                {
                    rootElement.KeyDown -= OnKeyDown;
                    rootElement.KeyUp -= OnKeyUp;
                    SubscriptionCounts.Remove(rootElement);
                }
                else
                {
                    SubscriptionCounts[rootElement] = count - 1;
                }
            }
        }

        ScopedElement = null;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        KeyPressedEventArgs eventArgs = e.ToKeyPressedEventArgs();
        RaiseKeyDown(eventArgs);
        if (eventArgs.Handled)
        {
            e.Handled = true;
        }
    }

    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        KeyPressedEventArgs eventArgs = e.ToKeyPressedEventArgs();
        RaiseKeyUp(eventArgs);
        if (eventArgs.Handled)
        {
            e.Handled = true;
        }
    }
}
