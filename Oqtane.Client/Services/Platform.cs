using Oqtane.UI;

namespace Oqtane.Services
{
    public class Platform : IPlatform
    {
        public Platform() : this(Runtime.Server)
        {

        }

        public Platform(Runtime runtime)
        {
            Runtime = runtime;
        }

        public Runtime Runtime { get; }

        public string Version => typeof(Platform).Assembly.GetName().Version.ToString(3);
    }
}
