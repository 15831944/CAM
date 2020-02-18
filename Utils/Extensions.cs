﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace CAM
{
    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;
            var attribute = (DescriptionAttribute)fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute.Description;
        }

        public static double Round(this double value, int digits) => Math.Round(value, digits);

    }
}
