namespace Oqtane.Modules
{
    public interface IModule
    {
        string Name { get; }
        string Description { get; }
        string Version { get; }
        string Owner { get; }
        string Url { get; }
        string Contact { get; }
        string License { get; }
        string Dependencies { get; }
    }
}
