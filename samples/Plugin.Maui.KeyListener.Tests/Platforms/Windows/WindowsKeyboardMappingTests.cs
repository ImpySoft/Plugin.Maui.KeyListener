#if WINDOWS
using NUnit.Framework;
using Windows.System;
using Plugin.Maui.KeyListener;
using System;
using System.Linq;

namespace Plugin.Maui.KeyListener.Tests.Platforms.Windows;

[TestFixture]
[Platform(Include = "Win")]
public class WindowsKeyboardMappingTests
{
    [Test]
    public void All_VirtualKey_Values_Are_Mapped_To_KeyboardKeys()
    {
        var virtualKeys = Enum.GetValues<VirtualKey>();
        var unmapped = virtualKeys
            .Where(vk => vk != VirtualKey.None)
            .Where(vk => vk.ToKeyboardKeys() == KeyboardKeys.None)
            .ToList();

        Assert.That(unmapped, Is.Empty,
            () => $"Unmapped VirtualKey values: {string.Join(", ", unmapped)}");
    }
}
#endif