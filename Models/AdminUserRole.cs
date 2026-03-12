using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class AdminUserRole
{
    public int AdminUserRoleId { get; set; }

    public int? AdminUserId { get; set; }

    public int? RoleId { get; set; }

    public string? AssignedBy { get; set; }

    public DateTime? AssignedDate { get; set; }

    public virtual AdminUser? AdminUser { get; set; }

    public virtual Role? Role { get; set; }
}
