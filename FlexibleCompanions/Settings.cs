using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace FlexibleCompanions
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "FlexibleCompanions";
        public override string DisplayName => "Flexible Companions";
        public override string FolderName => "FlexibleCompanions";
        public override string FormatType => "json2";

        [SettingPropertyInteger("Unspent attributes", 0, 10, "0 Point(s)", Order = 0, RequireRestart = false, HintText = "Number of unspent attribute points when hiring a companion.")]
        [SettingPropertyGroup("Flexible Companions")]
        public int UnspentAttributes { get; set; } = 1;

        [SettingPropertyInteger("Unspent focuses", 0, 20, "0 Point(s)", Order = 1, RequireRestart = false, HintText = "Number of unspent focus points when hiring a companion.")]
        [SettingPropertyGroup("Flexible Companions")]
        public int UnspentFocuses { get; set; } = 4;

        [SettingPropertyInteger("Unspent perks", 0, 10, "0 Perk(s)", Order = 2, RequireRestart = false, HintText = "Number of unspent perks for each skill when hiring a companion.")]
        [SettingPropertyGroup("Flexible Companions")]
        public int UnspentPerks { get; set; } = 1;
    }
}