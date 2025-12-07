using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class Volunteer
{
    public int VolunteerId { get; set; }

    public int? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? JoinedAt { get; set; }

    public string? Status { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<VolunteerAssignment> VolunteerAssignments { get; set; } = new List<VolunteerAssignment>();
}
