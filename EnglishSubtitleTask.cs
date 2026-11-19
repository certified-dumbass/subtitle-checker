using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Dreamstreaming.SubtitleTracker;

public sealed class EnglishSubtitleTask : IScheduledTask
{
    readonly ILibraryManager library;
    readonly ISubtitleManager subtitles;
    readonly ILogger<EnglishSubtitleTask> log;
    readonly string checkpoint;

    public EnglishSubtitleTask(ILibraryManager library, ISubtitleManager subtitles,
        IApplicationPaths paths, ILogger<EnglishSubtitleTask> log)
    {
        this.library = library;
        this.subtitles = subtitles;
        this.log = log;
        checkpoint = Path.Combine(paths.DataPath, "dreamstreaming-english-subtitles.cursor");
    }

    public string Name => "Find missing English subtitles (Wyzie)";
    public string Key => "DreamstreamingEnglishSubtitleBackfill";
    public string Category => "Dreamstreaming";
    public string Description => "Check movies and episodes for full English subtitles. Search up to 100 titles per run and resume on the next run.";
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
        new[] { new TaskTriggerInfo { Type = TaskTriggerInfoType.DailyTrigger, TimeOfDayTicks = TimeSpan.FromHours(3).Ticks } };

    static bool English(string language) =>
        string.Equals(language, "eng", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ||
        (language?.StartsWith("en-", StringComparison.OrdinalIgnoreCase) ?? false);

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken token)
    {
        var ids = library.GetItemIds(new InternalItemsQuery {
            IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode }, Recursive = true,
            IsVirtualItem = false
        }).Distinct().OrderBy(id => id).ToArray();
        var cursor = File.Exists(checkpoint) ? Guid.Parse(File.ReadAllText(checkpoint)) : Guid.Empty;
        var ordered = ids.Where(i => i.CompareTo(cursor) > 0).Concat(ids.Where(i => i.CompareTo(cursor) <= 0));
        int checkedCount = 0, searches = 0, downloads = 0, misses = 0;
        try
        {
            foreach (var id in ordered)
            {
                token.ThrowIfCancellationRequested();
                if (library.GetItemById(id) is Video video && File.Exists(video.Path))
                {
                    var options = library.GetLibraryOptions(video);
                    if ((options.SubtitleDownloadLanguages?.Any(English) ?? false) &&
                        !options.DisabledSubtitleFetchers.Contains("Wyzie Subs", StringComparer.OrdinalIgnoreCase) &&
                        !video.GetMediaStreams().Any(s => s.Type == MediaStreamType.Subtitle && !s.IsForced && English(s.Language)))
                    {
                        var providers = subtitles.GetSupportedProviders(video);
                        if (!providers.Any(p => p.Name.Equals("Wyzie Subs", StringComparison.OrdinalIgnoreCase)))
                            throw new InvalidOperationException("Wyzie Subs is unavailable for this item type.");
                        var episode = video as Episode;
                        searches++;
                        var results = await subtitles.SearchSubtitles(new SubtitleSearchRequest {
                            MediaPath = video.Path, Name = video.Name, Language = "eng",
                            ContentType = episode == null ? VideoContentType.Movie : VideoContentType.Episode,
                            ProviderIds = video.ProviderIds, ProductionYear = video.ProductionYear,
                            IndexNumber = video.IndexNumber, IndexNumberEnd = episode?.IndexNumberEnd,
                            ParentIndexNumber = video.ParentIndexNumber, SeriesName = episode?.SeriesName,
                            RuntimeTicks = video.RunTimeTicks, IsAutomated = true, IsPerfectMatch = false,
                            DisabledSubtitleFetchers = providers.Select(p => p.Name)
                                .Where(n => !n.Equals("Wyzie Subs", StringComparison.OrdinalIgnoreCase)).ToArray()
                        }, token);
                        token.ThrowIfCancellationRequested();
                        var match = results.FirstOrDefault(s => English(s.ThreeLetterISOLanguageName) &&
                            s.Forced != true && s.AiTranslated != true &&
                            s.ProviderName.Equals("Wyzie Subs", StringComparison.OrdinalIgnoreCase));
                        if (match == null) misses++;
                        else
                        {
                            await subtitles.DownloadSubtitles(video, options, match.Id, token);
                            downloads++;
                            log.LogInformation("Downloaded English subtitle for {ItemId}: {Title}", id, video.Name);
                        }
                    }
                }
                checkedCount++;
                cursor = id;
                progress.Report(100d * checkedCount / ids.Length);
                if (searches >= 100) break;
            }
        }
        finally
        {
            if (checkedCount > 0)
            {
                File.WriteAllText(checkpoint + ".tmp", cursor.ToString());
                File.Move(checkpoint + ".tmp", checkpoint, true);
            }
            log.LogInformation("English subtitle run: checked {Checked}, searched {Searches}, downloaded {Downloads}, empty or unsuitable {Misses}", checkedCount, searches, downloads, misses);
        }
    }
}
