using System.Runtime.CompilerServices;
using Solace.NoirThings;
using Color = UnityEngine.Color;

namespace Solace.SlugcatThings;
public static class PoacherClass
{
    // Technically this class is meant to hold variables used only for Poacher, but there's nothing barring me from using it with players in general. So i will. :)
    public class Poacher
    {
        // Poacher variables
        public int flicker;
        public int skullpos1;
        public int skullpos2;
        public int skullpos3;
        public Color customColor;
        public Color customColor2;
        public bool isPoacher;
        public bool IsSkullVisible;
        public bool IsInIntro;
        public bool isMakingPoppers;

        // General player variables
        public bool HatedHere;
        public bool JustGotMoonMark;
        public bool MoonMarkPassed;
        public int MarkExhaustion;
        public bool isRidingLizard;
        public float pointDir0;
        public float pointDir1;
        public int spearRotationTimer;
        public bool RainTimerExists;
        public DragonRiding.AbstractDragonRider rideStick;
        public Creature dragonSteed;
        public int grabCounter;
        public int glanceDir;

        // Friend variables
        public bool poleCrawlState;
        public bool longjump;
        public bool polejump;
        public bool upwardpolejump;
        public bool DoingAPoleJump;
        public bool LetGoOfPoleJump;
        public bool YesIAmLookingUpStopThinkingOtherwise;
        public bool WantsUp;
        public int poleSuperJumpTimer;
        public bool HighJumped;
        public int CrawlTurnCounter;
        public int AfterCrawlTurnCounter;
        public Player.AnimationIndex LastAnimation;
        public Overseer Iggy; // Yellow / 1
        public Overseer Wiggy; // Blue / 0
        public bool TriedSpawningOverseerInThisRoom;


        public Poacher()
        {
            isPoacher = false;
            isRidingLizard = false;
        }
    }
    public static readonly ConditionalWeakTable<Player, Poacher> CWT = new();
    public static Poacher GetPoacher(this Player player) => CWT.GetValue(player, _ => new());
    
    // Noir's CWT
    public static readonly ConditionalWeakTable<Player, NoirCatto.NoirData> NoirDeets = new ConditionalWeakTable<Player, NoirCatto.NoirData>();
    public static NoirCatto.NoirData GetNoir(this Player player) => NoirDeets.GetValue(player, _ => new(player));
    public static bool TryGetNoir(this Player player, out NoirCatto.NoirData noirData)
    {
        if (player.SlugCatClass == Solace.NoirName)
        {
            noirData = GetNoir(player);
            return true;
        }

        noirData = null;
        return false;
    }
}

public static class OverseerTracking
{
    public class Overseerer
    {
        public bool IAmAGuide;
        public Overseerer()
        {

        }
    }
    public static readonly ConditionalWeakTable<Overseer, Overseerer> CWT = new();
    public static Overseerer SeerData(this Overseer seer) => CWT.GetValue(seer, _ => new());

}
