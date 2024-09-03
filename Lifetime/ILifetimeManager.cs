namespace OpenSteamClient.DI.Lifetime;

/// <summary>
/// Manages component lifetimes.
/// </summary>
public interface ILifetimeManager {
	/// <summary>
	/// Register this component for client lifetime (from client start to its shutdown)
	/// This function will not throw if attempting to register the same item twice.
	/// </summary>
	public void RegisterForClientLifetime(IClientLifetime obj);

	/// <summary>
	/// Register this component for logon lifetime (from user successful logon to logout)
	/// This function will not throw if attempting to register the same item twice.
	/// </summary>
	public void RegisterForLogonLifetime(ILogonLifetime obj);

	/// <summary>
	/// Register a component that has either a client lifetime, logon lifetime, or both and is available through an IContainer.
	/// This function will not throw if attempting to register the same item twice.
	/// </summary>
	public void RegisterContainerType(Type type);
}