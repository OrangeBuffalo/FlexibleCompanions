using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

using Bannerlord.ButterLib.SaveSystem.Extensions;

namespace FlexibleCompanions.CampaignBehaviours
{
    internal sealed class FlexibleCompanionsBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(this, new Action<Hero>(OnNewCompanionAdded));
        }

        // Keep track of heroes that have been respeced
        // We don't want to respec a companion that have been kicked out of the player clan
        // and recruited again later.
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncDataAsJson("FlexibleCompanions", ref _flexibleCompanions);
        }

        [SaveableField(1)]
        public List<uint> _flexibleCompanions = new List<uint>();

        private bool HasBeenRespec(Hero hero)
        {
            if (_flexibleCompanions.Contains(hero.Id.InternalValue))
            {
                return true;
            }
            else
            {
                _flexibleCompanions.Add(hero.Id.InternalValue);
                return false;
            }
        }

        private void RespecHero(Hero hero)
        {
            // Reset attributes and focuses & potentially unspent points
            MethodInfo mClearFocuses = typeof(HeroDeveloper).GetMethod("ClearFocuses", BindingFlags.NonPublic | BindingFlags.Instance);
            mClearFocuses.Invoke(hero.HeroDeveloper, null);
            hero.ClearAttributes();
            hero.HeroDeveloper.ClearUnspentPoints();

            // Add default unspent attribute and focus points based on hero level
            MethodInfo mSetupDefaultPoints = typeof(HeroDeveloper).GetMethod("SetupDefaultPoints", BindingFlags.NonPublic | BindingFlags.Instance);
            mSetupDefaultPoints.Invoke(hero.HeroDeveloper, null);

            // Re-distribute attributes and focus points
            // Remove some unspent points based on a settings and re-add them after the distribution, so that they are unspent
            // when the companion is hired by the player.
            int attributePts = Math.Min(hero.HeroDeveloper.UnspentAttributePoints, Settings.Instance.UnspentAttributes);
            hero.HeroDeveloper.UnspentAttributePoints = hero.HeroDeveloper.UnspentAttributePoints - attributePts;
            int focusPts = Math.Min(hero.HeroDeveloper.UnspentFocusPoints, Settings.Instance.UnspentFocuses);
            hero.HeroDeveloper.UnspentFocusPoints = hero.HeroDeveloper.UnspentFocusPoints - focusPts;
            CharacterDevelopmentCampaignBehavior.DevelopCharacterStats(hero);
            hero.HeroDeveloper.ClearUnspentPoints();
            hero.HeroDeveloper.UnspentAttributePoints = attributePts;
            hero.HeroDeveloper.UnspentFocusPoints = focusPts;

            // Clear Perks and re-add them with higher required skill values based on settings
            // e.g. with a required skill value of +25, the last perk in each skill is not gonna be selected

            // Reset perks
            hero.ClearPerks();
            foreach (SkillObject skill in Skills.All)
            {
                hero.HeroDeveloper.CheckOpenedPerks(skill);
            }

            // Redistribute perks with higher required skill values based on settings
            // e.g. with a required skill value of +25, the last perk in each skill is not gonna be selected
            var perks = new List<PerkObject>();
            foreach (int i in Enumerable.Range(0, hero.HeroDeveloper.NumberOfOpenedPerks - 1))
            {
                PerkObject perk = hero.HeroDeveloper.GetOpenedPerk(i);
                if (perk.RequiredSkillValue <= hero.GetSkillValue(perk.Skill) - Settings.Instance.UnspentPerks * 25)
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
                hero.HeroDeveloper.AddPerk(p);
            }
        }

        private void OnNewCompanionAdded(Hero companion)
        {
            if (companion.CompanionOf != Clan.PlayerClan || !companion.IsWanderer)
            {
                return;
            }

            if (HasBeenRespec(companion))
            {
                return;
            }

            RespecHero(companion);
        }
    }
}