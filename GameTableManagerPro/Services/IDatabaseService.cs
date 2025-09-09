using GameTableManagerPro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameTableManagerPro.Services;

public interface IDatabaseService
{
    Task<List<GamingTable>> GetAllTablesAsync();
    Task<GamingTable?> GetTableByIdAsync(int id);
    Task<GamingTable?> GetTableByHostnameAsync(string hostname);
    Task<bool> AddTableAsync(GamingTable table);
    Task<bool> UpdateTableAsync(GamingTable table);
    Task<bool> DeleteTableAsync(int id);
    
    Task<List<DeploymentHistory>> GetDeploymentHistoryAsync(int tableId);
    Task<bool> AddDeploymentHistoryAsync(DeploymentHistory history);
    
    Task<int> GetOnlineTablesCountAsync();
    Task<int> GetOfflineTablesCountAsync();
    Task<int> GetTablesNeedingAttentionCountAsync();
    
    Task<bool> TestConnectionAsync();
    Task<bool> BackupDatabaseAsync(string backupPath);
    Task<bool> RestoreDatabaseAsync(string backupPath);
}
