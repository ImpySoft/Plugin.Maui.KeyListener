﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Maui.KeyListener
{
	public partial class KeyboardBehavior : PlatformBehavior<VisualElement>
	{
		KeyboardBehaviorTriggers _triggers;

		public VisualElement? ScopedElement { get; private set; }

		public KeyboardBehaviorTriggers Triggers => _triggers ??= new KeyboardBehaviorTriggers();

		public event EventHandler<KeyPressedEventArgs>? KeyDown;
		public event EventHandler<KeyPressedEventArgs>? KeyUp;

		internal void RaiseKeyDown(KeyPressedEventArgs args) => KeyDown?.Invoke(this, args);
		internal void RaiseKeyUp(KeyPressedEventArgs args) => KeyUp?.Invoke(this, args);
	}
}
