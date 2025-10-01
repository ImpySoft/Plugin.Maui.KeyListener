//namespace Plugin.Maui.KeyListener;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Foundation;
//using GameController;
//using UIKit;


//public partial class UglyNotUsedGameControllerKeyboardBehavior : PlatformBehavior<VisualElement>
//{
//	IDisposable? _subscription;

//	protected override void OnAttachedTo(VisualElement bindable, UIView platformView)
//	{
//		base.OnAttachedTo(bindable, platformView);

//		// Ensure the singleton GCKeyboard hook is running
//		GcGlobalKeyboard.EnsureStarted();

//		// Subscribe to app-wide key events
//		_subscription = GcGlobalKeyboard.Subscribe((code, pressed, mods) =>
//		{
//			var args = CreateArgs(code, pressed, mods);
//			if (pressed) RaiseDown(args);
//			else RaiseUp(args);
//		});
//	}

//	protected override void OnDetachedFrom(VisualElement bindable, UIView platformView)
//	{
//		_subscription?.Dispose();
//		_subscription = null;

//		base.OnDetachedFrom(bindable, platformView);
//	}

//	// === Map GCKeyboard to your plugin's KeyPressedEventArgs ===
//	// Adjust this method if your KeyPressedEventArgs has different ctor/properties.
//	static KeyPressedEventArgs CreateArgs(GCKeyCode code, bool pressed, KeyModifiers mods)
//	{
//		// If your plugin already has a Mac factory / mapper, call that instead.
//		// Example shown assumes a ctor like:
//		//   KeyPressedEventArgs(int keyCode, bool shift, bool alt, bool ctrl, bool cmd, bool pressed)
//		// Replace with your actual signature.
//		return new KeyPressedEventArgs(
//			(int)code,
//			mods.Shift, mods.Alt, mods.Control, mods.Command,
//			pressed
//		);
//	}
//}

//// Small DTO mirroring modifier state
//internal readonly record struct KeyModifiers(bool Shift, bool Control, bool Alt, bool Command, bool CapsLock);

///// <summary>
///// Process-level GCKeyboard hook with connect/disconnect handling.
///// One instance feeds any number of GlobalKeyboardBehavior subscribers.
///// </summary>
//internal static class GcGlobalKeyboard
//{
//	static readonly object _gate = new();
//	static bool _started;
//	static NSObject? _connectObs, _disconnectObs;
//	static GCKeyboardInput? _input;

//	public static void EnsureStarted()
//	{
//		lock (_gate)
//		{
//			if (_started) return;
//			_started = true;

//			Attach(GCKeyboard.Coalesced?.KeyboardInput);

//			_connectObs = NSNotificationCenter.DefaultCenter.AddObserver(
//				(NSString)"GCKeyboardDidConnect",
//				_ => Attach(GCKeyboard.Coalesced?.KeyboardInput));

//			_disconnectObs = NSNotificationCenter.DefaultCenter.AddObserver(
//				(NSString)"GCKeyboardDidDisconnect",
//				_ => Detach());
//		}
//	}

//	public static IDisposable Subscribe(Action<GCKeyCode, bool, KeyModifiers> onKey)
//	{
//		lock (_gate)
//		{
//			_onKey += onKey;
//			return new Unsub(() =>
//			{
//				lock (_gate) { _onKey -= onKey; }
//			});
//		}
//	}

//	static event Action<GCKeyCode, bool, KeyModifiers>? _onKey;

//	static void Attach(GCKeyboardInput? input)
//	{
//		if (ReferenceEquals(_input, input))
//			return;

//		Detach();
//		_input = input;
//		if (_input == null) return;

//		_input.KeyChangedHandler = (kb, keyCode, pressed) =>
//		{
//			var mods = new KeyModifiers(
//				Shift: kb.Shift, Control: kb.Control, Alt: kb.Alt,
//				Command: kb.Command, CapsLock: kb.CapsLock);

//			_onKey?.Invoke(keyCode, pressed, mods);
//		};
//	}

//	static void Detach()
//	{
//		if (_input != null)
//		{
//			_input.KeyChangedHandler = null;
//			_input = null;
//		}
//	}

//	sealed class Unsub : IDisposable
//	{
//		Action? _dispose;
//		public Unsub(Action dispose) => _dispose = dispose;
//		public void Dispose() { _dispose?.Invoke(); _dispose = null; }
//	}
//}


