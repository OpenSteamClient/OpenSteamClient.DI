using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OpenSteamClient.DI.Attributes;
using OpenSteamClient.DI.Lifetime;

namespace OpenSteamClient.DI;

/// <summary>
/// The reference implementation of IContainer. It's preferred to use your own implementation if advanced functionality is needed.
/// </summary>
public sealed class ContainerReferenceImplementation : IContainer
{
	/// <summary>
	/// A placeholder object to store in place for objects that are still factories
	/// </summary>
    private readonly object factoryPlaceholderObject = new object();

	/// <summary>
	/// Lock for everything this container does.
	/// </summary>
	private readonly object containerLock = new();

	/// <summary>
	/// All the objects inside this container
	/// </summary>
    private Dictionary<Type, object> registeredObjects { get; init; } = new();

	/// <summary>
	/// All the factories that have not been ran yet.
	/// </summary>
    private Dictionary<Type, Delegate> factories { get; init; } = new();

    public ContainerReferenceImplementation(IEnumerable<object> initialServices)
    {
		this.RegisterInstance(this);

		foreach (var item in initialServices)
		{
			this.RegisterInstance(item);
		}
    }

	/// <inheritdoc/>
    public void RegisterFactoryMethod(Type type, Delegate factoryMethod)
    {
        lock (containerLock)
		{
			if (this.registeredObjects.ContainsKey(type))
			{
				throw new InvalidOperationException("Type '" + type + "' already registered.");
			}
	
			if (this.factories.ContainsKey(type))
			{
				throw new InvalidOperationException("Factory for type '" + type + "' already registered.");
			}
	
			this.factories.Add(type, factoryMethod);
			this.registeredObjects.Add(type, factoryPlaceholderObject);
			var implementedInterfacesAttrs = type.GetCustomAttributes(typeof(DIRegisterInterfaceAttribute<>));
			foreach (var ifaceAttr in implementedInterfacesAttrs)
			{
				Type interfaceType = ifaceAttr.GetType().GetGenericArguments().First();
				this.factories.Add(interfaceType, factoryMethod);
				this.registeredObjects.Add(interfaceType, factoryPlaceholderObject);
			}

			if (this.TryGet(out ILifetimeManager? lifetimeManager)) {
				lifetimeManager.RegisterContainerType(type);
			}
		}
    }

    private object RunFactoryFor(Type type)
    {
        lock (containerLock)
		{
			if (!this.factories.ContainsKey(type))
			{
				throw new InvalidOperationException("Factory '" + type + "' not registered");
			}
	
			var factoryMethod = factories[type];
			object? ret = factoryMethod.DynamicInvoke(factoryMethod.GetMethodInfo().GetParameters().Select(p => this.Get(p.ParameterType)));
			if (ret == null)
			{
				throw new NullReferenceException("Factory for " + type + " returned null.");
			}
	
			List<Type> toRemove = new();
			foreach (var f in this.factories)
			{
				if (f.Value == factoryMethod) {
					toRemove.Add(f.Key);
				}
			}
	
			foreach (var item in toRemove)
			{
				this.factories.Remove(item);
	
				if (this.registeredObjects.ContainsKey(item))
				{
					if (object.ReferenceEquals(this.registeredObjects[item], factoryPlaceholderObject))
					{
						this.registeredObjects.Remove(item);
					}
					else
					{
						throw new InvalidOperationException("Type '" + item + "' already registered (and not factory placeholder)");
					}
				}
			}
	
			this.RegisterInstance(ret);
			return ret;
		}
    }

	/// <inheritdoc/>
    public object RegisterInstance(object instance)
    {
		lock (containerLock)
		{
			var type = instance.GetType();
	
	        if (this.registeredObjects.ContainsKey(type))
	        {
	            throw new InvalidOperationException("Type '" + type + "' already registered.");
	        }
	
	        if (instance == null)
	        {
	            throw new NullReferenceException("component is null");
	        }
	
	
	        this.registeredObjects.Add(type, instance);

			if (this.TryGet(out ILifetimeManager? lifetimeManager)) {
				lifetimeManager.RegisterContainerType(type);
			}

	        return instance;
		}
    }
	
	/// <inheritdoc/>
    public bool TryGet(Type type, [NotNullWhen(true)] out object? obj)
    {
		lock (containerLock)
		{
			if (!this.registeredObjects.TryGetValue(type, out obj))
			{
				return false;
			}
			
			// Run the factory if it's a factory
			if (object.ReferenceEquals(obj, factoryPlaceholderObject))
			{
				obj = RunFactoryFor(type);
			}
	
			return true;
		}
	}
}