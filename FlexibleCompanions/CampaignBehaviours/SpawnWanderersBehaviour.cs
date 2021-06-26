using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace FlexibleCompanions.CampaignBehaviours
{
    class SpawnWanderersBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
        }

        public override void SyncData(IDataStore dataStore) { }

        public void OnWeeklyTick()
        {
            List<CharacterObject> spawnedTemplates = new List<CharacterObject>();
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero.IsWanderer && hero.CompanionOf != Clan.PlayerClan)
                {
                    spawnedTemplates.Add(hero.Template);
                }
            }

            List<CharacterObject> nonSpawnedTemplates = new List<CharacterObject>();
            foreach (CharacterObject template in CharacterObject.Templates)
            {
                if (template.Occupation == Occupation.Wanderer && !spawnedTemplates.Contains(template))
                {
                    nonSpawnedTemplates.Add(template);
                }
            }

            if (nonSpawnedTemplates.Any())
            {
                Random random = new Random();
                int r = random.Next(nonSpawnedTemplates.Count);
                CharacterObject template = nonSpawnedTemplates[r];
                MethodInfo CreateCompanion = typeof(UrbanCharactersCampaignBehavior).GetMethod("CreateCompanion", BindingFlags.Instance | BindingFlags.NonPublic);
                CreateCompanion.Invoke(Campaign.Current.GetCampaignBehavior<UrbanCharactersCampaignBehavior>(), new object[] { template });
            }
        }
    }
}
