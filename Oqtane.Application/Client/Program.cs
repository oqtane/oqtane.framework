using System.Threading.Tasks;

namespace Oqtane.Application.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // defer client startup to Oqtane - do not modify
            await Oqtane.Client.Program.Main(args);
        }
    }
}
