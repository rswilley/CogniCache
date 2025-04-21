namespace CogniCache.Domain.Models;

public class NoteModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Snippet { get; set; }
    public required string Body { get; set; }
    public required string Html { get; set; }
    public List<string> Tags { get; set; } = [];
    public required DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public required bool IsStarred { get; set; }
    public bool IsDirty { get; set; }
    public bool IsSelected { get; set; }

    public string TagsAsString()
    {
        if (Tags != null && Tags.Count > 0)
        {
            return string.Join(',', Tags).TrimEnd(',');
        }

        return string.Empty;
    }
}
