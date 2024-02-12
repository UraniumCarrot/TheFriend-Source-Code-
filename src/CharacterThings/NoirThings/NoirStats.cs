namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    public static void SlugcatStatsOnctor(SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    {
        if (slugcat == Plugin.NoirName)
        {
            self.generalVisibilityBonus = -0.2f;
            self.visualStealthInSneakMode = 0.75f;
            self.loudnessFac = 0.6f;

            self.bodyWeightFac = 0.85f;
            self.throwingSkill = 1;

            self.runspeedFac = 0.8f;
            self.poleClimbSpeedFac = 1.4f;
            self.corridorClimbSpeedFac = 2f;

            self.foodToHibernate = 5;
            self.maxFood = 7;
        }
    }

    private const float NoirCrawlSpeedFac = 2.5f;
    public static bool PlayerOnAllowGrabbingBatflys(On.Player.orig_AllowGrabbingBatflys orig, Player self)
    {
        if (self.SlugCatClass == Plugin.NoirName) return false;
        return orig(self);
    }
}