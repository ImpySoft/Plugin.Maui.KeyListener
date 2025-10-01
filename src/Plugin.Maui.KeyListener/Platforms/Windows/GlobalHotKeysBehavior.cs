using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Application = Microsoft.Maui.Controls.Application;
using Window = Microsoft.Maui.Controls.Window;

namespace Plugin.Maui.KeyListener;

using Application = Application;
using Window = Window;

public partial class GlobalHotKeysBehavior : PlatformBehavior<VisualElement>
{
	// Tracks how many behaviors are attached per root element
	static readonly Dictionary<FrameworkElement, int> subscriptionCounts = new();

	protected override void OnAttachedTo(VisualElement bindable, FrameworkElement platformView)
	{
		base.OnAttachedTo(bindable, platformView);

		ScopedElement = bindable;

		foreach (var mauiWindow in Application.Current?.Windows ?? Enumerable.Empty<Window>())
		{
			var nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
			var rootElement = nativeWindow?.Content as FrameworkElement;
			if (rootElement != null)
			{
				if (!subscriptionCounts.TryGetValue(rootElement, out var count) || count == 0)
				{
					rootElement.KeyDown += OnKeyDown;
					rootElement.KeyUp += OnKeyUp;
				}

				subscriptionCounts[rootElement] = count + 1;
			}
		}
	}

	protected override void OnDetachedFrom(VisualElement bindable, FrameworkElement platformView)
	{
		base.OnDetachedFrom(bindable, platformView);

		foreach (var mauiWindow in Application.Current?.Windows ?? Enumerable.Empty<Window>())
		{
			var nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
			var rootElement = nativeWindow?.Content as FrameworkElement;
			if (rootElement != null && subscriptionCounts.TryGetValue(rootElement, out var count))
			{
				if (count <= 1)
				{
					rootElement.KeyDown -= OnKeyDown;
					rootElement.KeyUp -= OnKeyUp;
					subscriptionCounts.Remove(rootElement);
				}
				else
				{
					subscriptionCounts[rootElement] = count - 1;
				}
			}
		}

		ScopedElement = null;
	}

	void OnKeyDown(object sender, KeyRoutedEventArgs e)
	{
		var eventArgs = e.ToKeyPressedEventArgs();
		RaiseKeyDown(eventArgs);
		if (eventArgs.Handled)
		{
			e.Handled = true;
		}
	}

	void OnKeyUp(object sender, KeyRoutedEventArgs e)
	{
		var eventArgs = e.ToKeyPressedEventArgs();
		RaiseKeyUp(eventArgs);
		if (eventArgs.Handled)
		{
			e.Handled = true;
		}
	}
}