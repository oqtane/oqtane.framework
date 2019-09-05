namespace Oqtane.Modules
{
    public interface IModuleControl
    {
        SecurityAccessLevel SecurityAccessLevel { get; } // defines the security access level for this control - defaults to View
        string Title { get; } // title to display for this control - defaults to module title
        string Actions { get; } // allows for routing by configuration rather than by convention ( comma delimited ) - defaults to using component file name
        string ContainerType { get; } // container for embedding control - defaults to AdminContainer
    }
}
