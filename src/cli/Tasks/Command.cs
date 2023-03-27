namespace Avalanche.Cli.Tasks;

public abstract class Command
{
    internal readonly CancellationTokenSource _cancel;
    internal readonly AppConfig _config;

    private protected Command(bool waitForCompletion, AppConfig config, CancellationTokenSource cancel)
    {
        WaitForCompletion = waitForCompletion;
        _cancel = cancel;
        _config = config;
    }

    public bool WaitForCompletion { get; }

    public async Task<bool> Run()
    {
        return await SafeRun(RunCommand);
    }

    public async Task<bool> SafeRun(Func<Task<bool>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception in background task " + GetType().Name);
            _cancel.Cancel();
        }

        return false;
    }

    public void RunInBackground(Func<Task<bool>> func)
    {
        Task.Run(async () =>
        {
            await SafeRun(func);
        });
    }

    public abstract Task<bool> RunCommand();
}