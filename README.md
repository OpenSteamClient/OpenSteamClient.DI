# OpenSteamClient.DI
OpenSteamClient's own Dependency Injection pseudo "framework".
Provides a simple `IContainer` type, which OpenSteamClient libraries use for DI. 
You can implement `IContainer` yourself, or use `ContainerReferenceImplementation`.