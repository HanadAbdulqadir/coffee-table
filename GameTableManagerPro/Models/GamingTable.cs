using System.ComponentModel.DataAnnotations;

namespace GameTableManagerPro.Models;

public class GamingTable
{
    [Key]
    public int Id { get; set; }
    public required string Hostname { get; set; }
    public required string IPAddress { get; set; }
    public required string Status { get; set; }
    public string? Version { get; set; }
    public DateTime LastSeen { get; set; }
    public string? HardwareInfo { get; set; }

    public virtual ICollection<DeploymentHistory> DeploymentHistory { get; set; } = new List<DeploymentHistory>();
}
