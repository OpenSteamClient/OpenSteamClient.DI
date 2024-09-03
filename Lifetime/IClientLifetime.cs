namespace OpenSteamClient.DI.Lifetime;

public interface IClientLifetime {
    public Task RunStartup(IProgress<OperationProgress> progress);
    public Task RunShutdown(IProgress<OperationProgress> progress);
}

