using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyfinTagger;

public class Plugin 
    : BasePlugin<BasePluginConfiguration>
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) 
        : base(applicationPaths, xmlSerializer)
    {

    }

    public override string Name 
        => PluginConstants.PluginName;
    public override Guid Id 
        => Guid.Parse(PluginConstants.PluginGuid);
    public override string Description 
        => "Uses embedded MKV metadata as a primary source of metadata.";
}

internal class PluginConstants
{
    public const string
        PluginName = "Tagger",
        PluginGuid = "78052ebe-1b0c-4516-9417-09992c43da8e";
}
