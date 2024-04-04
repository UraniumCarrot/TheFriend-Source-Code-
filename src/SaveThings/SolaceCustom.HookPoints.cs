using TheFriend.PoacherThings;

namespace TheFriend.SaveThings;

public partial class SolaceCustom
{
    public static void RainWorldGameOnWin(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
    {
        if (self.manager.upcomingProcess != null)
        {
            orig(self, malnourished);
            return;
        }

        LanternSpear.CheckShelter(self);

        orig(self, malnourished); //Save after orig so malnourished is fetched properly

        if (self.session is StoryGameSession storySession)
            LanternSpear.SavePositions(storySession);
    }

    public static void StoryGameSessionOnctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
    {
        orig(self, savestatenumber, game);

        LanternSpear.LoadPositions(self);
    }
}