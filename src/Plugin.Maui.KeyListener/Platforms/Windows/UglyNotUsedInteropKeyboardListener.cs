namespace Plugin.Maui.KeyListener;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Application = Application;
using Window = Window;

public partial class UglyNotUsedInteropKeyboardListener : PlatformBehavior<VisualElement>
{
	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll")]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string? lpModuleName);

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	private static IntPtr _hookId = IntPtr.Zero;
	private static LowLevelKeyboardProc? _proc;
	private static int SubscriptionCount;

	protected override void OnAttachedTo(VisualElement bindable, FrameworkElement platformView)
	{
		base.OnAttachedTo(bindable, platformView);

		ScopedElement = bindable;

		if (SubscriptionCount == 0)
		{
			_proc = HookCallback;
			_hookId = SetHook(_proc);
		}
		SubscriptionCount++;
	}

	protected override void OnDetachedFrom(VisualElement bindable, FrameworkElement platformView)
	{
		base.OnDetachedFrom(bindable, platformView);

		SubscriptionCount--;
		if (SubscriptionCount == 0 && _hookId != IntPtr.Zero)
		{
			UnhookWindowsHookEx(_hookId);
			_hookId = IntPtr.Zero;
			_proc = null;
		}

		ScopedElement = null;
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule curModule = curProcess.MainModule!;
		return SetWindowsHookEx(WH_KEYBOARD_LL,
								proc,
								GetModuleHandle(curModule.ModuleName),
								0);
	}

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			int msg = wParam.ToInt32();
			KBDLLHOOKSTRUCT kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

			// Only handle if this behavior is attached to the active window
			Window? activeWindow = GetActiveMauiWindow();
			if (!IsBehaviorAttachedToWindow(activeWindow))
			{
				return CallNextHookEx(_hookId, nCode, wParam, lParam);
			}

