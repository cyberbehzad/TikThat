using SQLite;
using System.Diagnostics;
using TikThat.Models;

namespace TikThat.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized;

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized && _database is not null)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized && _database is not null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tikthat_storage.db3");

            _database = new SQLiteAsyncConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache
            );

            // Ensure Tasks table exists
            await _database.CreateTableAsync<TodoItem>();

            try
            {
                await _database.ExecuteAsync("PRAGMA journal_mode = WAL;");
                await _database.ExecuteAsync("PRAGMA synchronous = NORMAL;");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PRAGMA config warning: {ex.Message}");
            }

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<List<TodoItem>> GetAllTasksAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<TodoItem>().OrderByDescending(t => t.DueDate).ToListAsync();
    }

    public async Task<List<TodoItem>> GetTasksByDateAsync(DateTime date)
    {
        await EnsureInitializedAsync();
        var targetDate = date.Date;
        var nextDate = targetDate.AddDays(1);

        return await _database!.Table<TodoItem>()
            .Where(t => t.DueDate >= targetDate && t.DueDate < nextDate)
            .ToListAsync();
    }

    public async Task<List<TodoItem>> GetLocationTasksAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<TodoItem>()
            .Where(t => t.HasLocationReminder && !t.IsCompleted)
            .ToListAsync();
    }

    public async Task<int> SaveTaskAsync(TodoItem task)
    {
        await EnsureInitializedAsync();
        var existing = await _database!.Table<TodoItem>().Where(t => t.Id == task.Id).FirstOrDefaultAsync();
        if (existing is not null)
        {
            return await _database.UpdateAsync(task);
        }
        return await _database.InsertAsync(task);
    }

    public async Task<int> DeleteTaskAsync(TodoItem task)
    {
        await EnsureInitializedAsync();
        return await _database!.DeleteAsync(task);
    }

    public async Task InsertBatchTasksAsync(IEnumerable<TodoItem> tasks)
    {
        await EnsureInitializedAsync();
        await _database!.InsertAllAsync(tasks);
    }
}