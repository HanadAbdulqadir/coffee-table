using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTableManagerPro.Models;

public class DeploymentHistory
{
    [Key]
    public int Id { get; set; }
    public int GamingTableId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public required string Status { get; set; }
    public string? LogPath { get; set; }

    [ForeignKey("GamingTableId")]
    public virtual GamingTable? GamingTable { get; set; }
}
