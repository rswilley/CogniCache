using CogniCache.Domain.Extensions;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Text;

namespace CogniCache.Shared.Services
{
    public interface INavigationService
    {
        void NavigateToTag(string tag);
        void NavigateToMemo(int id, string title);
        void NavigateToMemos();
        void NavigateToMemos(Filter filter);
    }

    public class NavigationService(NavigationManager navigationManager) : INavigationService
    {
        public void NavigateToTag(string tag)
        {
            var uri = "/notes/?tag=" + WebUtility.HtmlEncode(tag);
            navigationManager.NavigateTo(uri);
        }

        public void NavigateToMemo(int id, string title)
        {
            var uri = "/notes/" + id + "/" + title.ToSlug();
            navigationManager.NavigateTo(uri);
        }

        public void NavigateToMemos()
        {
            navigationManager.NavigateTo("/notes");
        }

        public void NavigateToMemos(Filter filter)
        {
            var queryStringBuilder = new StringBuilder();
            if (filter.DateBegin.HasValue)
            {
                queryStringBuilder.Append("&dateBegin=" + filter.DateBegin.Value.ToString("yyyy-MM-dd"));
            }
            if (filter.DateEnd.HasValue)
            {
                queryStringBuilder.Append("&dateEnd=" + filter.DateEnd.Value.ToString("yyyy-MM-dd"));
            }

            navigationManager.NavigateTo($"/notes/?{queryStringBuilder}");
        }
    }

    public class Filter
    {
        public DateTime? DateBegin { get; init; }
        public DateTime? DateEnd { get; init; }
    }
}
