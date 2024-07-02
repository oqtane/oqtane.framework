namespace Oqtane.Interfaces
{
    /// <summary>
    /// Rich text editor provider interface.
    /// </summary>
    public interface ITextEditorProvider
    {
        /// <summary>
        /// The text editor provider name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The text editor type full name.
        /// </summary>
        string EditorType { get; }
    }
}
