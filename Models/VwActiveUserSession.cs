using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class VwActiveUserSession
{
    public long UserSessionLogId { get; set; }

    public int? AdminUserId { get; set; }

    public string UserName { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime LoginDateTime { get; set; }

    public string? Ipaddress { get; set; }

    public string? MachineName { get; set; }

    public int? SessionDuration { get; set; }

    public int IsIdle { get; set; }
}
