namespace Plugin.Maui.KeyListener;


#nullable enable
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

public static class ObjCInterop
{
	/// <summary>
	/// Represents a bidirectional association between two NSObject instances
	/// </summary>
	private sealed class ObjectAssociation : IDisposable
	{
		private readonly NSObject _objA;
		private readonly NSObject _objB;
		private readonly nint _key;
		private bool _disposed;

		public ObjectAssociation(NSObject objA, nint key, NSObject objB)
		{
			_objA = objA;
			_key = key;
			_objB = objB;
			objc_setAssociatedObject(_objA.Handle, _key, _objB.Handle, 0);
			objc_setAssociatedObject(_objB.Handle, _key, _objA.Handle, 0);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			// set to null to clean up association
			objc_setAssociatedObject(_objA.Handle, _key, 0, 0);
			objc_setAssociatedObject(_objB.Handle, _key, 0, 0);

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ObjectAssociation()
		{
			Dispose(false);
		}
	}

	private const string LibObjC = "/usr/lib/libobjc.dylib";

	[DllImport(LibObjC)]
	private static extern nint class_getInstanceMethod(nint cls, nint sel);

	[DllImport(LibObjC)]
	private static extern nint method_setImplementation(nint method, nint imp);

	[DllImport(LibObjC)]
	private static extern void objc_setAssociatedObject(nint obj, nint key, nint value, int policy);

	[DllImport(LibObjC)]
	private static extern IntPtr objc_getAssociatedObject(nint obj, nint key);

	/// <summary>
	/// Key for associating the underlying NSWindow of a UINSWindow
	/// Use with <see cref="AssociateObject"/> and <see cref="GetAssociatedObject"/>
	/// </summary>
	public static readonly nint UINSWindowKey = new NSString("underlyingnswindow").Handle;

	/// <summary>
	/// Overwrites an objective-c method, behaves similar to vtable patching
	/// </summary>
	/// <param name="classHandle"> Class handle of the method owner type </param>
	/// <param name="selectorName"> Selector to override </param>
	/// <param name="pOverrideMethod"> Function pointer to our implementation </param>
	/// <returns>
	/// A function pointer to the original method.
	/// Hold onto this if you need to call the base method
	/// </returns>
	public static nint OverwriteMethod(nint classHandle, string selectorName, nint pOverrideMethod)
	{
		// get the default implementation of the method we want to overwrite
		nint selectorHandle = Selector.GetHandle(selectorName);

		// note this returns the handle to the implementation of the method, not an actual function pointer
		nint baseObjcMethod = class_getInstanceMethod(classHandle, selectorHandle);

		// this overrides the method and returns a function pointer to the base method
		return method_setImplementation(baseObjcMethod, pOverrideMethod);
	}

	/// <summary>
	/// Creates a bidirectional association between two NSObject instances.
	/// Dispose this when the association is no longer needed
	/// </summary>
	/// <param name="objA"></param>
	/// <param name="key"> Can be anything, but best to be a static NSString handle </param>
	/// <param name="objB"></param>
	/// <returns></returns>
	public static IDisposable AssociateObject(NSObject objA, nint key, NSObject objB)
	{
		return new ObjectAssociation(objA, key, objB);
	}

	/// <summary>
	/// Gets the value associated with the given key from the specified NSObject
	/// </summary>
	/// <param name="aOrB"> Can be objA or objB from <see cref="AssociateObject"/> </param>
	/// <param name="key"> Same key used for <see cref="AssociateObject"/> </param>
	/// <returns></returns>
	public static NSObject? GetAssociatedObject(NSObject? aOrB, nint key)
	{
		if (aOrB is null)
		{
			return null;
		}
		nint handle = objc_getAssociatedObject(aOrB.Handle, key);
		return handle == 0 ? null : Runtime.GetNSObject(handle);
	}
}