using Menu.Remix.MixedUI;
using UnityEngine;

namespace TheFriend;

public class Options : OptionInterface
{
    public static Configurable<bool> NoFamine;
    public static Configurable<bool> FaminesForAll;
    public static Configurable<bool> LocalizedLizRep;
    public static Configurable<bool> LocalizedLizRepForAll;
    public static Configurable<bool> ExpeditionFamine;
    public static Configurable<bool> SolaceBlizzTimer;

    public static Configurable<bool> FriendAutoCrouch;
    public static Configurable<bool> PoleCrawl;
    public static Configurable<bool> FriendBackspear;
    public static Configurable<bool> FriendUnNerf;
    public static Configurable<bool> FriendRepLock;

    public static Configurable<bool> PoacherPupActs;
    public static Configurable<bool> PoacherBackspear;
    public static Configurable<bool> PoacherFreezeFaster;
    public static Configurable<bool> PoacherFoodParkour;

    public static Configurable<bool> LizRide;
    public static Configurable<bool> LizRideAll;
    public static Configurable<bool> LizRepMeter;
    public static Configurable<bool> LizRepMeterForAll;

    // Achievements
    public static Configurable<bool> SolaceFriendOEAchievement; // Live Well
    public static Configurable<bool> SolacePoacherOEAchievement; // A New Beginning
    public static Configurable<bool> SolaceFriendBadAscensionAchievement; // Betrayal
    public static Configurable<bool> SolaceFriendGoodAscensionAchievement; // Allegiance
    public static Configurable<bool> SolacePoacherBadAscensionAchievement; // Trapped
    public static Configurable<bool> SolacePoacherGoodAscensionAchievement; // Forgiveness
    public static Configurable<bool> SolacePebblesAchievement; // Alone No More
    public static Configurable<bool> SolacePebblesStolenEnlightenmentAchievement; // Broken Mind

