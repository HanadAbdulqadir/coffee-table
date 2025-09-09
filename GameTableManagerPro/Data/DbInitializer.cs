using GameTableManagerPro.Models;

namespace GameTableManagerPro.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Look for any gaming tables.
        if (context.GamingTables.Any())
        {
            return;   // DB has been seeded
        }

        var statuses = new[] { "Online", "Offline", "Needs Attention", "Deploying" };
        var random = new Random();

        var tables = new List<GamingTable>();
        for (int i = 1; i <= 10; i++)
        {
            tables.Add(new GamingTable
            {
                Hostname = $"Table{i:D2}",
                IPAddress = $"192.168.1.{100 + i}",
                Status = statuses[random.Next(statuses.Length)],
                Version = $"1.{random.Next(0, 5)}.{random.Next(0, 10)}",
                LastSeen = DateTime.UtcNow.AddMinutes(-random.Next(1, 60 * 24)),
                HardwareInfo = "CPU: i5, RAM: 16GB, GPU: RTX 3060"
            });
        }

        context.GamingTables.AddRange(tables);
        context.SaveChanges();

        var deploymentHistories = new List<DeploymentHistory>();
        foreach (var table in tables)
        {
            for (int j = 0; j < random.Next(1, 5); j++)
            {
                var startTime = DateTime.UtcNow.AddDays(-random.Next(1, 30));
                deploymentHistories.Add(new DeploymentHistory
                {
                    GamingTableId = table.Id,
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(random.Next(5, 20)),
                    Status = "Success",
                    LogPath = $"C:/Logs/Deploy_{table.Hostname}_{startTime:yyyyMMddHHmmss}.log"
                });
            }
        }

        context.DeploymentHistories.AddRange(deploymentHistories);
        context.SaveChanges();
    }
}
