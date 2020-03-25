namespace Oqtane.Shared
{
    public class InstallConfig
    {
        public string Alias { get; set; }
        public string ConnectionString { get; set; }
        public string HostUser { get; set; }
        public string Password { get; set; }
        public string HostEmail { get; set; }
        public bool IsMaster { get; set; }
    }
}
