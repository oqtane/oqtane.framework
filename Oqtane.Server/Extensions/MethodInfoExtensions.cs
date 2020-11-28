using System.Reflection;

namespace Oqtane.Extensions
{
    public static class MethodInfoExtensions
    {
        public static bool IsOverriden(this MethodInfo methodInfo)
            => (methodInfo.GetBaseDefinition() != methodInfo);
    }
}
