using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Helpers
{
    public static class CommonHelper
    {
        public static OptionRightType? ParseOptionType(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            s = s.Trim().ToLowerInvariant();
            if (s.StartsWith("p") || s.Contains("put") || s.Contains("pe"))
                return OptionRightType.Put;
            return OptionRightType.Call;
        }
    }
}
