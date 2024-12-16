using CogniCache.Domain.Repositories.SearchRepository;
using System.Text.RegularExpressions;

namespace CogniCache.Domain.Services
{
    public interface ISearchService
    {
        IEnumerable<SearchResult> Search(string searchQuery);
    }

    public class SearchService : ISearchService
    {
        private readonly ISearchRepository _searchRepository;
        private readonly IRenderService _formatService;

        public SearchService(
            ISearchRepository searchRepository,
            IRenderService formatService)
        {
            _searchRepository = searchRepository;
            _formatService = formatService;
        }

        public IEnumerable<SearchResult> Search(string searchQuery)
        {
            var result = _searchRepository.Search(searchQuery);
            if (result?.Snippets?.Any() == true)
            {
                var searchResults = result.Snippets.Select(r => new SearchResult
                {
                    NoteId = r.NoteId,
                    Title = r.Title,
                    Snippet = RemoveTitle(_formatService.ToHtml(r.Snippet))
                });
                return searchResults;
            }

            return [];
        }

        private static string RemoveTitle(string input)
        {
            string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";
            string title = "";

            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                title = match.Groups[1].Value;
            }

            if (string.IsNullOrEmpty(title))
                return input;

            return input.Replace(title, "");
        }

        private static string RemoveHtmlTags(string input)
        {
            string pattern = "<.*?>";
            return Regex.Replace(input, pattern, string.Empty);
        }
    }
}
