using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GameTableManagerPro.Services;

public class PowerShellService : IPowerShellService
{
    public async Task<PowerShellResult> ExecuteScriptAsync(string scriptPath, string[]? arguments = null, TimeSpan? timeout = null)
    {
        if (!File.Exists(scriptPath))
        {
            return new PowerShellResult
            {
                Success = false,
                Error = $"Script file not found: {scriptPath}",
                ExitCode = -1
            };
        }

        var argumentString = arguments != null ? string.Join(" ", arguments) : "";
        var fullCommand = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" {argumentString}".Trim();

        return await ExecutePowerShellCommandAsync(fullCommand, timeout);
    }

    public async Task<PowerShellResult> ExecuteCommandAsync(string command, TimeSpan? timeout = null)
    {
        return await ExecutePowerShellCommandAsync(command, timeout);
    }

    public async Task<bool> TestConnectionAsync(string hostname)
    {
        try
        {
            var result = await ExecuteCommandAsync($"Test-Connection -ComputerName {hostname} -Count 1 -Quiet");
            return result.Success && result.Output.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task<PowerShellResult> ExecutePowerShellCommandAsync(string command, TimeSpan? timeout = null)
    {
        var result = new PowerShellResult();
        var startTime = DateTime.UtcNow;

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromMinutes(5));
            
            process.Start();
            
            // Read output and error asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Wait for process exit with timeout
            var processTask = process.WaitForExitAsync(cts.Token);
            await Task.WhenAll(processTask, outputTask, errorTask);

            result.Output = await outputTask;
            result.Error = await errorTask;
            result.ExitCode = process.ExitCode;
            result.Success = process.ExitCode == 0;
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.Error = "Command execution timed out";
            result.ExitCode = -2;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = $"Execution failed: {ex.Message}";
            result.ExitCode = -1;
        }
        finally
        {
            result.ExecutionTime = DateTime.UtcNow - startTime;
        }

        return result;
    }
}

// Helper extension for Process.WaitForExitAsync
public static class ProcessExtensions
{
    public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        process.EnableRaisingEvents = true;
        process.Exited += (sender, args) => tcs.TrySetResult(true);
        
        if (cancellationToken != default)
        {
            cancellationToken.Register(() => tcs.TrySetCanceled());
        }

        return process.HasExited ? Task.CompletedTask : tcs.Task;
    }
}
