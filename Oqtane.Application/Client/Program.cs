using System.Threading.Tasks;

namespace Oqtane.Application.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Oqtane.Client.Program.Main(args);
        }
    }
}
