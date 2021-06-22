using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace FlexibleCompanions
{
    public class SubModule : MBSubModuleBase
    {

        public static string Version => "1.1.0";
        public static string DisplayName => "Flexible Companions";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

        }

        protected override void OnGameStart(Game game, IGameStarter starterObject)
        {
            base.OnGameStart(game, starterObject);

            if (game.GameType is Campaign)
            {
                var initializer = (CampaignGameStarter)starterObject;
                initializer.AddBehavior(new CampaignBehaviours.FlexibleCompanionsBehaviour());
            }
        }
    }
}
