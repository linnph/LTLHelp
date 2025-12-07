using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class VolunteerActivity
{
    public int ActivityId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? MaxVolunteers { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<VolunteerAssignment> VolunteerAssignments { get; set; } = new List<VolunteerAssignment>();
}
