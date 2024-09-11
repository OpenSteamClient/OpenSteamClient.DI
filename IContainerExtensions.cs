using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OpenSteamClient.DI.Lifetime;

namespace OpenSteamClient.DI;

public static class IContainerExtensions {
	/// <summary>
	/// Gets an object from the container, throwing if not found.
	/// </summary>
	/// <param name="type">The type to find</param>
	/// <returns>The registered object</returns>
	/// <exception cref="KeyNotFoundException">If the type was not registered</exception>
	public static object Get(this IContainer container, Type type) {
		if (!container.TryGet(type, out object? obj)) {
			throw new KeyNotFoundException("Type '" + type + "' not registered.");
		}

		return obj;
	}

	private static ConstructorInfo GetConstructorFor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
    {
        ConstructorInfo[] ctors = type.GetConstructors();
        if (ctors.Length == 0)
        {
            throw new ArgumentException("No constructors for " + type.Name);
        }

        if (ctors.Length > 1)
        {
            throw new NotSupportedException("More than one constructor for " + type.Name);
        }

        return ctors.First();
    }

	/// <summary>
	/// Construct an object of the specified type, with DI.
	/// </summary>
	/// <param name="type">The type to construct an instance of.</param>
	/// <param name="extraArgs">Extra objects that the constructor needs.</param>
	/// <returns></returns>
	public static object Construct(this IContainer container, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, params object[] extraArgs)
	{
		List<object> ctorArgsBuilt = [];
		ConstructorInfo ctor = GetConstructorFor(type);
		
		Type[] wantedArgs = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
		Type[] extraArgsTypes = extraArgs.Select(t => t.GetType()).ToArray();
		foreach (var argType in wantedArgs)
		{
			ctorArgsBuilt.Add(Array.Find(extraArgs, p => p.GetType() == argType) ?? Get(container, argType));
		}

		return ctor.Invoke(ctorArgsBuilt.ToArray());
	}

	/// <summary>
	/// Construct an object of the specified type, with DI.
	/// </summary>
	/// <param name="type">The type to construct an instance of.</param>
	/// <returns></returns>
	public static object Construct(this IContainer container, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
	{
		ConstructorInfo ctor = GetConstructorFor(type);
		List<object> ctorArgsBuilt = new();
		Type[] wantedArgs = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
		foreach (var argType in wantedArgs)
		{
			ctorArgsBuilt.Add(Get(container, argType));
		}

		return ctor.Invoke(ctorArgsBuilt.ToArray());
	}

	/// <summary>
	/// Register an object into the container.
	/// </summary>
	/// <param name="instance">The object to register.</param>
	/// <returns>The registered object.</returns>
	public static T RegisterInstance<T>(this IContainer container, T instance) where T: class
		=> (T)container.RegisterInstance(typeof(T), instance);

	[Obsolete("Your code is trying to register System.Object!")]
	public static object RegisterInstance(this IContainer container, object instance)
		=> throw new UnreachableException("Do not call this! Your code has errors, and is trying to register System.Object!");

	/// <summary>
	/// Register the type to be lazily constructed.
	/// </summary>
	public static void RegisterLazy(this IContainer container, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
		=> container.RegisterFactoryMethod(type, () => Construct(container, type));
	
	/// <summary>
	/// Construct the type, and register it.
	/// </summary>
	public static object ConstructAndRegister(this IContainer container, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
		=> container.RegisterInstance(type, Construct(container, type));

	/// <summary>
	/// Register a factory method, so that when it attempts to get retrieved with TryGet, it will run the factory method and produce it's output.
	/// For implementers: The output of the factory method should be registered, and the factory removed.
	/// </summary>
	/// <param name="factoryMethod">The factory method. Can have arguments, which will be dependency injected.</param>
	public static void RegisterFactoryMethod<T>(this IContainer container, Delegate factoryMethod) where T: class
		=> container.RegisterFactoryMethod(typeof(T), factoryMethod);

	public static bool TryGet<T>(this IContainer container, [NotNullWhen(true)] out T? obj) where T: class
	{
		// Stinky C# syntax at it again
		bool ret = container.TryGet(typeof(T), out object? untyped);
		obj = (T?)untyped;
		return ret;
	}

	public static bool Contains(this IContainer container, Type type)
		=> container.TryGet(type, out _);

	public static bool Contains<T>(this IContainer container) where T: class
		=> Contains(container, typeof(T));
		
	public static T Get<T>(this IContainer container) where T: class
		=> (T)Get(container, typeof(T));

	/// <summary>
	/// Construct an object of the specified type, with DI.
	/// </summary>
	/// <param name="type">The type to construct an instance of.</param>
	/// <param name="extraArgs">Extra objects that the constructor needs.</param>
	/// <returns></returns>	
	public static T Construct<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IContainer container, params object[] extraArgs)
		=> (T)Construct(container, typeof(T), extraArgs);

	/// <summary>
	/// Construct the type, and register it.
	/// </summary>
	public static T ConstructAndRegister<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IContainer container) where T: class
		=> RegisterInstance<T>(container, Construct<T>(container));

	/// <summary>
	/// Register the type to be lazily constructed.
	/// </summary>
	public static void RegisterLazy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IContainer container) where T: class
		=> RegisterLazy(container, typeof(T));
}