using fftivc.utility.modloader.Template.Configuration;

using Reloaded.Mod.Interfaces.Structs;

using System.ComponentModel;
using System.Threading.Channels;

namespace fftivc.utility.modloader.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Merge nex/nxd changes")]
        [Description("Whether to merge Nex (.nxd) changes made to a specific nex table by multiple tables.\n" +
            "NOTE: This should always be enabled.")]
        [DefaultValue(true)]
        public bool MergeNexFileChanges { get; set; } = true;

        [DisplayName("Neutralize anti-debug")]
        [Description("(Advanced users only) Whether to disable anti-debugging.\n" +
            "For more information refer to: nenkai.github.io/ffxvi-modding/resources/other/debugging/")]
        [DefaultValue(true)]
        public bool DisableAntiDebugger { get; set; } = true;

        [DisplayName("Remove exception/crash handler")]
        [Description("(Advanced users only) Whether to remove the default exception handler.\n" +
            "Removes the 'An unexpected error has occurred.' message on crash.")]
        [DefaultValue(true)]
        public bool RemoveExceptionHandler { get; set; } = true;

        [Category("Debug")]
        [DisplayName("Log nex cell changes")]
        [Description("Whether to log specific row cell changes made to nex tables made by mods.\n" +
            "NOTE: \"Merge Nex / Nxd Changes\" must be enabled.")]
        [DefaultValue(false)]
        public bool LogNexCellChanges { get; set; } = false;

        [Category("Debug")]
        [DisplayName("Log general file accesses")]
        [Description("(Advanced users only) Whether to display general file accesses in the console.")]
        [DefaultValue(false)]
        public bool LogGeneralFileAccesses { get; set; } = false;

        [Category("Debug")]
        [DisplayName("Log G2D File Accesses")]
        [Description("(Advanced users only) Whether to show G2D file accesses in the console.")]
        [DefaultValue(false)]
        public bool LogG2DFileAccesses { get; set; } = false;

        [Category("Debug")]
        [DisplayName("Log FFTPack File Accesses")]
        [Description("(Advanced users only) Whether to show FFTPack.bin file accesses in the console.")]
        [DefaultValue(false)]
        public bool LogFFTPackFileAccesses { get; set; } = false;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
