using Plugin.Maui.KeyListener;
using NUnit.Framework;
using System.Collections.Generic;

namespace Plugin.Maui.KeyListener.Tests;

public class KeyboardKeysExtensionsTests
{
    [Test]
    public void KeyboardKeysValues_Contains_All_Enum_Values()
    {
        var allEnumValues = Enum.GetValues<KeyboardKeys>();
        Assert.That(KeyboardKeysExtensions.KeyboardKeysValues, Is.EqualTo(allEnumValues));
    }

    [Test]
    public void KeyboardKeysValues_Has_No_Duplicates()
    {
        var values = KeyboardKeysExtensions.KeyboardKeysValues;
        var set = new HashSet<KeyboardKeys>(values);
        Assert.That(set.Count, Is.EqualTo(values.Length));
    }
}