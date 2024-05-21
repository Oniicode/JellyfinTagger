using Emby.Naming.TV;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace JellyfinTagger.Services
{
    public class TaggerEpisodeProvider : ILocalMetadataProvider<Episode>
    {
        public const string ForcetagsFileName = ".forcetags";

        public TaggerEpisodeProvider(ILogger<TaggerEpisodeProvider> logger)
        {
            _logger = logger;
            _episodeResolver = new EpisodeResolver(new());
        }

        public string Name => PluginConstants.PluginName + " .forcetags workaround";

        public Task<MetadataResult<Episode>> GetMetadata(ItemInfo itemInfo, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            if (!ShouldProvideFor(itemInfo, directoryService))
                return EmptyResult;

            _logger.LogInformation("Tagger: .forcetags detected for {path}. Filling episode information from embedded tags.", itemInfo.Path);

            using var taggedFile = TagLib.File.Create(itemInfo.Path);
            var episode = new Episode
            {
                Name = ExtractSafe(taggedFile.Tag, tag => tag.Title),
                Overview = ExtractSafe(taggedFile.Tag, tag => tag.Description)
            };
            
            // We assume the tags are incomplete, if no title is set.
            if (string.IsNullOrWhiteSpace(episode.Name))
                return EmptyResult;

            //TODO: Read Episode and Season numbers from Embedded Tags where possible first.
            var fallbackEpisodeInfo = new Lazy<Emby.Naming.TV.EpisodeInfo?>(() => _episodeResolver.Resolve(itemInfo.Path, false));
            episode.IndexNumber ??= fallbackEpisodeInfo.Value?.EpisodeNumber;
            episode.IndexNumberEnd ??= fallbackEpisodeInfo.Value?.EndingEpisodeNumber;
            episode.ParentIndexNumber ??= fallbackEpisodeInfo.Value?.SeasonNumber;

            _logger.LogInformation("Tagger: Resolved: {path} -- S{parentIndexNumber}E{indexNumber}-{indexNumberEnd}: {episodeName}",
                itemInfo.Path, episode.ParentIndexNumber, episode.IndexNumber, episode.IndexNumberEnd, episode.Name);

            return Task.FromResult(new MetadataResult<Episode>
            {
                HasMetadata = true,
                Item = episode
            });
        }

        private static bool ShouldProvideFor(ItemInfo itemInfo, IDirectoryService directoryService)
            => directoryService.GetFiles(Path.GetDirectoryName(itemInfo.Path)!)
                .Any(file => file.Name.StartsWith(ForcetagsFileName, StringComparison.OrdinalIgnoreCase));

        private static string? ExtractSafe(TagLib.Tag tag, Func<TagLib.Tag, string?> expression)
        {
            if (tag is TagLib.Matroska.Tag mkvTag)
                // MKV Workaround.
                return mkvTag.Tags.Select(expression)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .LastOrDefault();

            return expression(tag);
        }

        private static Task<MetadataResult<Episode>> EmptyResult 
            => Task.FromResult(new MetadataResult<Episode>());

        private readonly ILogger<TaggerEpisodeProvider> _logger;
        private readonly EpisodeResolver _episodeResolver;
    }
}
