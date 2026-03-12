using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class UserSessionLog
{
    public long UserSessionLogId { get; set; }

    public int? AdminUserId { get; set; }

    public DateTime LoginDateTime { get; set; }

    public DateTime? LogoutDateTime { get; set; }

    public string? Ipaddress { get; set; }

    public string? MachineName { get; set; }

    public int? SessionDuration { get; set; }

    public string? LoginStatus { get; set; }

    public string? FailureReason { get; set; }

    public bool? IsActive { get; set; }

    public virtual AdminUser? AdminUser { get; set; }
}
