using Plugin.Maui.KeyListener;
using NUnit.Framework;

namespace Plugin.Maui.KeyListener.Tests;

public class KeyPressedEventArgsTests
{
    [Test]
    public void Can_Set_And_Get_All_Properties()
    {
        var args = new KeyPressedEventArgs
        {
            Keys = KeyboardKeys.F1,
            Modifiers = KeyboardModifiers.Control | KeyboardModifiers.Shift,
            KeyName = "F1",
            Handled = true
        };

        Assert.That(args.Keys, Is.EqualTo(KeyboardKeys.F1));
		Assert.That(args.Modifiers.HasFlag(KeyboardModifiers.Control), Is.True);
		Assert.That(args.Modifiers.HasFlag(KeyboardModifiers.Shift), Is.True);
        Assert.That(args.KeyName, Is.EqualTo("F1"));
        Assert.That(args.Handled, Is.True);
    }

    [Test]
    public void Default_Properties_Are_Default()
    {
        var args = new KeyPressedEventArgs();
        Assert.That(args.Keys, Is.EqualTo(default(KeyboardKeys)));
        Assert.That(args.Modifiers, Is.EqualTo(default(KeyboardModifiers)));
        Assert.That(args.KeyName, Is.Null);
        Assert.That(args.Handled, Is.False);
    }
}