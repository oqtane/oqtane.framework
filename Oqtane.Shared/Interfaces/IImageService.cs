using System.IO;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IImageService
    {
        public string[] GetAvailableFormats();

        public Task<Stream> CreateImageAsync(Models.File file, int width, int height, string mode, string position, string background, string rotate, string format, string imageName);
    }
}
