namespace OpenSteamClient.DI.Lifetime;

public interface ILogonLifetime {
    public Task RunLogon(IProgress<OperationProgress> progress);
    public Task RunLogoff(IProgress<OperationProgress> progress);
}