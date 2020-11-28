using System;

namespace Oqtane.Infrastructure.Startup
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class OqtaneServicesAttribute : Attribute
    {
        public OqtaneServicesAttribute(Type servicesType)
        {
            if (servicesType == null)
            {
                throw new ArgumentNullException(nameof(servicesType));
            }

            if (!servicesType.IsSubclassOf(typeof(DefaultOqtaneServerServices)))
            {
                throw new ArgumentException($"The '{servicesType.Name}' should inherits from {nameof(IOqtaneServices)} or {nameof(DefaultOqtaneServerServices)}.");
            }

            ServicesType = servicesType;
        }

        public Type ServicesType { get; }
    }
}
