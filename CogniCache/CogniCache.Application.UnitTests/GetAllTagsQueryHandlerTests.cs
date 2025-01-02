using CogniCache.Application.Queries;
using CogniCache.Domain.Enums;
using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.TagRepository;
using Moq;
using static Lucene.Net.Util.Fst.Util;

namespace CogniCache.Application.UnitTests
{
    public class GetAllTagsQueryHandlerTests
    {
        [Fact]
        public void Handle_NoExistingTagsFound_ReturnsHierarchy()
        {
            _tagRepositoryMock.Setup(x => x.GetAll()).Returns(new List<Tag>());

            _noteRepositoryMock.Setup(x => x.GetManyPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<NoteSortMode?>(), It.IsAny<string?>())).Returns(new List<Note>
            {
                new()
                {
                    Title = "Title",
                    Body = "",
                    FileName = "",
                    LastUpdatedDate = DateTime.UtcNow,
                    Tags = [ 
                        "this/is/a/hierarchy"
                    ]
                }
            });

            var subject = GetSubject();
            var result = subject.Handle(new GetAllTagsQuery());
            
            Assert.Equal("this/is/a/hierarchy", "");
        }

        private Mock<INoteRepository> _noteRepositoryMock = new Mock<INoteRepository>();
        private Mock<ITagRepository> _tagRepositoryMock = new Mock<ITagRepository>();

        private GetAllTagsQueryHandler GetSubject()
        {
            return new GetAllTagsQueryHandler(_noteRepositoryMock.Object, _tagRepositoryMock.Object);
        }
    }
}
