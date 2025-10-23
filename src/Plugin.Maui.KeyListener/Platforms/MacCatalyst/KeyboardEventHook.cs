namespace Plugin.Maui.KeyListener;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

/// <summary>
/// Used handle keyboard input on mac
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Platform interop")]
internal static unsafe class KeyboardEventHook
{
	/// <summary>
	/// Key is the behavior, value is the UI element it is attached to
	/// </summary>
	private static Dictionary<GlobalHotKeysBehavior, UIView> GlobalKeyboardBehaviors { get; } = new();

	/// <summary>
	/// Maintains a collection of Maui Window to UIWindow/NSWindow associations
	/// </summary>
	private static Dictionary<Window, IDisposable> MauiWindowToUINSAssociation { get; } = new();

	/// Since we are overriding SendEvent, we need to keep a reference to the base implementation function pointer
	private static readonly nint BaseSendEvent;

	private static readonly nint BaseHostWindowForUIWindow;

	static KeyboardEventHook()
	{
		// override SendEvent
		// this allows us to reliably capture keyboard input
		BaseSendEvent = ObjCInterop.OverwriteMethod(Class.GetHandle("UINSWindow"),
			"sendEvent:",
			(nint)(delegate* unmanaged<nint, nint, nint, void>)&SendEvent);

		// in atn, this is in MainContainer
		// we use this to associate our UIWindow with its NSWindow
		BaseHostWindowForUIWindow = ObjCInterop.OverwriteMethod(Class.GetHandle("UINSApplicationDelegate"),
			"hostWindowForUIWindow:",
			(nint)(delegate* unmanaged<nint, nint, nint, nint>)&HostWindowForUiWindow);
	}

	/// <summary>
	/// Call when the behavior is attached to a control
	/// </summary>
	/// <param name="keyboardBehavior"></param>
	/// <param name="platformView"></param>
	internal static void RegisterGlobalKeyboardBehavior(GlobalHotKeysBehavior keyboardBehavior, UIView platformView)
	{
		GlobalKeyboardBehaviors[keyboardBehavior] = platformView;
	}

	/// <summary>
	/// Call when the behavior is detached from a control
	/// </summary>
	/// <param name="keyboardBehavior"></param>
	internal static void UnregisterGlobalKeyboardBehavior(GlobalHotKeysBehavior keyboardBehavior)
	{
		GlobalKeyboardBehaviors.Remove(keyboardBehavior);
	}

	/// <summary>
	/// This gets called by the runtime.
	/// We use it to know when a window's underlying NSWindow is ready to use
	/// </summary>
	/// <param name="self">The underlying UINSApplicationDelegate</param>
	/// <param name="cmd">hostWindowForUIWindow: selector</param>
	/// <param name="uiWindowHandle">Native handle to the UIWindow</param>
	/// <returns></returns>
	[UnmanagedCallersOnly]
	private static unsafe nint HostWindowForUiWindow(nint self, nint cmd, nint uiWindowHandle)
	{
		// self is the UINSApplicationDelegate
		// cmd is the hostWindowForUIWindow: selector
		// uiWindow is the handle to the UIWindow
		nint nsWindowHandle =
			((delegate* unmanaged<nint, nint, nint, nint>)BaseHostWindowForUIWindow)(self, cmd, uiWindowHandle);

		if (nsWindowHandle == nint.Zero)
		{
			return nsWindowHandle;
		}

		// we have a non-null NSWindow, grab that here
		NSObject? nsWindow = Runtime.GetNSObject(nsWindowHandle);

		// api changed in 11, the real window is here
		if (OperatingSystem.IsMacCatalystVersionAtLeast(11) && nsWindow is not null)
		{
			nsWindow = nsWindow.ValueForKey(new("attachedWindow"));
		}

		if (nsWindow is null)
		{
			return nsWindowHandle;
		}

		// if we're here with a valid NSWindow, we can safely assume that the UIWindow and the Maui Window are created and ready to use
		UIWindow? uiWindow = Runtime.GetNSObject<UIWindow>(uiWindowHandle);
		if (uiWindow is not null)
		{
			// find the Maui window this one belongs to
			Window? MauiWindow = Application.Current?.Windows
				.FirstOrDefault(x => ReferenceEquals(x.Handler?.PlatformView, uiWindow));

			// HostWindowForUIWindow is invoked by the runtime when the window is closing
			// but by that point, it is no longer part of Maui's window collection
			if (MauiWindow is not null)
			{
				// this is different then atn
				// the association happens in WorkspaceWindow
				// AssociateObject is needed as keyboard events reference the NSWindow and we need to get back to the UIWindow
				if (!MauiWindowToUINSAssociation.ContainsKey(MauiWindow))
				{
					MauiWindowToUINSAssociation[MauiWindow] =
						ObjCInterop.AssociateObject(uiWindow, ObjCInterop.UINSWindowKey, nsWindow);
					;
				}
			}
		}

		return nsWindowHandle;
	}

