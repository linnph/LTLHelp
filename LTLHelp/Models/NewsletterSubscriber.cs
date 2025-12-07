using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class NewsletterSubscriber
{
    public int SubscriberId { get; set; }

    public string? Email { get; set; }

    public DateTime? SubscribedAt { get; set; }

    public bool? IsActive { get; set; }
}
