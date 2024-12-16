using CogniCache.Domain.Repositories.NoteRepository;

namespace CogniCache.Application.Queries
{
    public record GetAllTagsQuery;

    public class GetAllTagsQueryHandler : IRequest<GetAllTagsQuery, GetAllTagsQueryResponse>
    {
        private readonly INoteRepository _noteRepository;

        public GetAllTagsQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public GetAllTagsQueryResponse Handle(GetAllTagsQuery request)
        {
            var notes = _noteRepository.GetAll();
            var tags = new Dictionary<string, TagListModel>();

            foreach (var note in notes)
            {
                if (note.Tags.Count != 0)
                {
                    foreach (var tag in note.Tags)
                    {
                        if (tags.TryGetValue(tag, out TagListModel? value))
                        {
                            var model = value;
                            model.NoteCount++;
                            tags[tag] = model;
                        }
                        else
                        {
                            tags.Add(tag, new TagListModel
                            {
                                Tag = tag,
                                NoteCount = 1,
                                SortPriority = 1
                            });
                        }
                    }
                }
                else
                {
                    if (tags.TryGetValue("Untagged", out TagListModel? value))
                    {
                        var model = value;
                        model.NoteCount++;
                        tags["Untagged"] = model;

                    }
                    else
                    {
                        tags.Add("Untagged", new TagListModel
                        {
                            Tag = "Untagged",
                            NoteCount = 1,
                            SortPriority = 0
                        });
                    }
                }
            }

            var sorted = tags
                .OrderBy(t => t.Value.SortPriority)
                .ThenBy(t => t.Key)
                .ToDictionary(t => t.Key, t => t.Value);

            return new GetAllTagsQueryResponse(sorted);
        }
    }

    public record GetAllTagsQueryResponse(Dictionary<string, TagListModel> Tags);

    public class TagListModel
    {
        public required string Tag { get; set; }
        public int NoteCount { get; set; }
        public int SortPriority { get; set; }
    }
}
