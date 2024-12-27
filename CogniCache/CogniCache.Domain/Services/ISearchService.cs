using CogniCache.Domain.Repositories.SearchRepository;

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
        private readonly INoteService _noteService;

        public SearchService(
            ISearchRepository searchRepository,
            IRenderService formatService,
            INoteService noteService)
        {
            _searchRepository = searchRepository;
            _formatService = formatService;
            _noteService = noteService;
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
                    Snippet = _noteService.RemoveTitle(_formatService.ToHtml(r.Snippet))
                });
                return searchResults;
            }

            return [];
        }
    }
}