			if (msg == 0x100 || msg == 0x104) // KeyDown
			{
				KeyboardKeys key = MapVirtualKeyToKeyboardKey(unchecked((int)kb.vkCode));
				KeyboardModifiers modifiers = GetModifiers();

				KeyPressedEventArgs eventArgs = new()
				{
					Key = key,
					Modifiers = modifiers,
					KeyName = key.ToString()
				};

				RaiseKeyDown(eventArgs);
				if (eventArgs.Handled)
				{
					return 1;
				}
			}
			else if (msg == 0x101 || msg == 0x105) // KeyUp
			{
				KeyboardKeys key = MapVirtualKeyToKeyboardKey(unchecked((int)kb.vkCode));
				KeyboardModifiers modifiers = GetModifiers();

				KeyPressedEventArgs eventArgs = new()
				{
					Key = key,
					Modifiers = modifiers,
					KeyName = key.ToString()
				};

				RaiseKeyUp(eventArgs);
				if (eventArgs.Handled)
				{
					return 1;
				}
			}
		}
		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}

	// Checks if this behavior is attached to the active window
	private bool IsBehaviorAttachedToWindow(Window? activeWindow)
	{
		if (activeWindow == null)
		{
			return false;
		}

		// If ScopedElement is a VisualElement, get its Window property
		if (ScopedElement is VisualElement ve && ve.Window is Window attachedWindow)
		{
			return ReferenceEquals(attachedWindow, activeWindow);
		}
		return false;
	}

	private KeyboardKeys MapVirtualKeyToKeyboardKey(int vkCode)
	{
		if (vkCode >= 0x41 && vkCode <= 0x5A)
		{
			return (KeyboardKeys)(vkCode - 0x41 + (int)KeyboardKeys.A);
		}
		if (vkCode >= 0x30 && vkCode <= 0x39)
		{
			return (KeyboardKeys)(vkCode - 0x30 + (int)KeyboardKeys.Number0);
		}
		if (vkCode >= 0x60 && vkCode <= 0x69)
		{
			return (KeyboardKeys)(vkCode - 0x60 + (int)KeyboardKeys.NumPad0);
		}
		switch (vkCode)
		{
			case 0x70: return KeyboardKeys.F1;
			case 0x71: return KeyboardKeys.F2;
			case 0x72: return KeyboardKeys.F3;
			case 0x73: return KeyboardKeys.F4;
			case 0x74: return KeyboardKeys.F5;
			case 0x75: return KeyboardKeys.F6;
			case 0x76: return KeyboardKeys.F7;
			case 0x77: return KeyboardKeys.F8;
			case 0x78: return KeyboardKeys.F9;
			case 0x79: return KeyboardKeys.F10;
			case 0x7A: return KeyboardKeys.F11;
			case 0x7B: return KeyboardKeys.F12;
			case 0x1B: return KeyboardKeys.Escape;
			case 0x2C: return KeyboardKeys.PrintScreen;
			case 0x91: return KeyboardKeys.ScrollLock;
			case 0x13: return KeyboardKeys.Pause;
			case 0xC0: return KeyboardKeys.Backquote;
			case 0xBD: return KeyboardKeys.Minus;
			case 0xBB: return KeyboardKeys.Equals;
			case 0x08: return KeyboardKeys.Backspace;
			case 0x09: return KeyboardKeys.Tab;
			case 0xDB: return KeyboardKeys.LeftBracket;
			case 0xDD: return KeyboardKeys.RightBracket;
			case 0xDC: return KeyboardKeys.Backslash;
			case 0x14: return KeyboardKeys.CapsLock;
			case 0xBA: return KeyboardKeys.Semicolon;
			case 0xDE: return KeyboardKeys.Quote;
			case 0x0D: return KeyboardKeys.Return;
			case 0xA0: return KeyboardKeys.LeftShift;
			case 0xA1: return KeyboardKeys.RightShift;
			case 0xA2: return KeyboardKeys.LeftControl;
			case 0xA3: return KeyboardKeys.RightControl;
			case 0xA4: return KeyboardKeys.LeftAlt;
			case 0xA5: return KeyboardKeys.RightAlt;
			case 0x5B: return KeyboardKeys.LeftCommand;
			case 0x5C: return KeyboardKeys.RightCommand;
			case 0x20: return KeyboardKeys.Space;
			case 0x2D: return KeyboardKeys.Insert;
			case 0x2E: return KeyboardKeys.Delete;
			case 0x24: return KeyboardKeys.Home;
			case 0x23: return KeyboardKeys.End;
			case 0x21: return KeyboardKeys.PageUp;
			case 0x22: return KeyboardKeys.PageDown;
			case 0x25: return KeyboardKeys.LeftArrow;
			case 0x27: return KeyboardKeys.RightArrow;
			case 0x26: return KeyboardKeys.UpArrow;
			case 0x28: return KeyboardKeys.DownArrow;
			case 0x90: return KeyboardKeys.NumLock;
			case 0x6F: return KeyboardKeys.NumPadDivide;
			case 0x6A: return KeyboardKeys.NumPadMultiply;
			case 0x6D: return KeyboardKeys.NumPadMinus;
			case 0x6B: return KeyboardKeys.NumPadPlus;
			case 0x6C: return KeyboardKeys.NumPadEnter;
			case 0x6E: return KeyboardKeys.NumPadPeriod;
			case 0x60: return KeyboardKeys.NumPad0;
			case 0x61: return KeyboardKeys.NumPad1;
			case 0x62: return KeyboardKeys.NumPad2;
			case 0x63: return KeyboardKeys.NumPad3;
			case 0x64: return KeyboardKeys.NumPad4;
			case 0x65: return KeyboardKeys.NumPad5;
			case 0x66: return KeyboardKeys.NumPad6;
			case 0x67: return KeyboardKeys.NumPad7;
			case 0x68: return KeyboardKeys.NumPad8;
			default: return KeyboardKeys.None;
		}
	}

	private Window? GetActiveMauiWindow()
	{
		IntPtr foregroundHwnd = GetForegroundWindow();

		foreach (Window mauiWindow in Application.Current.Windows)
		{
			Microsoft.UI.Xaml.Window? nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
			if (nativeWindow != null)
			{
				IntPtr windowHwnd = WindowNative.GetWindowHandle(nativeWindow);
				if (windowHwnd == foregroundHwnd)
				{
					return mauiWindow;
				}
			}
		}
		return null;
	}

	private KeyboardModifiers GetModifiers()
	{
		KeyboardModifiers modifiers = KeyboardModifiers.None;

		if ((GetAsyncKeyState(0x10) & 0x8000) != 0)
		{
			modifiers |= KeyboardModifiers.Shift;
		}
		if ((GetAsyncKeyState(0x11) & 0x8000) != 0)
		{
			modifiers |= KeyboardModifiers.Control;
		}
		if ((GetAsyncKeyState(0x12) & 0x8000) != 0)
		{
			modifiers |= KeyboardModifiers.Alt;
		}
		if ((GetAsyncKeyState(0x5B) & 0x8000) != 0 ||
			(GetAsyncKeyState(0x5C) & 0x8000) != 0)
		{
			modifiers |= KeyboardModifiers.Command;
		}

		return modifiers;
	}

	private const int WH_KEYBOARD_LL = 13;

	[StructLayout(LayoutKind.Sequential)]
	private struct KBDLLHOOKSTRUCT
	{
		public uint vkCode;
		public uint scanCode;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}
}

