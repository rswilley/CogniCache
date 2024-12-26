using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.TagRepository;
using static Lucene.Net.Search.FieldCache;

namespace CogniCache.Application.Queries
{
    public record GetAllTagsQuery;

    public class GetAllTagsQueryHandler : IRequest<GetAllTagsQuery, GetAllTagsQueryResponse>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ITagRepository _tagRepository;

        public GetAllTagsQueryHandler(
            INoteRepository noteRepository,
            ITagRepository tagRepository)
        {
            _noteRepository = noteRepository;
            _tagRepository = tagRepository;
        }

        public GetAllTagsQueryResponse Handle(GetAllTagsQuery request)
        {
            var allTags = _tagRepository.GetAll();
            if (allTags.Count != 0)
            {
                return new GetAllTagsQueryResponse(allTags);
            }

            var tags = new List<Tag>();

            foreach (var note in _noteRepository.GetAll())
            {
                foreach (var tag in note.Tags)
                {
                    var parts = tag.Split('/');
                    Tag? parent = null;
                    var path = "";

                    for (int i = 0; parts.Length > i; i++)
                    {
                        var currentValue = parts[i];
                        path += currentValue + "/";

                        if (i == 0)
                        {
                            parent = tags.SingleOrDefault(t => t.Name == currentValue);
                            if (parent == null)
                            {
                                parent = new Tag
                                {
                                    Parent = parent,
                                    Name = currentValue,
                                    Path = path.TrimEnd('/')
                                };
                                tags.Add(parent);
                            }
                        } else
                        {
                            if (!parent!.Children.Select(c => c.Name).Contains(currentValue))
                            {
                                var children = parent.Children.ToList();
                                var child = new Tag {
                                    Parent = parent,
                                    Name = currentValue,
                                    Path = path.TrimEnd('/')
                                };
                                children.Add(child);
                                parent.Children = children;

                                parent = child;
                            }
                        }
                    }
                }
            }

            return new GetAllTagsQueryResponse(tags);
        }
    }

    public record GetAllTagsQueryResponse(IEnumerable<Tag> Tags);
}
