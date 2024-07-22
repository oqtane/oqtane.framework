using System.Threading.Tasks;

namespace Oqtane.Interfaces
{
    /// <summary>
    /// Text editor interface.
    /// </summary>
    public interface ITextEditor
    {
        string Name { get; }

        /// <summary>
        /// initializes the editor with the initialize content.
        /// </summary>
        /// <param name="content">the initialize content.</param>
        void Initialize(string content); 

        /// <summary>
        /// get content from the editor.
        /// </summary>
        /// <returns></returns>
        Task<string> GetContent();
    }
}
