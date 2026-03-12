using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public int? BlockId { get; set; }

    public int? FloorNumber { get; set; }

    public int? RowCount { get; set; }

    public int? ColumnCount { get; set; }

    public int? TotalCapacity { get; set; }

    public bool? IsActive { get; set; }

    public virtual Block? Block { get; set; }

    public virtual ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
