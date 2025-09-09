using System.Threading.Tasks;

namespace GameTableManagerPro.Services;

public interface IPowerShellService
{
    Task<PowerShellResult> ExecuteScriptAsync(string scriptPath, string[]? arguments = null, TimeSpan? timeout = null);
    Task<PowerShellResult> ExecuteCommandAsync(string command, TimeSpan? timeout = null);
    Task<bool> TestConnectionAsync(string hostname);
}

public class PowerShellResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
