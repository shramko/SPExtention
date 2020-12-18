using System.Reflection;

namespace SPExtention
{
    class Utils
    {
        internal static PropertyInfo[] GetPropertyFields<T>(bool excludeBase = false)
        {
            var bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.GetProperty;

            if (excludeBase)
                bindingAttr = bindingAttr | BindingFlags.DeclaredOnly;

            return typeof(T).GetProperties(bindingAttr);
        }
    }
}
