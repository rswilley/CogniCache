using System.Net;

namespace CogniCache.Domain.Repositories.TagRepository
{
    public class Tag
    {
        public Tag? Parent { get; set; }
        public required string Name { get; set; }
        public IEnumerable<Tag> Children { get; set; } = [];
        public string Path { get; set; } = string.Empty;
    }
}
