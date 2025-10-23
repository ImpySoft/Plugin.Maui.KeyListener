namespace Plugin.Maui.KeyListener;

using UIKit;

public partial class GlobalHotKeysBehavior : PlatformBehavior<VisualElement>
{
	protected override void OnAttachedTo(VisualElement bindable, UIView platformView)
	{
		KeyboardEventHook.RegisterGlobalKeyboardBehavior(this, platformView);
		base.OnAttachedTo(bindable, platformView);
	}

	protected override void OnDetachedFrom(VisualElement bindable, UIView platformView)
	{
		KeyboardEventHook.UnregisterGlobalKeyboardBehavior(this);
		base.OnDetachedFrom(bindable, platformView);
	}
}