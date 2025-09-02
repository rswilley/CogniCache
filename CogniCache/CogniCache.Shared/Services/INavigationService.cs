using Microsoft.AspNetCore.Components;
using System.Net;

namespace CogniCache.Shared.Services
{
    public interface INavigationService
    {
        void NavigateToTag(string tag);
        void NavigateToMemo(int id);
        void NavigateToMemos();
    }

    public class NavigationService(NavigationManager navigationManager) : INavigationService
    {
        private readonly NavigationManager _navigationManager = navigationManager;

        public void NavigateToTag(string tag)
        {
            var uri = "/memos/?tags=" + WebUtility.HtmlEncode(tag);
            _navigationManager.NavigateTo(uri);
        }

        public void NavigateToMemo(int id)
        {
            var uri = "/memos/" + id;
            _navigationManager.NavigateTo(uri);
        }

        public void NavigateToMemos()
        {
            _navigationManager.NavigateTo("/memos");
        }
    }
}
