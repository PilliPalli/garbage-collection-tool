using System;
using System.Collections.Generic;

namespace Garbage_Collector.Model;

public partial class CleanupLog
{
    public int CleanupLogId { get; set; }

    public int? UserId { get; set; }

    public DateTime CleanupDate { get; set; }

    public int FilesDeleted { get; set; }

    public double SpaceFreedInMb { get; set; }

    public string CleanupType { get; set; } = null!;

    public virtual User? User { get; set; }
}
