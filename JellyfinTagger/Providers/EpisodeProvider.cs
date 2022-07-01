using Emby.Naming.Common;
using Emby.Naming.TV;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyfinTagger.Providers
{
    public class EpisodeProvider : ILocalMetadataProvider<Episode>
    {
        private readonly ILogger<EpisodeProvider> _logger;
        private readonly NamingOptions _namingOptions = new NamingOptions();
        private readonly EpisodeResolver _episodeResolver;

        public EpisodeProvider(ILogger<EpisodeProvider> logger)
        {
            _logger = logger;
            _episodeResolver = new EpisodeResolver(_namingOptions);
        }

        public string Name => Constants.PluginName + " .forcetags workaround";

        public async Task<MetadataResult<Episode>> GetMetadata(ItemInfo info, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            using var file = TagLib.File.Create(info.Path);

            if(!directoryService.GetFiles(Path.GetDirectoryName(info.Path)).Any(file => file.Name.StartsWith(".forcetags", StringComparison.OrdinalIgnoreCase)))
                return Empty;
            _logger.LogInformation($"Tagger: .forcetags detected for {info.Path}. Filling episode information from embedded tags.");

            var correctTitle = file.Tag.Title;
            if (file.GetType() == typeof(TagLib.Matroska.File)) // MKVs require this workaround because of a bug in TagLib -.-
            {
                var combinedTag = (TagLib.Matroska.Tag)file.Tag;
                var latestTag = combinedTag.Tags.Last();
                if (latestTag.Title != null)
                    correctTitle = latestTag.Title;
            }

            var fallbackEpisode = _episodeResolver.Resolve(info.Path, false);
            //TODO: Read Episode and Season numbers
            //Jellyfin got unit tests for parsing those outta file names. Use those methods as a fallback.

            if (string.IsNullOrWhiteSpace(correctTitle) || fallbackEpisode == null)
                return Empty;

            _logger.LogInformation($"Tagger: Resolved: {info.Path} -- S{fallbackEpisode.SeasonNumber}E{fallbackEpisode.SeasonNumber}: {correctTitle}");

            return new MetadataResult<Episode>
            {
                HasMetadata = true,
                Item = new Episode
                {
                    Name = correctTitle,
                    IndexNumber = fallbackEpisode.EpisodeNumber,
                    ParentIndexNumber = fallbackEpisode.SeasonNumber,
                }
            };
        }

        private static MetadataResult<Episode> Empty => new MetadataResult<Episode>();
    }
}
