using System;

namespace Oqtane.Shared
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class OqtaneAssemblyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OqtaneIgnoreAttribute : Attribute
    {
    }
}
