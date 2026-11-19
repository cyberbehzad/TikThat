using System;
using System.Collections.Generic;

namespace TikThat.Models;

public class BackupDataModel
{
    public string AppVersion { get; set; } = "1.0";
    public DateTime BackupDate { get; set; } = DateTime.Now;

   public UserProfile? Profile { get; set; }

    public List<TodoItem> Tasks { get; set; } = [];
}