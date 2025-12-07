using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class BlogCategory
{
    public int BlogCategoryId { get; set; }

    public string? Name { get; set; }

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
