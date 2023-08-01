namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    private static void SlugcatStatsOnctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    {
        orig(self, slugcat, malnourished);
        
        if (slugcat == Plugin.NoirName)
        {
            self.generalVisibilityBonus = -0.1f;
            self.visualStealthInSneakMode = 0.6f;
            self.loudnessFac = 0.75f;

            self.bodyWeightFac = 0.90f;
            self.throwingSkill = 1;

            self.runspeedFac = 0.8f;
            self.poleClimbSpeedFac = 1.35f;
            self.corridorClimbSpeedFac = 1.4f;

            self.foodToHibernate = 5;
            self.maxFood = 7;
        }
    }

    private const float NoirCrawlSpeedFac = 2.5f;
    private static bool PlayerOnAllowGrabbingBatflys(On.Player.orig_AllowGrabbingBatflys orig, Player self)
    {
        if (self.SlugCatClass == Plugin.NoirName) return false;
        return orig(self);
    }
}