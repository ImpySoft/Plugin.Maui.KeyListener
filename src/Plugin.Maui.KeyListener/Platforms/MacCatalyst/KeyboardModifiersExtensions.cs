using CoreGraphics;

namespace Plugin.Maui.KeyListener;

using UIKit;

static class KeyboardModifiersExtensions
{
	static readonly KeyboardModifiers[] VirtualModifiersValues = Enum.GetValues<KeyboardModifiers>();

	static readonly UIKeyModifierFlags[] UIKeyModifierFlagsValues = Enum.GetValues<UIKeyModifierFlags>();

	private static readonly CGEventFlags[] CGEventFlagsValues = Enum.GetValues<CGEventFlags>();

	/// <remarks>
	///     https://developer.apple.com/documentation/uikit/uikeymodifierflags
	/// </remarks>
	internal static UIKeyModifierFlags ToPlatformModifiers(this KeyboardModifiers virtualModifiers)
	{
		UIKeyModifierFlags platformModifiers = 0;

		foreach (var virtualModifier in VirtualModifiersValues)
		{
			if (virtualModifiers.HasFlag(virtualModifier))
			{
				var platformModifier = ToPlatformModifier(virtualModifier);
				platformModifiers |= platformModifier;
			}
		}

		return platformModifiers;
	}

	internal static KeyboardModifiers ToVirtualModifiers(this UIKeyModifierFlags platformModifiers)
	{
		KeyboardModifiers virtualModifiers = 0;

		foreach (var platformModifier in UIKeyModifierFlagsValues)
		{
			if (platformModifiers.HasFlag(platformModifier))
			{
				var virtualModifier = ToVirtualModifier(platformModifier);
				virtualModifiers |= virtualModifier;
			}
		}

		return virtualModifiers;
	}

	internal static KeyboardModifiers ToVirtualModifiers(this CGEventFlags platformModifiers)
	{
		KeyboardModifiers virtualModifiers = 0;

		foreach (CGEventFlags platformModifier in CGEventFlagsValues)
		{
			if (platformModifiers.HasFlag(platformModifier))
			{
				KeyboardModifiers virtualModifier = ToVirtualModifier(platformModifier);
				virtualModifiers |= virtualModifier;
			}
		}

		return virtualModifiers;
	}

	static UIKeyModifierFlags ToPlatformModifier(KeyboardModifiers virtualModifier)
	{
		return virtualModifier switch
		{
			KeyboardModifiers.Alt => UIKeyModifierFlags.Alternate,
			KeyboardModifiers.Command => UIKeyModifierFlags.Command,
			KeyboardModifiers.Control => UIKeyModifierFlags.Control,
			KeyboardModifiers.Shift => UIKeyModifierFlags.Shift,
			_ => 0
		};
	}

	static KeyboardModifiers ToVirtualModifier(UIKeyModifierFlags platformModifier)
	{
		return platformModifier switch
		{
			UIKeyModifierFlags.AlphaShift => KeyboardModifiers.Shift,
			UIKeyModifierFlags.Alternate => KeyboardModifiers.Alt,
			UIKeyModifierFlags.Command => KeyboardModifiers.Command,
			UIKeyModifierFlags.Control => KeyboardModifiers.Control,
			UIKeyModifierFlags.Shift => KeyboardModifiers.Shift,
			_ => 0
		};
	}

	private static KeyboardModifiers ToVirtualModifier(CGEventFlags platformModifier)
	{
		return platformModifier switch
		{
			CGEventFlags.NonCoalesced => 0,
			CGEventFlags.AlphaShift => KeyboardModifiers.CapsLock,
			CGEventFlags.Shift => KeyboardModifiers.Shift,
			CGEventFlags.Control => KeyboardModifiers.Control,
			CGEventFlags.Alternate => KeyboardModifiers.Alt,
			CGEventFlags.Command => KeyboardModifiers.Command,
			CGEventFlags.NumericPad => 0, // can be used to determine if we pressed a numpad key
			CGEventFlags.Help => 0,
			CGEventFlags.SecondaryFn => 0, // usually the Fn keys and arrow keys
			_ => 0
		};
	}
}