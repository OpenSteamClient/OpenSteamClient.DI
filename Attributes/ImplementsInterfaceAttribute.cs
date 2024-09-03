namespace OpenSteamClient.DI.Attributes;

/// <summary>
/// Add this attribute to a class to register an implemented interface, along with the object.
/// </summary>
/// <typeparam name="T">The interface to register</typeparam>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class DIRegisterInterfaceAttribute<T> : System.Attribute
{
    public DIRegisterInterfaceAttribute() { }
}