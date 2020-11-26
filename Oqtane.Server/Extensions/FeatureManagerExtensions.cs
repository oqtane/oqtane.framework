namespace Microsoft.FeatureManagement
{
    public static class FeatureManagerExtensions
    {
        public static bool IsEnabled(this IFeatureManager featureManager, string feature)
            => featureManager.IsEnabledAsync(feature).GetAwaiter().GetResult();

        public static bool IsEnabled<TContext>(this IFeatureManager featureManager, string feature, TContext context)
            => featureManager.IsEnabledAsync(feature, context).GetAwaiter().GetResult();
    }
}
