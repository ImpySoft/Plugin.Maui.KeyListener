using System;
using Plugin.Maui.KeyListener;
using NUnit.Framework;

namespace Plugin.Maui.KeyListener.Tests;

public class KeyboardBehaviorTests
{
    [Test]
    public void Triggers_Property_Returns_Same_Instance()
    {
        var behavior = new KeyboardBehavior();
        var first = behavior.Triggers;
        var second = behavior.Triggers;
        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void RaiseKeyDown_Raises_Event_With_Correct_Args()
    {
        var behavior = new KeyboardBehavior();
        KeyPressedEventArgs? received = null;
        behavior.KeyDown += (s, e) => received = e;

        var args = new KeyPressedEventArgs { Keys = KeyboardKeys.A, Modifiers = KeyboardModifiers.Alt, KeyName = "A" };
        behavior.RaiseKeyDown(args);

		Assert.That(received, Is.Not.Null);
		Assert.That(received, Is.EqualTo(args));
    }

    [Test]
    public void RaiseKeyUp_Raises_Event_With_Correct_Args()
    {
        var behavior = new KeyboardBehavior();
        KeyPressedEventArgs? received = null;
        behavior.KeyUp += (s, e) => received = e;

        var args = new KeyPressedEventArgs { Keys = KeyboardKeys.B, Modifiers = KeyboardModifiers.Control, KeyName = "B" };
        behavior.RaiseKeyUp(args);

        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.EqualTo(args));
    }

    [Test]
    public void Additional_Condition_Test()
    {
        var condition = true;
        Assert.That(condition, Is.True);
    }
}