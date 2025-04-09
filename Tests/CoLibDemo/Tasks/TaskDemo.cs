// Written by Colin on 2025-02-11

namespace CoLibDemo.Tasks;

public class TaskDemo
{
    public static async Task Start()
    {
        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            cts.Cancel();
        });

        try
        {
            await Task.Run(async () =>
            {
                await Task.Delay(2000, cts.Token);
            }, cts.Token);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}