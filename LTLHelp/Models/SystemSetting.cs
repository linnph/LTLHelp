using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class SystemSetting
{
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? Description { get; set; }
}
