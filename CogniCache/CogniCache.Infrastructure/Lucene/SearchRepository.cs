using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;
using CogniCache.Domain.Services;
using CogniCache.Infrastructure.Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LuceneDirectory = Lucene.Net.Store.Directory;
using OpenMode = Lucene.Net.Index.OpenMode;
using SearchResult = CogniCache.Domain.Repositories.SearchRepository.SearchResult;

namespace CogniCache.Infrastructure.Lucene
{
    public class SearchRepository : ISearchRepository
    {
        private readonly IConfiguration _config;
        private readonly string _indexPath;
        private readonly LuceneVersion _luceneVersion;

        public SearchRepository(IConfiguration config)
        {
            _config = config;
            _indexPath = Path.Combine(_config.AppDirectory, "notes_index");
            _luceneVersion = LuceneVersion.LUCENE_48;
        }

        public void Create(Note note)
        {
            using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
            using IndexWriter writer = new IndexWriter(indexDir, GetIndexWriterConfig().config);

            writer.AddDocument(note.ToDocument());

            //Flush and commit the index data to the directory
            writer.Commit();
        }

        public void Update(Note note)
        {
            DeleteById(note.Id);
            Create(note);
        }

        public void DeleteById(int noteId)
        {
            using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
            using IndexWriter writer = new IndexWriter(indexDir, GetIndexWriterConfig().config);

            writer.DeleteDocuments(new Term(nameof(Note.Id), noteId.ToString()));

            //Flush and commit the index data to the directory
            writer.Commit();
        }

        public SearchResult Search(string searchQuery)
        {
            using LuceneDirectory indexDir = FSDirectory.Open(_indexPath);
            using var reader = DirectoryReader.Open(indexDir);
            IndexSearcher searcher = new IndexSearcher(reader);

            var analyzer = GetStandardAnalyzer();
            var parser = new MultiFieldQueryParser(
                _luceneVersion,
                [nameof(Note.Title), nameof(Note.Body)],
                analyzer, new Dictionary<string, float>
                {
                    { nameof(Note.Title), 2.0f }, // Boost title field
                    { nameof(Note.Body), 1.0f }   // Default boost for body
                }
            );

            Query query;
            try
            {
                query = parser.Parse(searchQuery);
            }
            catch (ParseException)
            {
                // Fallback for invalid query syntax
                query = parser.Parse(QueryParserBase.Escape(searchQuery));
            }

            TopDocs topDocs = searcher.Search(query, int.MaxValue);
            var snippets = GetSnippets(query, topDocs, searcher, analyzer);

            return new SearchResult
            {
                TotalCount = topDocs.TotalHits,
                Snippets = snippets
            };
        }

        private static List<SearchSnippet> GetSnippets(Query query, TopDocs topDocs, IndexSearcher searcher, Analyzer analyzer)
        {
            var snippets = new List<SearchSnippet>();
            var scorer = new QueryScorer(query);
            var highlighter = new Highlighter(new SimpleHTMLFormatter("==", "=="), scorer)
            {
                // Set fragment size (characters)
                TextFragmenter = new SimpleFragmenter(150)
            };

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(scoreDoc.Doc);
                var content = doc.Get(nameof(Note.Body));

                // Extract a fragment with highlighting
                using var tokenStream = analyzer.GetTokenStream(nameof(Note.Body), content);
                var snippet = highlighter.GetBestFragments(tokenStream, content, 1, "...");

                // If no highlight was found, use the beginning of the content
                if (string.IsNullOrEmpty(snippet) && !string.IsNullOrEmpty(content))
                {
                    snippet = content.Length > 150 ? string.Concat(content.AsSpan(0, 150), "...") : content;
                }

                var noteSearchItem = doc.ToNoteSearchItem();

                snippets.Add(new SearchSnippet
                {
                    NoteId = noteSearchItem.Id,
                    Title = noteSearchItem.Title,
                    Snippet = snippet
                });
            }

            return snippets;
        }

        private (IndexWriterConfig config, Analyzer analyzer) GetIndexWriterConfig()
        {
            var standardAnalyzer = GetStandardAnalyzer();
            return (new IndexWriterConfig(_luceneVersion, standardAnalyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            }, standardAnalyzer);
        }

        private Analyzer GetStandardAnalyzer()
        {
            return new StandardAnalyzer(_luceneVersion);
        }
    }
}
