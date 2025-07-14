using Plugin.Maui.KeyListener;
using NUnit.Framework;

namespace Plugin.Maui.KeyListener.Tests;

public class KeyboardKeysConversionTests
{
    [Test]
    public void Enum_To_String_And_Back_RoundTrips()
    {
        foreach (var key in Enum.GetValues<KeyboardKeys>())
        {
            string name = key.ToString();
            var parsed = (KeyboardKeys)Enum.Parse(typeof(KeyboardKeys), name);
            Assert.That(parsed, Is.EqualTo(key));
        }
    }
}