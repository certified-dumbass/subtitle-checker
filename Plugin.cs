using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Dreamstreaming.SubtitleTracker;

public sealed class Settings : BasePluginConfiguration { }

public sealed class Plugin : BasePlugin<Settings>, IHasWebPages
{
    public Plugin(IApplicationPaths paths, IXmlSerializer serializer) : base(paths, serializer) { }
    public override string Name => "Dreamstreaming Subtitle Tracker";
    public override Guid Id => new("cb07cc67-f8cf-4a4f-8ac0-bc4242975f19");
    public override string Description => "See which movies and episodes have indexed subtitle tracks.";
    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo { Name = "DreamstreamingSubtitleTracker", EmbeddedResourcePath = "Tracker.admin.html", EnableInMainMenu = true };
    }
}
