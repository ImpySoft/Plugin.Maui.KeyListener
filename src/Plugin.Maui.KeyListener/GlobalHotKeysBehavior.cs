using System.Diagnostics.CodeAnalysis;

namespace Plugin.Maui.KeyListener;

/// <summary>
///     Provides a platform-specific behavior for handling global hotkeys in .NET MAUI Windows applications.
///     Unlike <see cref="KeyboardBehavior" />, which typically handles keyboard events scoped to a specific control or
///     view,
///     <c>GlobalHotKeysBehavior</c> attaches to the root element of each window and listens for key events at the
///     application level.
///     This enables detection and handling of keyboard shortcuts regardless of which control currently has focus,
///     making it suitable for implementing global hotkey functionality.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "UI Behavior")]
public partial class GlobalHotKeysBehavior : PlatformBehavior<VisualElement>
{
	KeyboardBehaviorTriggers? triggers;

	/// <summary>
	///     The element that received the keypress event.  Needed for determining scope
	/// </summary>
	public VisualElement? ScopedElement { get; private set; }

	/// <summary>
	///     Behaviors have triggers.  In this implementation they are not used, but required.  Using the trigger in the
	///     declaration will
	///     currently throw a not implemented exception.
	/// </summary>
	public KeyboardBehaviorTriggers Triggers => triggers ??= [];

	/// <summary>
	///     Event raised when the key is pressed down.
	///     Setting <see cref="KeyPressedEventArgs.Handled" /> to true will prevent the event from propagating further up the
	///     visual tree.
	/// </summary>
	public event EventHandler<KeyPressedEventArgs>? KeyDown;

	/// <summary>
	///     Event raised when the key is released.
	///     Setting <see cref="KeyPressedEventArgs.Handled" /> to true will prevent the event from propagating further up the
	///     visual tree.
	/// </summary>
	public event EventHandler<KeyPressedEventArgs>? KeyUp;

	public void RaiseKeyDown(KeyPressedEventArgs args)
	{
		KeyDown?.Invoke(this, args);
	}

	public void RaiseKeyUp(KeyPressedEventArgs args)
	{
		KeyUp?.Invoke(this, args);
	}
}