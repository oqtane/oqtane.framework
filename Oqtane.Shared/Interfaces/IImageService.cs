using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IImageService
    {
        public string[] GetAvailableFormats();

        public string CreateImage(string filepath, int width, int height, string mode, string position, string background, string rotate, string format, string imagepath);
    }
}
