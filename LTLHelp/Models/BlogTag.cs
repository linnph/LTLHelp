using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class BlogTag
{
    public int TagId { get; set; }

    public string? Name { get; set; }

    public string? Slug { get; set; }

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
