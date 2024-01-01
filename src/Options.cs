using System.Collections.Generic;
using System.Linq;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using TheFriend.CharacterThings.NoirThings;
using UnityEngine;

namespace TheFriend;

/*public class Options : OptionInterface
{
    public class BetterComboBox : OpComboBox //Thanks Henpemaz
    {
        public BetterComboBox(ConfigurableBase configBase, Vector2 pos, float width, List<ListItem> list) : base(configBase, pos, width, list) { }
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if(this._rectList != null && !_rectList.isHidden)
            {
                for (int j = 0; j < 9; j++)
                {
                    this._rectList.sprites[j].alpha = 1;
                }
            }
        }
    }

    public static Configurable<bool> NoFamine;
    public static Configurable<bool> FaminesForAll;
    public static Configurable<bool> LocalizedLizRep;
    public static Configurable<bool> LocalizedLizRepForAll;
    public static Configurable<bool> ExpeditionFamine;
    public static Configurable<bool> SolaceBlizzTimer;

    // Friend
    public static Configurable<bool> FriendAutoCrouch;
    public static Configurable<bool> PoleCrawl;
    public static Configurable<bool> FriendBackspear;
    public static Configurable<bool> FriendUnNerf;
    public static Configurable<bool> FriendRepLock;

    // Poacher
    public static Configurable<bool> PoacherPupActs;
    public static Configurable<bool> PoacherBackspear;
    public static Configurable<bool> PoacherFreezeFaster;
    public static Configurable<bool> PoacherFoodParkour;
    
    // Noir
    public static Configurable<bool> NoirAltSlashConditions;
    public static Configurable<bool> NoirBuffSlash;
    public static Configurable<bool> NoirAutoSlash;
    public static Configurable<bool> NoirDisableAutoCrouch;
    public static Configurable<NoirCatto.CustomStartMode> NoirUseCustomStart;
    public static Configurable<bool> NoirAttractiveMeow;
    public static Configurable<bool> NoirHideEars;
    public static Configurable<KeyCode> NoirMeowKey;

    public static Configurable<bool> LizRideAll;
    public static Configurable<bool> LizRepMeterForAll;

    // Achievements //todo: Use SaveThings.SolaceCustom
    public static Configurable<bool> SolaceFriendOEAchievement; // Live Well
    public static Configurable<bool> SolacePoacherOEAchievement; // A New Beginning
    public static Configurable<bool> SolaceFriendBadAscensionAchievement; // Betrayal
    public static Configurable<bool> SolaceFriendGoodAscensionAchievement; // Allegiance
    public static Configurable<bool> SolacePoacherBadAscensionAchievement; // Trapped
    public static Configurable<bool> SolacePoacherGoodAscensionAchievement; // Forgiveness
    public static Configurable<bool> SolacePebblesAchievement; // Alone No More
    public static Configurable<bool> SolacePebblesStolenEnlightenmentAchievement; // Hollow Mind

    public Options()
    {
        #region friend
        FriendAutoCrouch = config.Bind("FriendAutoCrouch", true, new ConfigAcceptableList<bool>(true, false));
        PoleCrawl = config.Bind("PoleCrawl", true, new ConfigAcceptableList<bool>(true, false));
        FriendUnNerf = config.Bind("FriendUnNerf", false, new ConfigAcceptableList<bool>(true, false));
        FriendBackspear = config.Bind("FriendBackspear", false, new ConfigAcceptableList<bool>(true, false));
        FriendRepLock = config.Bind("FriendRepLock", true, new ConfigAcceptableList<bool>(true, false));
        #endregion
        #region poacher
        PoacherPupActs = config.Bind("PoacherPupActs", true, new ConfigAcceptableList<bool>(true, false));
        PoacherBackspear = config.Bind("PoacherBackspear", false, new ConfigAcceptableList<bool>(true, false));
        PoacherFreezeFaster = config.Bind("PoacherFreezeFaster", false, new ConfigAcceptableList<bool>(true, false));
        PoacherFoodParkour = config.Bind("PoacherFoodParkour", true, new ConfigAcceptableList<bool>(true, false));
        #endregion
        #region noir
        NoirAltSlashConditions = config.Bind(nameof(NoirAltSlashConditions), false);
        NoirBuffSlash = config.Bind(nameof(NoirBuffSlash), false);
        NoirAutoSlash = config.Bind(nameof(NoirAutoSlash), false);
        NoirDisableAutoCrouch = config.Bind(nameof(NoirDisableAutoCrouch), false);
        NoirUseCustomStart = config.Bind(nameof(NoirUseCustomStart), NoirCatto.CustomStartMode.Story);
        NoirAttractiveMeow = config.Bind(nameof(NoirAttractiveMeow), true);
        NoirHideEars = config.Bind(nameof(NoirHideEars), false);
        NoirMeowKey = config.Bind(nameof(NoirMeowKey), KeyCode.LeftAlt);
        #endregion

        ExpeditionFamine = config.Bind("ExpeditionFamine", false, new ConfigAcceptableList<bool>(true, false));
        NoFamine = config.Bind("NoFamine", false, new ConfigAcceptableList<bool>(true, false));
        FaminesForAll = config.Bind("FaminesForAll", false, new ConfigAcceptableList<bool>(true, false));
        LocalizedLizRep = config.Bind("LocalizedLizRep", true, new ConfigAcceptableList<bool>(true, false));
        LocalizedLizRepForAll = config.Bind("LocalizedLizRepForAll", false, new ConfigAcceptableList<bool>(true, false));
        SolaceBlizzTimer = config.Bind("SolaceBlizzTimer", false, new ConfigAcceptableList<bool>(true, false));
        LizRideAll = config.Bind("LizRideAll", false, new ConfigAcceptableList<bool>(true, false));
        LizRepMeterForAll = config.Bind("LizRepMeterForAll", false, new ConfigAcceptableList<bool>(true, false));

        #region unimplemented
        SolaceFriendOEAchievement = config.Bind("SolaceFriendOEAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherOEAchievement = config.Bind("SolacePoacherOEAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolaceFriendBadAscensionAchievement = config.Bind("SolaceFriendBadAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolaceFriendGoodAscensionAchievement = config.Bind("SolaceFriendGoodAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherBadAscensionAchievement = config.Bind("SolacePoacherBadAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherGoodAscensionAchievement = config.Bind("SolacePoacherGoodAscensionAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePebblesAchievement = config.Bind("SolacePebblesAchievement", false, new ConfigAcceptableList<bool>(true, false));
        SolacePebblesStolenEnlightenmentAchievement = config.Bind("SolacePebblesStolenEnlightenmentAchievement", false, new ConfigAcceptableList<bool>(true, false));
        #endregion
    }

    // Greyout-able boxes and labels
    static OpCheckBox NoFamineBox;
    static OpCheckBox AllFamineBox;
    static OpCheckBox ExpeditionFamineBox;
    static OpCheckBox LocalLizRepBox;
    static OpCheckBox LocalLizRepAllBox;
    // Dynamic stuff
    static OpCheckBox NoirSlashConditionsCheckBox;
    static OpLabel NoirSlashConditionsLabel;

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

        //LocalLizRepAllBox.greyedOut = LocalLizRepBox.value == "false";
        // Noir
        NoirSlashConditionsLabel.text = "Slash conditions: " + (NoirSlashConditionsCheckBox.GetValueBool() ?
                "Alternative - Main hand must be empty" :
                "Default - Empty hands, or no directional input while holding an object");
    }
    public override void Initialize()
    {
        var opTab0 = new OpTab(this, "General");
        var opTab1 = new OpTab(this, "Friend");
        var opTab2 = new OpTab(this, "Poacher");
        var opTabNoir = new OpTab(this, "Noir"); //TODO: Merge into Solace properly
        OpContainer GenSprites = new OpContainer(Vector2.zero);
        OpContainer FriendSprites = new OpContainer(Vector2.zero);
        OpContainer PoacherSprites = new OpContainer(Vector2.zero);
        OpContainer NoirSprites = new OpContainer(Vector2.zero);

        float labelX = 20;
        float labelY = 570;
        
        base.Initialize();
        Tabs = new[] { opTab0, opTab1, opTab2, opTabNoir };
        
        float row1 = 475; // Row Y values
        float row2 = 375;
        float row3 = 275;
        float row4 = 175;
        float row5 = 75;
        
        float textY = -25; // Added to an OpLabel's row value to put it under a checkbox
        
        float defX = 40; // Default X value
        float addX = 80; // Added to defX to make row longer horizontally, increase Here to change spacing between elements, multiply in Ops to "select columns"

        #region General Tab
        
        var genline1 = new FSprite("pixel");
        genline1.color = Menu.MenuColorEffect.rgbMediumGrey;
        genline1.scaleX = 450;
        genline1.scaleY = 2;
        genline1.SetPosition(300,row3+65);
        
        var genline2 = new FSprite("pixel");
        genline2.color = Menu.MenuColorEffect.rgbMediumGrey;
        genline2.scaleX = 450;
        genline2.scaleY = 2;
        genline2.SetPosition(300,row5+65);
        
        // Add sprites
        GenSprites.container.AddChild(genline1);
        GenSprites.container.AddChild(genline2);
        
        // Actual configs

        ExpeditionFamineBox = new OpCheckBox(
            ExpeditionFamine,
            defX + (addX * 0),
            row3 - 100);
        ExpeditionFamineBox.description = Translate("Forces famines to appear in Expedition Mode");

        NoFamineBox = new OpCheckBox(
            NoFamine,
            defX + (addX * 0),
            row3);
        NoFamineBox.description = Translate("Famine mechanics and changes are disabled when checked");

        AllFamineBox = new OpCheckBox(
            FaminesForAll,
            defX + (addX * 0),
            row3 - 50);
        AllFamineBox.description = Translate("Famine mechanics and changes are given to every character");
        
        opTab0.AddItems(new UIelement[]
        {
            GenSprites,
            new OpLabel(labelX, labelY, Translate("Rain World: Solace Config - General"), true),
            new OpLabel(labelX, labelY - 20, Translate($"Version {Plugin.MOD_VERSION}")),
            
            new OpLabel(
                defX-20,
                row1 + 25,  
                "Lizards", true) { alpha = 0.5f },
            
            new OpCheckBox(
                LizRideAll, 
                defX + (addX*0), 
                row1) 
                { description = 
                    Translate("Allows all lizards to be ridden if tamed (excluding Young and Mother lizards)") },
            new OpLabel(
                defX + (addX*0) - 22,
                row1 + textY,
                "Global Rides"),
            
            new OpCheckBox(
                LizRepMeterForAll, 
                defX + (addX*1), 
                row1) 
                { description = 
                    Translate("Displays lizard reputation in the current region for all slugcats") },
            new OpLabel(
                defX + (addX*1) - 32,
                row1 + textY,  
                "Universal Meter"),
            
            //////
            
            new OpLabel(
                defX-20,
                row3 + 25,  
                "Famines", true) { alpha = 0.5f },
            
            NoFamineBox,
            new OpLabel(
                defX + (addX*0) - 36,
                row3 + textY,  
                "Famines Disabled"),
            
            AllFamineBox,
            new OpLabel(
                defX + (addX*0) - 36,
                row3 - 50 + textY,  
                "Universal Famine"),
            
            ExpeditionFamineBox,
            new OpLabel(
                defX + (addX*0) - 39,
                row3 - 100 + textY,  
                "Expedition Famine"),
            
            //////
            
            new OpLabel(
                defX-20,
                row5 + 25,  
                "Other", true) { alpha = 0.5f },
            
            new OpCheckBox(
                SolaceBlizzTimer, 
                defX + (addX*0), 
                row5)
            { description = 
                Translate("Forces the rain timer to be visible for Solace slugcats and Saint") },
            new OpLabel(
                defX + (addX*0) - 29,
                row5 + textY,  
                "Blizzard Timer"),
            
        });
        #endregion
        #region Friend Tab

        // Sprites
        var FriendSymbol = new FSprite("symbolfriend");
        FriendSymbol.scale = 1.45f;
        FriendSymbol.SetPosition(300,300);
        FriendSymbol.alpha = 0.05f;

        var friendline1 = new FSprite("pixel");
        friendline1.color = Menu.MenuColorEffect.rgbMediumGrey;
        friendline1.scaleX = 450;
        friendline1.scaleY = 2;
        friendline1.SetPosition(300,row1-35);
        
        var friendline2 = new FSprite("pixel");
        friendline2.color = Menu.MenuColorEffect.rgbMediumGrey;
        friendline2.scaleX = 450;
        friendline2.scaleY = 2;
        friendline2.SetPosition(300,row2-35);
        
        // Add sprites
        FriendSprites.container.AddChild(FriendSymbol);
        FriendSprites.container.AddChild(friendline1);
        FriendSprites.container.AddChild(friendline2);

        // Actual configs
        opTab1.AddItems(new UIelement[]
        {
            FriendSprites,
            new OpLabel(labelX, labelY, Translate("Rain World: Solace Config - Friend"), true),
            new OpLabel(labelX, labelY - 20, Translate($"Version {Plugin.MOD_VERSION}")),

            new OpLabel(
                defX-20,
                row1 + 25,  
                "Movement", true) { alpha = 0.5f },
            
            new OpCheckBox(
                    FriendUnNerf, 
                    defX + (addX*0), 
                    row1) 
                { description = 
                    Translate("Changes various Friend stats to be as they were earlier in the mod's development") },
            new OpLabel(
                defX + (addX*0) - 22.5f,
                row1 + textY,
                Translate("Legacy Stats")),
            
            new OpCheckBox(
                    FriendAutoCrouch, 
                    defX + (addX*1), 
                    row1) 
                { description = 
                    Translate("Makes Friend crouch automatically after a standing jump") },
            new OpLabel(
                defX + (addX*1) - 22.5f,
                row1 + textY,
                Translate("Auto Crouch")),
            
            new OpCheckBox(
                    PoleCrawl, 
                    defX + (addX*2), 
                    row1) 
                { description = 
                    Translate("Allows Friend to crawl along the tops of poles like Noir does") },
            new OpLabel(
                defX + (addX*2) - 16f,
                row1 + textY,
                Translate("Pole Crawl")),
            
            //////
            
            new OpLabel(defX-20,row2 + 25,  
            "Combat", true) { alpha = 0.5f },
            
            new OpCheckBox(
                    FriendBackspear,
                    defX+(addX*0), 
                    row2)
                { description = 
                    Translate("Allows Friend to use a backspear like they could earlier in the mod's development") },
            new OpLabel(
                defX + (addX*0) - 35f,
                row2 + textY,
                Translate("Backspear Enable")),
            
            //////
            
            new OpLabel(defX-20,row3 + 25,  
                "Other", true) { alpha = 0.5f },
            
            new OpCheckBox(
                    FriendRepLock,
                    defX+(addX*0),
                    row3)
                { description = 
                    Translate("Stops changes to lizard reputation happening on cycle 0 for Friend. Lizard reputation may break if disabled") },
            new OpLabel(
                defX + (addX*0) - 14f,
                row3 + textY,
                Translate("Rep Lock")),

        });
        #endregion
        #region Poacher Tab
        
        // Sprites
        var PoacherSymbol = new FSprite("symbolpoacher");
        PoacherSymbol.scale = 1.315f;
        PoacherSymbol.SetPosition(300,300);
        PoacherSymbol.alpha = 0.05f;
        
        var poacherline1 = new FSprite("pixel");
        poacherline1.color = Menu.MenuColorEffect.rgbMediumGrey;
        poacherline1.scaleX = 450;
        poacherline1.scaleY = 2;
        poacherline1.SetPosition(300,row1-35);
        
        var poacherline2 = new FSprite("pixel");
        poacherline2.color = Menu.MenuColorEffect.rgbMediumGrey;
        poacherline2.scaleX = 450;
        poacherline2.scaleY = 2;
        poacherline2.SetPosition(300,row2-35);
        
        // Add sprites
        PoacherSprites.container.AddChild(PoacherSymbol);
        PoacherSprites.container.AddChild(poacherline1);
        PoacherSprites.container.AddChild(poacherline2);
        
        // Actual config
        opTab2.AddItems(new UIelement[]
        {
            PoacherSprites,
            new OpLabel(labelX, labelY, Translate("Rain World: Solace Config - Poacher"), true),
            new OpLabel(labelX, labelY - 20, Translate($"Version {Plugin.MOD_VERSION}")),
            
            new OpLabel(
                defX-20,
                row1 + 25,  
                "Movement", true) { alpha = 0.5f },
            
            new OpCheckBox(
                PoacherFoodParkour, 
                defX + (addX*0), 
                row1)
                { description = 
                    Translate("Allows some food items to affect Poacher's movement when held") },
            new OpLabel(
                defX + (addX*0) - 23f,
                row1 + textY,
                Translate("Heavy Foods")),
            
            //////
            
            new OpLabel(defX-20,row2 + 25,  
                "Combat", true) { alpha = 0.5f },
            
            new OpCheckBox(
                PoacherBackspear, 
                defX + (addX*0), 
                row2)
                { description = 
                    Translate("Allows Poacher to use a backspear as was planned early in their development") },
            new OpLabel(
                defX + (addX*0) - 35f,
                row2 + textY,
                Translate("Backspear Enable")),
            
            //////
            
            new OpLabel(defX-20,row3 + 25,  
                "Other", true) { alpha = 0.5f },
            
            new OpCheckBox(
                PoacherPupActs, 
                defX + (addX*0), 
                row3)
                { description = 
                    Translate("Allows Poacher to behave like a pup. Kind of subtle.") },
            new OpLabel(
                defX + (addX*0) - 25f,
                row3 + textY,
                Translate("Pup Behavior")),
            
            new OpCheckBox(
                PoacherFreezeFaster, 
                defX + (addX*1), 
                row3)
                { description = 
                    Translate("Makes Poacher get cold as fast as they did in earlier versions of the mod") },
            new OpLabel(
                defX + (addX*1) - 20f,
                row3 + textY,
                Translate("Legacy Cold")),

        });
        #endregion
        #region Noir Tab
        
        // Sprites
        var NoirSymbol = new FSprite("symbolnoir");
        NoirSymbol.scale = 1.4f;
        NoirSymbol.SetPosition(300,300);
        NoirSymbol.alpha = 0.05f;
        
        var noirline1 = new FSprite("pixel");
        noirline1.color = Menu.MenuColorEffect.rgbMediumGrey;
        noirline1.scaleX = 450;
        noirline1.scaleY = 2;
        noirline1.SetPosition(300,row1-35);
        
        var noirline2 = new FSprite("pixel");
        noirline2.color = Menu.MenuColorEffect.rgbMediumGrey;
        noirline2.scaleX = 450;
        noirline2.scaleY = 2;
        noirline2.SetPosition(300,row2-35);
        
        NoirSprites.container.AddChild(NoirSymbol);
        //NoirSprites.container.AddChild(noirline1);
        //NoirSprites.container.AddChild(noirline2);
        
        opTabNoir.AddItems(
            NoirSprites
            );
        #endregion

        NoirSlashConditionsCheckBox = new OpCheckBox(NoirAltSlashConditions, 10f, 520f);
        NoirSlashConditionsLabel = new OpLabel(40f, 520f, "Slash conditions: ") { verticalAlignment = OpLabel.LabelVAlignment.Center };
        var offset = 105f; //yes I'm lazy
        opTabNoir.AddItems(new UIelement[]
        {
            new OpLabel(10f, 550f, "Main", true),
            NoirSlashConditionsCheckBox,
            NoirSlashConditionsLabel,

            new OpCheckBox(NoirBuffSlash, 10f, 490f),
            new OpLabel(40f, 490f, "Buff Noir slash (stronger stun and damage)") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirAutoSlash, 10f, 460f),
            new OpLabel(40f, 460f, "Auto-repeat slash when holding throw") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirDisableAutoCrouch, 10f, 430f),
            new OpLabel(40f, 430f, "Disable Noir tripping") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            // new BetterComboBox(NoirUseCustomStart, new Vector2(10f, 400f), 200f, OpResourceSelector.GetEnumNames(null, typeof(NoirCatto.CustomStartMode)).ToList()),
            // new OpLabel(240f, 400f, "Custom Start (disable if Story Mode fails to load)") { verticalAlignment = OpLabel.LabelVAlignment.Center },

            new OpLabel(10f, 450f - offset, "Fun and Extras", true) { color = new Color(0.65f, 0.85f, 1f) },
            new OpKeyBinder(NoirMeowKey, new Vector2(10f, 420f - offset), new Vector2(150f, 30f), true, OpKeyBinder.BindController.AnyController),
            new OpLabel(166f, 420f - offset, "Meow!") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirAttractiveMeow, 10f, 390f - offset),
            new OpLabel(40f, 390f - offset, "Creatures react to Meows") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirHideEars, 10f, 360f - offset),
            new OpLabel(40f, 360f - offset, "Hide ears (eg. For use with DMS)") { verticalAlignment = OpLabel.LabelVAlignment.Center },

            //Added last due to overlap
            new BetterComboBox(NoirUseCustomStart, new Vector2(10f, 400f), 200f, OpResourceSelector.GetEnumNames(null, typeof(NoirCatto.CustomStartMode)).ToList()),
            new OpLabel(220f, 400f, "Custom Start (disable if Story Mode fails to load)") { verticalAlignment = OpLabel.LabelVAlignment.Center },
        });
    }
}*/
