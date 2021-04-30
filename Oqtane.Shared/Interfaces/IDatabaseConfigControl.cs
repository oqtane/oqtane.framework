namespace Oqtane.Interfaces
{
    public interface IDatabaseConfigControl
    {
        string GetConnectionString();

        bool IsInstaller { get; set; }
    }
}
