namespace CogniCache.Domain.Models;

public class MemoModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsStarred { get; set; }
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
