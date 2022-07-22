namespace Oqtane.Infrastructure;

public interface IContentManager
{
    string GetContentPath(params string[] segments);
    string ContentRootPath { get; }
}
