using System;
using System.Collections.Generic;

namespace BikeVille.Models;

public partial class OperationLog
{
    public int LogId { get; set; }

    public string? TableName { get; set; }

    public string? OperationType { get; set; }

    public DateTime? OperationDate { get; set; }

    public string? UserName { get; set; }
}
