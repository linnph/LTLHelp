using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbTestimonial
{
    public int TestimonialId { get; set; }

    public string DonorName { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? Avatar { get; set; }

    public string? DonorRole { get; set; }

    public DateTime? CreatedAt { get; set; }
}
