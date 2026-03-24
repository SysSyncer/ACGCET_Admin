using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.Services
{
    /// <summary>
    /// Loads the logged-in user's role permissions and provides permission checking.
    /// Implements RBAC enforcement as described in the abstract.
    /// Access via UserPermissionService.Current after login.
    /// </summary>
    public class UserPermissionService
    {
        private readonly List<RolePermissionEntry> _permissions = new();
        private bool _isSuperAdmin;

        /// <summary>Global singleton instance set after DI registration.</summary>
        public static UserPermissionService Current { get; set; } = new();

        public bool IsSuperAdmin => _isSuperAdmin;

        public async Task LoadPermissionsAsync(AcgcetDbContext db, int adminUserId)
        {
            _permissions.Clear();

            var userRoles = await db.AdminUserRoles
                .Where(ur => ur.AdminUserId == adminUserId)
                .Select(ur => ur.Role!.RoleName)
                .ToListAsync();

            _isSuperAdmin = userRoles.Any(r =>
                r == "Super Admin" || r == "Administrator" || r == "COE" || r == "CEO");

            var roleIds = await db.AdminUserRoles
                .Where(ur => ur.AdminUserId == adminUserId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var perms = await db.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => roleIds.Contains(rp.RoleId))
                .ToListAsync();

            foreach (var rp in perms)
            {
                _permissions.Add(new RolePermissionEntry
                {
                    PermissionCode = rp.Permission?.PermissionCode ?? "",
                    ModuleName = rp.Permission?.ModuleName ?? "",
                    CanCreate = rp.CanCreate ?? false,
                    CanRead = rp.CanRead ?? false,
                    CanUpdate = rp.CanUpdate ?? false,
                    CanDelete = rp.CanDelete ?? false
                });
            }
        }

        public bool HasPermission(string permissionCode, string accessType)
        {
            if (_isSuperAdmin) return true;

            var perm = _permissions.FirstOrDefault(p => p.PermissionCode == permissionCode);
            if (perm == null) return false;

            return accessType switch
            {
                "Create" => perm.CanCreate,
                "Read" => perm.CanRead,
                "Update" => perm.CanUpdate,
                "Delete" => perm.CanDelete,
                _ => false
            };
        }

        public bool CanCreate(string permissionCode) => HasPermission(permissionCode, "Create");
        public bool CanRead(string permissionCode) => HasPermission(permissionCode, "Read");
        public bool CanUpdate(string permissionCode) => HasPermission(permissionCode, "Update");
        public bool CanDelete(string permissionCode) => HasPermission(permissionCode, "Delete");

        public void Clear()
        {
            _permissions.Clear();
            _isSuperAdmin = false;
        }

        private class RolePermissionEntry
        {
            public string PermissionCode { get; set; } = "";
            public string ModuleName { get; set; } = "";
            public bool CanCreate { get; set; }
            public bool CanRead { get; set; }
            public bool CanUpdate { get; set; }
            public bool CanDelete { get; set; }
        }
    }
}
