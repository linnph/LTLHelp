using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class VolunteerApplication
{
    public int ApplicationId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Occupation { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
