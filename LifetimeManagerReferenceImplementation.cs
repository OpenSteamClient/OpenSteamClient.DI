using OpenSteamClient.DI.Attributes;
using OpenSteamClient.DI.Lifetime;

namespace OpenSteamClient.DI;

/// <summary>
/// The reference implementation of ILifetimeManager. It's preferred to use your own implementation if advanced functionality is needed.
/// </summary>
[DIRegisterInterface<ILifetimeManager>]
public sealed class LifetimeManagerReferenceImplementation : ILifetimeManager
{
	private readonly IContainer container;

	public LifetimeManagerReferenceImplementation(IContainer container)
	{
		this.container = container;
	}

	private class LifetimeObject
	{
		public object? Object { get; set; }
		public Type? ContainerType { get; set; }
		public Type RealType
		{
			get
			{
				if (Object != null)
				{
					return Object.GetType();
				}
				else if (ContainerType != null)
				{
					return ContainerType;
				}

				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}
		}

		private readonly IContainer container;
		public LifetimeObject(IContainer container)
		{
			this.container = container;
		}

		public override string ToString()
		{
			if (Object != null)
			{
				return Object.GetType().Name;
			}
			else if (ContainerType != null)
			{
				return ContainerType.Name;
			}

			throw new InvalidDataException("Neither Object nor ContainerType is specified");
		}

		public async Task RunClientStartup(IProgress<OperationProgress> progress)
		{
			IClientLifetime? obj = (IClientLifetime?)Object;
			if (obj == null && ContainerType != null)
			{
				obj = (IClientLifetime?)container.Get(ContainerType);
			}

			if (obj == null)
			{
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunStartup(progress);
		}

		public async Task RunClientShutdown(IProgress<OperationProgress> progress)
		{
			IClientLifetime? obj = (IClientLifetime?)Object;
			if (obj == null && ContainerType != null)
			{
				obj = (IClientLifetime?)container.Get(ContainerType);
			}

			if (obj == null)
			{
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunShutdown(progress);
		}

		public async Task RunLogon(IProgress<OperationProgress> progress)
		{
			ILogonLifetime? obj = (ILogonLifetime?)Object;
			if (obj == null && ContainerType != null)
			{
				obj = (ILogonLifetime?)container.Get(ContainerType);
			}

			if (obj == null)
			{
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunLogon(progress);
		}

		public async Task RunLogoff(IProgress<OperationProgress> progress)
		{
			ILogonLifetime? obj = (ILogonLifetime?)Object;
			if (obj == null && ContainerType != null)
			{
				obj = (ILogonLifetime?)container.Get(ContainerType);
			}

			if (obj == null)
			{
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunLogoff(progress);
		}
	}

	private readonly List<LifetimeObject> clientLifetimeOrder = [];
	private readonly List<LifetimeObject> logonLifetimeOrder = [];

	private bool hasRanStartup = false;

	/// <inheritdoc/>
	public void RegisterForClientLifetime(IClientLifetime obj) {
		clientLifetimeOrder.Add(new(container) { Object = obj });
	}

	/// <inheritdoc/>
	public void RegisterForLogonLifetime(ILogonLifetime obj) {
		logonLifetimeOrder.Add(new(container) { Object = obj });
	}

	/// <inheritdoc/>
	public void RegisterContainerType(Type type) {
		if (type.IsAssignableTo(typeof(IClientLifetime))) {
			clientLifetimeOrder.Add(new(container) { ContainerType = type });
		}
		
		if (type.IsAssignableTo(typeof(ILogonLifetime))) {
			logonLifetimeOrder.Add(new(container) { ContainerType = type });
		}
	}

	private readonly SemaphoreSlim clientLifetimeLock = new(1, 1);
	public async Task RunClientStartup(IProgress<OperationProgress> progress)
    {
		await clientLifetimeLock.WaitAsync();
		try
		{
			foreach (var component in clientLifetimeOrder)
			{
				await component.RunClientStartup(progress);
			}

			hasRanStartup = true;
		}
		finally
		{
			clientLifetimeLock.Release();
		}
    }

    public async Task RunClientShutdown(IProgress<OperationProgress> progress)
    {
		await clientLifetimeLock.WaitAsync();
		try
		{
			if (!hasRanStartup)
			{
				return;
			}

			hasRanStartup = false;
			foreach (var component in clientLifetimeOrder)
			{
				await component.RunClientShutdown(progress);
			}
		}
		finally
		{
			clientLifetimeLock.Release();
		}
    }

	private readonly SemaphoreSlim logonLock = new(1, 1);
    public async Task RunLogon(IProgress<OperationProgress> progress)
    {
		await logonLock.WaitAsync();
		try
		{
			foreach (var component in logonLifetimeOrder)
			{
				await component.RunLogon(progress);
			}
		}
		finally
		{
			logonLock.Release();
		}
    }

    public async Task RunLogoff(IProgress<OperationProgress> progress)
    {
		await logonLock.WaitAsync();
		try
		{
			foreach (var component in logonLifetimeOrder)
			{
				await component.RunLogoff(progress);
			}
		}
		finally
		{
			logonLock.Release();
		}
    }
}