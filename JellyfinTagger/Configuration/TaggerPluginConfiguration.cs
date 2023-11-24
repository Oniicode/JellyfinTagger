using MediaBrowser.Model.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyfinTagger.Configuration
{
    public class TaggerPluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// If true, only episodes with a <see cref="ForcetagsFiles"/>-File in their directory will prefer embedded tags.
        /// </summary>
        public bool ForcetagsFilesEnabled { get; set; } = true;
        public string[] ForcetagsFiles { get; set; } = new string[] { ".forcetags", ".ft" };
    }
}
