using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garbage_Collector.Model;

public partial class User
{

    [NotMapped]
    public string? DisplayRole { get; set; }
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CleanupLog> CleanupLogs { get; set; } = new List<CleanupLog>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
