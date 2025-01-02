using CogniCache.Domain.Services;
using Markdig;

namespace CogniCache.Infrastructure.Services
{
    public class MarkdownService : IRenderService
    {
        public string ToHtml(string value)
        {
            return Markdown.ToHtml(value ?? string.Empty);
        }
    }
}
