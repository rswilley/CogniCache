using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;
using CogniCache.Infrastructure.Extensions;
using CogniCache.Infrastructure.Services;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
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

            var titleQuery = new TermQuery(new Term(nameof(Note.Title), searchQuery))
            {
                Boost = 2.0f
            };
            var bodyQuery = new TermQuery(new Term(nameof(Note.Body), searchQuery))
            {
                Boost = 1.0f
            };
            var booleanQuery = new BooleanQuery()
            {
                { titleQuery, Occur.SHOULD },
                { bodyQuery, Occur.SHOULD }
            };

            TopDocs topDocs = searcher.Search(booleanQuery, int.MaxValue);

            var snippets = new List<SearchSnippet>();
            var highlighter = new SimpleHTMLFormatter("<b>", "</b>");

            var analyzer = GetStandardAnalyzer();
            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(scoreDoc.Doc);
                var content = doc.Get(nameof(Note.Body));
                var tokenStream = analyzer.GetTokenStream(nameof(Note.Body), content);
                var snippet = highlighter.HighlightTerm(content, new TokenGroup(tokenStream));

                var noteSearchItem = doc.ToNoteSearchItem();

                snippets.Add(new SearchSnippet
                {
                    NoteId = noteSearchItem.Id,
                    Title = noteSearchItem.Title,
                    Snippet = snippet
                });
            }

            return new SearchResult
            {
                TotalCount = topDocs.TotalHits,
                Snippets = snippets
            };
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
