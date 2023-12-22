using TheFriend.SlugcatThings;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    private static void PlayerOncheckInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        if (!self.TryGetNoir(out var noirData)) return;

        //Moving all inputs one slot up
        for (var i = noirData.UnchangedInput.Length - 1; i > 0; i--)
        {
            noirData.UnchangedInput[i] = noirData.UnchangedInput[i - 1];
        }
        //Copying original unmodified input
        noirData.UnchangedInput[0] = self.input[0];
        noirData.YinputForPole = self.input[0].y > 0;
    }

    private static void ResetPoleInputBlocker(NoirData noirData)
    {
        if (noirData.Cat.animation != Player.AnimationIndex.StandOnBeam || !noirData.YinputForPole)
            noirData.YinputForPoleBlocker = 0;
    }

    private static void ModifyLeapInput(Player self)
    {
        if (!self.standing && self.superLaunchJump >= 20)
        {
            if (self.input[0].y > 0)
            {
                self.input[0].y = 0;
                self.input[0].x = 0;
            }
        }
    }

    private static void ModifyPoleLeapInput(NoirData noirData)
    {
        var self = noirData.Cat;
        if (noirData.CanCrawlOnBeam() && self.input[0].x == 0 || (self.input[0].jmp && noirData.SuperCrawlPounce >= 20))
        {
            if (self.input[0].jmp)
            {
                self.input[0].jmp = false;
                self.input[0].x = 0;
                self.input[0].y = 0;
                noirData.YinputForPole = false;
                noirData.YinputForPoleBlocker = 10;

                if (noirData.SuperCrawlPounce < 20)
                {
                    noirData.SuperCrawlPounce++;
                }
            }
        }
    }
}