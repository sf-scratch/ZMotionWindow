using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.Extensions
{
    internal static class EnumExtension
    {
        // 获取枚举值的描述字符串的辅助方法
        public static string GetEnumDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attribute.Length > 0 ? attribute[0].Description : value.ToString();
        }
    }
}
