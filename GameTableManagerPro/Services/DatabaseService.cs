using GameTableManagerPro.Data;
using GameTableManagerPro.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameTableManagerPro.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DatabaseService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<GamingTable>> GetAllTablesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GamingTables
            .Include(g => g.DeploymentHistory)
            .OrderBy(g => g.Hostname)
            .ToListAsync();
    }

    public async Task<GamingTable?> GetTableByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GamingTables
            .Include(g => g.DeploymentHistory)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GamingTable?> GetTableByHostnameAsync(string hostname)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GamingTables
            .Include(g => g.DeploymentHistory)
            .FirstOrDefaultAsync(g => g.Hostname == hostname);
    }

    public async Task<bool> AddTableAsync(GamingTable table)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.GamingTables.Add(table);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateTableAsync(GamingTable table)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.GamingTables.Update(table);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteTableAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var table = await context.GamingTables.FindAsync(id);
            if (table != null)
            {
                context.GamingTables.Remove(table);
                await context.SaveChangesAsync();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<DeploymentHistory>> GetDeploymentHistoryAsync(int tableId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DeploymentHistories
            .Where(d => d.GamingTableId == tableId)
            .OrderByDescending(d => d.StartTime)
            .ToListAsync();
    }

    public async Task<bool> AddDeploymentHistoryAsync(DeploymentHistory history)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.DeploymentHistories.Add(history);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetOnlineTablesCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GamingTables.CountAsync(g => g.Status == "Online");
    }

    public async Task<int> GetOfflineTablesCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GamingTables.CountAsync(g => g.Status == "Offline");
    }

    public async Task<int> GetTablesNeedingAttentionCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GamingTables.CountAsync(g => g.Status == "Needs Attention");
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> BackupDatabaseAsync(string backupPath)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Database.ExecuteSqlRawAsync($"VACUUM INTO '{backupPath}'");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreDatabaseAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                return false;

            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Database.CloseConnectionAsync();
            
            // Close any existing connections
            var databasePath = context.Database.GetDbConnection().DataSource;
            File.Copy(backupPath, databasePath, true);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
