using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class ContactMessage
{
    public int ContactId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsHandled { get; set; }
}
