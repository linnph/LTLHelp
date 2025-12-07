using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class VolunteerAssignment
{
    public int AssignmentId { get; set; }

    public int VolunteerId { get; set; }

    public int ActivityId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public string? Status { get; set; }

    public virtual VolunteerActivity Activity { get; set; } = null!;

    public virtual Volunteer Volunteer { get; set; } = null!;
}
