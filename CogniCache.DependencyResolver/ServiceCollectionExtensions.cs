using CogniCache.Application;
using CogniCache.Application.Queries;
using CogniCache.Domain.Services;
using CogniCache.Domain;
using CogniCache.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;
using CogniCache.Infrastructure.LiteDb;
using CogniCache.Infrastructure.Lucene;
using CogniCache.Domain.Repositories.TagRepository;
using CogniCache.Shared.Services;

namespace CogniCache.DependencyResolver
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShared(this IServiceCollection services)
        {
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IPubSubService, PubSubService>();
            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Application Use Cases
            services.AddSingleton<IRequest<GetAllTagsQuery, GetAllTagsQueryResponse>, GetAllTagsQueryHandler>();

            return services;
        }

        public static IServiceCollection AddDomain(this IServiceCollection services) {
            // Domain
            services.AddSingleton(new AppState());
            services.AddSingleton<INoteService, NoteService>();
            services.AddSingleton<ISearchService, SearchService>();
            services.AddSingleton<IIdService, IdService>();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services) {
            // Infrastructure
            services.AddSingleton<IConfiguration, Configuration>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IRenderService, MarkdownService>();
            services.AddSingleton<INoteRepository, NoteRepository>();
            services.AddSingleton<ITagRepository, TagRepository>();
            services.AddSingleton<ISearchRepository, SearchRepository>();

            return services;
        }
    }
}