    public Options()
    {
        FriendAutoCrouch = config.Bind("FriendAutoCrouch", true, new ConfigAcceptableList<bool>(true, false));
        PoleCrawl = config.Bind("PoleCrawl", true, new ConfigAcceptableList<bool>(true, false));
        FriendUnNerf = config.Bind("FriendUnNerf", false, new ConfigAcceptableList<bool>(true, false));
        FriendBackspear = config.Bind("FriendBackspear", false, new ConfigAcceptableList<bool>(true, false));
        FriendRepLock = config.Bind("FriendRepLock", true, new ConfigAcceptableList<bool>(true, false));

        PoacherPupActs = config.Bind("PoacherPupActs", true, new ConfigAcceptableList<bool>(true, false));
        PoacherBackspear = config.Bind("PoacherBackspear", false, new ConfigAcceptableList<bool>(true, false));
        PoacherFreezeFaster = config.Bind("PoacherFreezeFaster", false, new ConfigAcceptableList<bool>(true, false));
        PoacherFoodParkour = config.Bind("PoacherFoodParkour", true, new ConfigAcceptableList<bool>(true, false));

        ExpeditionFamine = config.Bind("ExpeditionFamine", false, new ConfigAcceptableList<bool>(true, false));
        NoFamine = config.Bind("NoFamine", false, new ConfigAcceptableList<bool>(true, false));
        FaminesForAll = config.Bind("FaminesForAll", false, new ConfigAcceptableList<bool>(true, false));
        LocalizedLizRep = config.Bind("LocalizedLizRep", true, new ConfigAcceptableList<bool>(true, false));
        LocalizedLizRepForAll = config.Bind("LocalizedLizRepForAll", false, new ConfigAcceptableList<bool>(true, false));
        SolaceBlizzTimer = config.Bind("SolaceBlizzTimer", false, new ConfigAcceptableList<bool>(true, false));

        LizRide = config.Bind("LizRide", false, new ConfigAcceptableList<bool>(true, false));
        LizRideAll = config.Bind("LizRideAll", false, new ConfigAcceptableList<bool>(true, false));
        LizRepMeter = config.Bind("LizRepMeter", false, new ConfigAcceptableList<bool>(true, false));
        LizRepMeterForAll = config.Bind("LizRepMeterForAll", false, new ConfigAcceptableList<bool>(true, false));

        // Achievements
        SolaceFriendOEAchievement = config.Bind("SolaceFriendOEAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherOEAchievement = config.Bind("SolacePoacherOEAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolaceFriendBadAscensionAchievement = config.Bind("SolaceFriendBadAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolaceFriendGoodAscensionAchievement = config.Bind("SolaceFriendGoodAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherBadAscensionAchievement = config.Bind("SolacePoacherBadAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherGoodAscensionAchievement = config.Bind("SolacePoacherGoodAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePebblesAchievement = config.Bind("SolacePebblesAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePebblesStolenEnlightenmentAchievement = config.Bind("SolacePebblesStolenEnlightenmentAchievement", false, new ConfigAcceptableList<bool>(true, false));
    }

    // Greyout-able boxes and labels
    static OpCheckBox NoFamineBox;
    static OpCheckBox AllFamineBox;
    static OpCheckBox ExpeditionFamineBox;
    static OpCheckBox LizRideBox;
    static OpCheckBox LizRideAllBox;
    static OpCheckBox LizRepMeterBox;
    static OpCheckBox LizRepMeterAllBox;
    static OpCheckBox LocalLizRepBox;
    static OpCheckBox LocalLizRepAllBox;

    public override void Update()
    {
        base.Update();

        if (NoFamineBox.value == "true")
        {
            AllFamineBox.greyedOut = true; 
            ExpeditionFamineBox.greyedOut = true;
        }
        else
        {
            AllFamineBox.greyedOut = false; 
            ExpeditionFamineBox.greyedOut = false;
        }
        
        LizRideAllBox.greyedOut = LizRideBox.value == "false";
        
        LizRepMeterAllBox.greyedOut = LizRepMeterBox.value == "false";
        
        LocalLizRepAllBox.greyedOut = LocalLizRepBox.value == "false";
    }
    public override void Initialize()
    {
        var opTab0 = new OpTab(this, "General");
        var opTab1 = new OpTab(this, "Characters");
        var opTab2 = new OpTab(this, "Experimental");
        var opTab3 = new OpTab(this, "Memorial");
        OpContainer genCont = new OpContainer(Vector2.zero);
        opTab0.AddItems(genCont);
        OpContainer charCont = new OpContainer(Vector2.zero);
        opTab1.AddItems(charCont);
        OpContainer achieveCont = new OpContainer(Vector2.zero);
        opTab1.AddItems(achieveCont);

        base.Initialize();
        Tabs = new[] { opTab0, opTab1, opTab2, opTab3 };

        var labelMod = new OpLabel(20, 600 - 30, Translate("Rain World: Solace Config - General Settings"), true);
        var labelVersion = new OpLabel(20, 600 - 30 - 20, Translate("Version 0.3.0"));
        var labelMod1 = new OpLabel(20, 600 - 30, Translate("Rain World: Solace Config - Character Settings"), true);
        var labelVersion1 = new OpLabel(20, 600 - 30 - 20, Translate("Version 0.3.0"));
        var labelMod2 = new OpLabel(20, 600 - 30, Translate("Rain World: Solace Config - Experimental Settings"), true);
        var labelVersion2 = new OpLabel(20, 600 - 30 - 20, Translate("Version 0.3.0"));

        // General
        #region General
        NoFamineBox = new OpCheckBox(NoFamine, new Vector2(50, 600 - 100)) { description = Translate("Famine mechanics and changes are disabled when checked") };
        var NoFamineLabel = new OpLabel(new(50, 600 - 118), Vector2.zero, Translate("Famines Disabled"));
        AllFamineBox = new OpCheckBox(FaminesForAll, new Vector2(50, 600 - 150)) { description = Translate("Famine mechanics and changes are given to every character") };
        var AllFamineLabel = new OpLabel(new(50, 600 - 168), Vector2.zero, Translate("Famines For All"));
        ExpeditionFamineBox = new OpCheckBox(ExpeditionFamine, new Vector2(50, 600 - 200)) { description = Translate("Forces famines to appear in Expedition Mode") };
        var ExpeditionFamineLabel = new OpLabel(new(50, 600 - 218), Vector2.zero, Translate("Expedition Famines"));

        var FamineLabel = new OpLabel(new Vector2(295, 600 - 236), Vector2.zero, Translate("Famines"), FLabelAlignment.Center, true);
        FSprite line0 = new FSprite("pixel");
        line0.color = labelMod.color;
        line0.scaleX = 600;
        line0.scaleY = 2;
        line0.SetPosition(new Vector2(300, 600 - 242));
        genCont.container.AddChild(line0);
        var RepLabel = new OpLabel(new Vector2(295, 600 - 242 - 23), Vector2.zero, Translate("Reputation"), FLabelAlignment.Center, true);

        LocalLizRepBox = new OpCheckBox(LocalizedLizRep, new Vector2(50, 600 - 236 - 60)) { description = Translate("Lizard reputation is localized for Solace slugcats when checked. May break things if disabled!") };
        var LocalLizRepLabel = new OpLabel(new(50, 600 - 236 - 78), Vector2.zero, Translate("Local Lizard Rep"));
        LocalLizRepAllBox = new OpCheckBox(LocalizedLizRepForAll, new Vector2(50, 600 - 236 - 110)) { description = Translate("Lizard reputation is localized for every character") };
        var LocalLizRepAllLabel = new OpLabel(new(50, 600 - 236 - 128), Vector2.zero, Translate("Local Liz Rep For All"));
        var SolaceTimerBox = new OpCheckBox(SolaceBlizzTimer, new Vector2(50, 600 - 236 - 160)) { description = Translate("Forces the rain timer to be visible for Solace slugcats and Saint") };
        var SolaceTimerLabel = new OpLabel(new(50, 600 - 236 - 178), Vector2.zero, Translate("Rain Timer"));


        #endregion
        // Characters
        #region Characters
        var FriendAutoCrouchBox = new OpCheckBox(FriendAutoCrouch, new Vector2(50, 600 - 100)) { description = Translate("Makes Friend crouch automatically after a standing jump") };
        var FriendAutoCrouchLabel = new OpLabel(new(50, 600 - 118), Vector2.zero, Translate("Automatic Crouch"));

        var FriendPoleCrawlBox = new OpCheckBox(PoleCrawl, new Vector2(50, 600 - 150)) { description = Translate("Allows Friend to crawl along the tops of poles like Noir Catto does. Noir Catto not required to work. Thank you Noir for giving this to me!") };
        var FriendPoleCrawlLabel = new OpLabel(new(50, 600 - 168), Vector2.zero, Translate("Pole Crawl"));
        
        var FriendUnNerfBox = new OpCheckBox(FriendUnNerf, new Vector2(50, 600 - 200)) { description = Translate("Changes various Friend stats to be as they were earlier in the mod's development") };
        var FriendUnNerfLabel = new OpLabel(new(50, 600 - 218), Vector2.zero, Translate("Legacy Movement"));

        var FriendBackspearBox = new OpCheckBox(FriendBackspear, new Vector2(50, 600 - 250)) { description = Translate("Allows Friend to use a backspear like they could earlier in the mod's development") };
        var FriendBackspearLabel = new OpLabel(new(50, 600 - 268), Vector2.zero, Translate("Backspear Enable"));

        var FriendRepLockBox = new OpCheckBox(FriendRepLock, new Vector2(150, 600 - 100)) { description = Translate("Stops changes to lizard reputation happening on cycle 0 for Friend. Lizard reputation may break if disabled") };
        var FriendRepLockLabel = new OpLabel(new(150, 600 - 118), Vector2.zero, Translate("Cycle 0 Rep Lock"));


        var FriendLabel = new OpLabel(new Vector2(295, 311), Vector2.zero, Translate("Friend"), FLabelAlignment.Center, true);
        FSprite line = new FSprite("pixel");
        line.color = labelMod.color;
        line.scaleX = 600;
        line.scaleY = 2;
        line.SetPosition(new Vector2(300,305));
        charCont.container.AddChild(line);
        var PoacherLabel = new OpLabel(new Vector2(295, 282), Vector2.zero, Translate("Poacher"), FLabelAlignment.Center, true);

        var PoacherPupActsBox = new OpCheckBox(PoacherPupActs, new Vector2(50, 600 - 350)) { description = Translate("Allows Poacher to behave like a pup. Kind of subtle.") };
        var PoacherPupActsLabel = new OpLabel(new(50, 600 - 368), Vector2.zero, Translate("Pup Behaviors"));

        var PoacherBackspearBox = new OpCheckBox(PoacherBackspear, new Vector2(50, 600 - 400)) { description = Translate("Allows Poacher to use a backspear as was planned early in their development") };
        var PoacherBackspearLabel = new OpLabel(new(50, 600 - 418), Vector2.zero, Translate("Backspear Enable"));

        var PoacherFreezeBox = new OpCheckBox(PoacherFreezeFaster, new Vector2(50, 600 - 450)) { description = Translate("Makes Poacher get cold as fast as they did in earlier versions of the mod") };
        var PoacherFreezeLabel = new OpLabel(new(50, 600 - 468), Vector2.zero, Translate("Legacy Hypothermia"));

        var PoacherFoodBox = new OpCheckBox(PoacherFoodParkour, new Vector2(50, 600 - 500)) { description = Translate("Allows some food items to affect Poacher's movement") };
        var PoacherFoodLabel = new OpLabel(new(50, 600 - 518), Vector2.zero, Translate("Heavy Foods"));
        #endregion
        // Experimental
        #region Experimental
        LizRideBox = new OpCheckBox(LizRide, new Vector2(50, 600 - 100)) { description = Translate("Allows mother lizards to be ridden if lizard rep is high enough") };
        var LizRideLabel = new OpLabel(new(50, 600 - 118), Vector2.zero, Translate("Lizard Riding"));

        LizRideAllBox = new OpCheckBox(LizRideAll, new Vector2(50, 600 - 150)) { description = Translate("Allows all lizards to be ridden if tamed (excluding Young and Mother lizards)") };
        var LizRideAllLabel = new OpLabel(new(50, 600 - 168), Vector2.zero, Translate("Universal Lizard Riding"));

        LizRepMeterBox = new OpCheckBox(LizRepMeter, new Vector2(50, 600 - 200)) { description = Translate("Displays lizard reputation in the current region for Solace slugcats") };
        var LizRepLabel = new OpLabel(new(50, 600 - 218), Vector2.zero, Translate("Lizard Rep Meter"));

        LizRepMeterAllBox = new OpCheckBox(LizRepMeterForAll, new Vector2(50, 600 - 250)) { description = Translate("Displays lizard reputation in the current region for all slugcats") };
        var LizRepAllLabel = new OpLabel(new(50, 600 - 268), Vector2.zero, Translate("Universal Lizard Meter"));
        #endregion
        // Memorial
        #region Memorial
        #endregion

        Tabs[0].AddItems( // General
            labelMod,
            labelVersion,

            FamineLabel,
            RepLabel,
            NoFamineBox,
            NoFamineLabel,
            AllFamineBox,
            AllFamineLabel,
            ExpeditionFamineBox,
            ExpeditionFamineLabel,
            LocalLizRepBox,
            LocalLizRepLabel,
            LocalLizRepAllBox,
            LocalLizRepAllLabel,
            SolaceTimerBox,
            SolaceTimerLabel
            );
        Tabs[1].AddItems( // Characters
            labelMod1,
            labelVersion1,

            FriendLabel,
            FriendAutoCrouchBox,
            FriendAutoCrouchLabel,
            FriendPoleCrawlBox,
            FriendPoleCrawlLabel,
            FriendUnNerfBox,
            FriendUnNerfLabel,
            FriendBackspearBox,
            FriendBackspearLabel,
            FriendRepLockBox,
            FriendRepLockLabel,

            PoacherLabel,
            PoacherPupActsBox,
            PoacherPupActsLabel,
            PoacherBackspearBox,
            PoacherBackspearLabel,
            PoacherFreezeBox,
            PoacherFreezeLabel,
            PoacherFoodBox,
            PoacherFoodLabel
            );
        Tabs[2].AddItems( // Experimental
            labelMod2,
            labelVersion2,

            LizRideBox,
            LizRideLabel,
            LizRideAllBox,
            LizRideAllLabel,
            LizRepMeterBox,
            LizRepLabel,
            LizRepMeterAllBox,
            LizRepAllLabel
            );
        Tabs[3].AddItems( // Memorial
            );
    }
}
