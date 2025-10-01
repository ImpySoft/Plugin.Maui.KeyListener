namespace Plugin.Maui.KeyListener;

using UIKit;

public partial class GlobalHotKeysBehavior : PlatformBehavior<VisualElement>
{
    //TODO: Implement MacCatalyst specific functionality for GlobalHotKeysBehavior
    protected override void OnAttachedTo(VisualElement bindable, UIView platformView)
    {
        base.OnAttachedTo(bindable, platformView);
    }

    //TODO: Implement MacCatalyst specific functionality for GlobalHotKeysBehavior
    protected override void OnDetachedFrom(VisualElement bindable, UIView platformView)
    {
        base.OnDetachedFrom(bindable, platformView);
    }
}
