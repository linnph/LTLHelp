using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class BlogComment
{
    public int CommentId { get; set; }

    public int BlogPostId { get; set; }

    public int? UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsApproved { get; set; }

    public virtual BlogPost BlogPost { get; set; } = null!;

    public virtual User? User { get; set; }
}
