using System.Net;

namespace CogniCache.Shared.Services
{
    public interface ILinkService
    {
        string CreateTagUrl(string tag);
    }

    public class LinkService : ILinkService
    {
        public string CreateTagUrl(string tag)
        {
            return "/notes/" + WebUtility.HtmlEncode(tag);
        }
    }
}
