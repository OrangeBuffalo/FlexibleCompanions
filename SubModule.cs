using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using HarmonyLib;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace FlexibleCompanions
{
    public class FlexibleCompanions_SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var harmony = new Harmony("FlexibleCompanions");
            harmony.PatchAll();
        }
    }

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

    [HarmonyPatch(typeof(DefaultCompanionHiringPriceCalculationModel), "GetCompanionHiringPrice")]
    static class PatchGetCompanionHiringPrice
    {
        static void Postfix(Hero companion)
        {
            if (companion.IsWanderer)
            {
                // Reset attributes and focuses & potentially unspent points
                MethodInfo mClearFocuses = typeof(HeroDeveloper).GetMethod("ClearFocuses", BindingFlags.NonPublic | BindingFlags.Instance);
                mClearFocuses.Invoke(companion.HeroDeveloper, null);
                companion.ClearAttributes();
                companion.HeroDeveloper.ClearUnspentPoints();

                // Add default unspent attribute and focus points based on hero level
                MethodInfo mSetupDefaultPoints = typeof(HeroDeveloper).GetMethod("SetupDefaultPoints", BindingFlags.NonPublic | BindingFlags.Instance);
                mSetupDefaultPoints.Invoke(companion.HeroDeveloper, null);

                // Re-distribute attributes and focus points
                // Remove some unspent points based on a settings and re-add them after the distribution, so that they are unspent
                // when the companion is hired by the player.
                int attributePts = Math.Min(companion.HeroDeveloper.UnspentAttributePoints, Settings.Instance.UnspentAttributes);
                companion.HeroDeveloper.UnspentAttributePoints = companion.HeroDeveloper.UnspentAttributePoints - attributePts;
                int focusPts = Math.Min(companion.HeroDeveloper.UnspentFocusPoints, Settings.Instance.UnspentFocuses);
                companion.HeroDeveloper.UnspentFocusPoints = companion.HeroDeveloper.UnspentFocusPoints - focusPts;
                CharacterDevelopmentCampaignBehavior.DevelopCharacterStats(companion);
                companion.HeroDeveloper.ClearUnspentPoints();
                companion.HeroDeveloper.UnspentAttributePoints = attributePts;
                companion.HeroDeveloper.UnspentFocusPoints = focusPts;

                // Clear Perks and re-add them with higher required skill values based on settings
                // e.g. with a required skill value of +25, the last perk in each skill is not gonna be selected

                // Reset perks
                companion.ClearPerks();
                foreach (SkillObject skill in Skills.All)
                {
                    companion.HeroDeveloper.CheckOpenedPerks(skill);
                }

                // Redistribute perks with higher required skill values based on settings
                // e.g. with a required skill value of +25, the last perk in each skill is not gonna be selected
                var perks = new List<PerkObject>();
                foreach (int i in Enumerable.Range(0, companion.HeroDeveloper.NumberOfOpenedPerks - 1))
                {
                    PerkObject perk = companion.HeroDeveloper.GetOpenedPerk(i);
                    if (perk.RequiredSkillValue <= companion.GetSkillValue(perk.Skill) - Settings.Instance.UnspentPerks * 25)
                    {
                        if (perk.AlternativePerk != null && MBRandom.RandomFloat < 0.5f)
                        {
                            perk = perk.AlternativePerk;
                        }
                        perks.Add(perk);
                    }
                }
                foreach (PerkObject p in perks)
                {
                    companion.HeroDeveloper.AddPerk(p);
                }
            }
        }
    }
}