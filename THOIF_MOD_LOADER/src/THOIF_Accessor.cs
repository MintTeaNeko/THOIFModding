using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace THOIF_Mod_Loader
{
    public class THOIF_Accessor
    {
        private static FieldInfo GetFieldInfo(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo;
        }

        public static object GetFieldValue<T>(object obj, string fieldName)
        {
            FieldInfo info = GetFieldInfo(obj, fieldName);
            return (T) info.GetValue(obj);
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            FieldInfo info = GetFieldInfo(obj, fieldName);
            info.SetValue(obj, value);
        }
    }
}
