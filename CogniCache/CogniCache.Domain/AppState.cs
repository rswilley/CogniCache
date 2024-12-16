namespace CogniCache.Domain
{
    public class AppState
    {
        public AppNavigation Navigation { get; set; } = new();
    }

    public class AppNavigation
    {
        public LinkedListNode<HistoryItem> CurrentNode { get; set; } = null!;
        public LinkedList<HistoryItem> History { get; set; } = [];
    }

    public class HistoryItem
    {
        public int Id { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
