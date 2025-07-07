using Plugin.Maui.KeyListener;
using NUnit.Framework;
using System.Collections.Generic;

namespace Plugin.Maui.KeyListener.Tests;

public class KeyboardKeysEnumTests
{
    [Test]
    public void All_KeyboardKeys_Enum_Values_Are_Unique()
    {
        var values = Enum.GetValues<KeyboardKeys>();
        var set = new HashSet<KeyboardKeys>(values);
        Assert.That(set.Count, Is.EqualTo(values.Length));
    }

    [Test]
    public void All_KeyboardKeys_Enum_Values_Have_Valid_Names()
    {
        var values = Enum.GetValues<KeyboardKeys>();
        foreach (var value in values)
        {
            string name = Enum.GetName(typeof(KeyboardKeys), value)!;
            Assert.That(string.IsNullOrWhiteSpace(name), Is.False);
        }
    }


}