	/// <summary>
	/// Gets called by the runtime for keyboard and mouse events per window
	/// </summary>
	/// <param name="self"> NSWindow </param>
	/// <param name="cmd"> sendEvent: selector handle</param>
	/// <param name="eventHandle"> Native handle to the NSEvent </param>
	[UnmanagedCallersOnly]
	private static void SendEvent(nint self, nint cmd, nint eventHandle)
	{
		// call the base method
		((delegate* unmanaged<nint, nint, nint, void>)BaseSendEvent)(self, cmd, eventHandle);

		// get the NSEvent
		NSObject? nsEvent = Runtime.GetNSObject(eventHandle);

		if (nsEvent is null)
		{
			return;
		}

		// determine the event type
		// for now, we only need keyup and keydown
		CGEventType eventType = (CGEventType)((NSNumber)nsEvent.ValueForKey(new("type"))).UInt32Value;

		// pressing and releasing a modifier key only sends a FlagsChanged, not KeyUp/KeyDown
		if (eventType is CGEventType.KeyDown or CGEventType.KeyUp)
		{
			// keycodes are a bit complicated
			// on Windows, VirtualKey uses the ascii value of the key
			// A = 65
			// in UIKit, they use HID codes
			// The Game Controller apis also use HID codes
			// A = 4
			// GCKeyCode.KeyA = 4
			// In appkit, keycode corresponds to the physical location of the key
			// A = 0 (on a qwerty keyboard)

			// this would not work as keycode 0 is the 'A' on a qwerty layout but 'Q' on an azerty layout
			// ushort keycode = ((NSNumber)nsEvent.ValueForKey(new("keyCode"))).UInt16Value;

			// the various docs and AI suggest using this
			// it works for the most part, but you get a different string if shift is held
			// I.E pressing shift + 1 (not numpad 1) will send a '!'
			// this also applies to letters, capital vs lowercase
			// oddly enough, it does not apply to caps lock
			// when caps lock is on, pressing a key still sends the lowercase letter
			NSString keyString = ((NSString)nsEvent.ValueForKey(new("charactersIgnoringModifiers")));

			if (keyString.Length == 0)
			{
				return;
			}

			// grab the key we pressed
			char keyChar = keyString[0];

			// get the modifiers
			// need to mask out the lower 16 bits to get the keys themselves
			// the modifiers themselves can be used to tell us some useful info such as:
			// the difference between numpad and normal number keys, (NumericPad)
			// if it's an arrow key or function key (SecondaryFn)
			const ulong deviceIndependentFlagsMask = 0xffffffff_ffff0000;
			CGEventFlags modifiers = (CGEventFlags)(((NSNumber)nsEvent.ValueForKey(new("modifierFlags"))).UInt64Value &
													deviceIndependentFlagsMask);

			// holding down a key fires lots of keydown events
			// this lets us only handle single keystroke events
			bool isRepeat = ((NSNumber)nsEvent.ValueForKey(new("isARepeat"))).BoolValue;

			// grab the window that received the keystroke
			// this is needed so down the line, the correct behaviors receive the event
			NSObject senderWindow = nsEvent.ValueForKey(new("window"));

			if (!isRepeat)
			{
				// TODO: implement local keyboard behaviors

				// TODO: determine how "handled" should behave
				// on Windows, the event is sent upwards from its respective control
				// if we set e.handled to true, the event does not propagate further upwards
				// on Mac, the event always originates at the window level
				// so we have to manually send it downwards to the focused control

				// since there is only a single first responder (in theory)
				// we could fire local behaviors first
				// if one of them sets handled to true, we skip firing global behaviors

				// get global keyboard behaviors where their UIWindow's underlying NSWindow matches the sender window
				IEnumerable<GlobalHotKeysBehavior> globalBehaviorsToSend =
					GlobalKeyboardBehaviors
						.Where(kv =>
							ReferenceEquals(ObjCInterop.GetAssociatedObject(kv.Value.Window, ObjCInterop.UINSWindowKey),
								senderWindow))
						.Select(kv => kv.Key);

				// prepare the event args
				KeyPressedEventArgs args = new()
				{
					Modifiers = modifiers.ToVirtualModifiers(),
					KeyName = keyString.ToString(),
					Keys = keyChar.ToKeyboardKeys()
				};

				// finally, send the event to each global behavior
				foreach (GlobalHotKeysBehavior globalBehavior in globalBehaviorsToSend)
				{
					if (eventType == CGEventType.KeyDown)
					{
						globalBehavior.RaiseKeyDown(args);
					}
					else
					{
						globalBehavior.RaiseKeyUp(args);
					}
				}

				System.Diagnostics.Debug.WriteLine(
					$"Keyboard Event: Window: 0x{senderWindow.Handle.Handle:x8} KeyInt: {(int)keyChar} 0x{(int)keyChar:x4} {keyChar} {eventType}, Mods: {modifiers}, Repeat: {isRepeat}");
			}
		}
	}
}