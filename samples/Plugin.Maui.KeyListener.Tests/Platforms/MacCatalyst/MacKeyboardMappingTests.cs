#if MACCATALYST
using NUnit.Framework;
using Plugin.Maui.KeyListener;
using UIKit;
using System;
using System.Linq;

namespace Plugin.Maui.KeyListener.Tests.Platforms.MacCatalyst;

[TestFixture]
[Platform(Include = "MacOsX")]
public class MacKeyboardMappingTests
{
    [Test]
    public void All_KeyboardKeys_Are_Mapped_To_UIKeyboardHidUsage()
    {
        var allKeys = Enum.GetValues<KeyboardKeys>();
        var unmapped = allKeys
            .Cast<KeyboardKeys>()
            .Where(k => k != KeyboardKeys.None)
            .Where(k => k.ToUIKeyboardHidUsage() == 0)
            .ToList();

        Assert.That(unmapped, Is.Empty,
            () => $"Unmapped KeyboardKeys: {string.Join(", ", unmapped)}");
    }

    [Test]
    public void All_UIKeyboardHidUsage_Are_Mapped_To_KeyboardKeys()
    {
        var allUsages = Enum.GetValues<UIKeyboardHidUsage>();
        var unmapped = allUsages
            .Cast<UIKeyboardHidUsage>()
            .Where(u => u != 0)
            .Where(u => u.ToKeyboardKeys() == KeyboardKeys.None)
            .ToList();

        Assert.That(unmapped, Is.Empty,
            () => $"Unmapped UIKeyboardHidUsage values: {string.Join(", ", unmapped)}");
    }
}
#endif