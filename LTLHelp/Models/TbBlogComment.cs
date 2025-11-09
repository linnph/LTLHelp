using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbBlogComment
{
    public int CommentId { get; set; }

    public int BlogId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual TbBlog Blog { get; set; } = null!;
}
