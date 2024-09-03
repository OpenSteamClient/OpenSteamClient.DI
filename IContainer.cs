using System.Diagnostics.CodeAnalysis;

namespace OpenSteamClient.DI;

/// <summary>
/// The interface which all container implementations must implement.
/// The API is quite simple, and all of the more complex stuff is defined in IContainerExtensions.
/// </summary>
public interface IContainer {
	/// <summary>
	/// Register a factory method, so that when it attempts to get retrieved with TryGet, it will run the factory method and produce it's output.
	/// For implementers: The output of the factory method should be registered, and the factory removed.
	/// </summary>
	/// <param name="type">The type we're registering a factory for</param>
	/// <param name="factoryMethod">The factory method. Can have arguments, which will be dependency injected.</param>
    public void RegisterFactoryMethod(Type type, Delegate factoryMethod);

	/// <summary>
	/// Register an object into the container.
	/// </summary>
	/// <param name="instance">The object to register.</param>
	/// <returns>The registered object.</returns>
    public object RegisterInstance(object instance);

	/// <summary>
	/// Try to retrieve a given object from the container.
	/// </summary>
	/// <param name="type">The type of the object to retrieve.</param>
	/// <param name="obj">The returned object, or null if no object is registered</param>
	/// <returns>False if no object is registered, true if an object was found</returns>
    public bool TryGet(Type type, [NotNullWhen(true)] out object? obj);
}