using System.Runtime.CompilerServices;

namespace Plugin.Maui.KeyListener;

using Microsoft.Maui.Handlers;
using UIKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;

public static partial class MauiProgramExtensions
{
	static partial void SetupPlatformKeyListener(MauiAppBuilder builder)
	{
		// method swizzling is static in nature
		RuntimeHelpers.RunClassConstructor(typeof(KeyboardEventHook).TypeHandle);

		builder.ConfigureMauiHandlers(handlers =>
		{
			PageHandler.PlatformViewFactory = handler =>
			{
				if (handler is not PageHandler pageHandler)
				{
					return null;
				}

				KeyboardPageViewController vc = new(handler.VirtualView, handler.MauiContext);
				handler.ViewController = vc;
				return (Microsoft.Maui.Platform.ContentView)vc.View.Subviews[0];
			};
		});

		return;
	}
}