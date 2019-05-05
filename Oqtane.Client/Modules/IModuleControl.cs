namespace Oqtane.Modules
{
    public interface IModuleControl
    {
        string Title { get; }
        SecurityAccessLevelEnum SecurityAccessLevel { get; }
        string Actions { get; } // can be specified as a comma delimited set of values
    }
}
