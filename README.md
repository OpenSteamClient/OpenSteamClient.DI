# OpenSteamClient.DI
OpenSteamClient's Dependency Injection and lifetime management library.
Provides a simple `IContainer` type, which OpenSteamClient libraries use for DI. 
You can implement `IContainer` yourself, or use `ContainerReferenceImplementation`.
You can implement `ILifetimeManager` yourself, or use `LifetimeManagerReferenceImplementation`.
