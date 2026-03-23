using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class Log22180011
{
    public int LogId { get; set; }

    public string? TableName { get; set; }

    public string? OperationType { get; set; }

    public DateTime? OperationTime { get; set; }
}
