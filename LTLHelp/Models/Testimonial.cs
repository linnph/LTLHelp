using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class Testimonial
{
    public int TestimonialId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Position { get; set; }

    public int Rating { get; set; }

    public string Message { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }
}
