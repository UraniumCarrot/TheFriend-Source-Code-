using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using TheFriend.CharacterThings;
using TheFriend.FriendThings;
using TheFriend.Objects.SolaceScarfObject;
using UnityEngine;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;
using JollyColorMode = Options.JollyColorMode;
using TheFriend.PoacherThings;
using On.JollyCoop.JollyMenu;

namespace TheFriend.SlugcatThings;

public class SlugcatGraphics
{
    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;
    
    public static void PlayerGraphics_Update(PlayerGraphics self)
    { // Cosmetic movement
        if (self.player.TryGetFriend(out _))
            FriendGraphics.FriendGraphicsUpdate(self);

        if (self.player.room.world.rainCycle.RainApproaching < 1f &&
            UnityEngine.Random.value > self.player.room.world.rainCycle.RainApproaching &&
            UnityEngine.Random.value < 1f / 102f &&
            (self.player.room.roomSettings.DangerType == DangerType.FloodAndAerie)) 
            self.LookAtRain();
    }
    
    public static Color GraphicsModule_HypothermiaColorBlend(On.GraphicsModule.orig_HypothermiaColorBlend orig, GraphicsModule self, Color oldCol)
    { // Poacher hypothermia color fix
        if (self.owner is Player player)
            if (player.TryGetPoacher(out _))
                return PoacherGraphics.PoacherHypothermiaColor(player, oldCol);
        return orig(self, oldCol);
    }
    
    public static void PlayerGraphics_ctor(PlayerGraphics self, PhysicalObject ow)
    { // Implement CustomTail
        if (self.player.TryGetFriend(out _))
            FriendGraphics.FriendTailCtor(self);
    }
    
    public static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    { // Detail Sprites init
        orig(self, sLeaser, rCam);
        if (self.player.TryGetPoacher(out _))
            PoacherGraphics.PoacherSpritesInit(self, sLeaser, rCam);
        
    }
    public static void PlayerGraphics_ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self.player.TryGetPoacher(out _))
            PoacherGraphics.PoacherPalette(self, sLeaser, rCam, palette);
        
    }

    // Fix layering and force to render
    public static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (self.player.TryGetPoacher(out _))
            PoacherGraphics.PoacherSpritesContainer(self, sLeaser, rCam, newContainer);
        
    }
    // Implement FriendHead, Poacher graphics
    public static void PlayerGraphics_DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var head = sLeaser.sprites[3];
        var legs = sLeaser.sprites[4];
        var face = sLeaser.sprites[9];
        self.player.GetGeneral().glanceDir = Mathf.RoundToInt(head.scaleX);
        self.player.GetGeneral().pointDir0 = sLeaser.sprites[5].rotation;
        self.player.GetGeneral().pointDir1 = sLeaser.sprites[6].rotation;

        if (self.player.GetGeneral().dragonSteed != null)
        {
            sLeaser.sprites[4].isVisible = false;
            sLeaser.sprites[0].rotation = 0;
        }
        
        if (self.player.TryGetFriend(out _))
            FriendGraphics.FriendDrawSprites(self, head, legs);
        
        else if (self.player.TryGetPoacher(out _))
        {
            PoacherGraphics.PoacherThinness(self, sLeaser, rCam, timeStacker, camPos);
            PoacherGraphics.PoacherAnimator(self, sLeaser, rCam, timeStacker, camPos);
        }
        self.player.GetGeneral().scarfPos = Vector2.Lerp(sLeaser.sprites[0].GetPosition(),sLeaser.sprites[1].GetPosition(),0.4f);
        self.player.GetGeneral().scarfRotation = head.rotation;
        self.player.GetGeneral().head = sLeaser.sprites[3];
        if (self.player.abstractCreature.stuckObjects.Exists(x => x.A.realizedObject is SolaceScarf || x.B.realizedObject is SolaceScarf))
        {
            sLeaser.sprites[5].MoveBehindOtherNode(sLeaser.sprites[0]);
            sLeaser.sprites[6].MoveBehindOtherNode(sLeaser.sprites[0]);
        }
        self.Squint(sLeaser);
    }
    #region jolly coop character previews
    // used for third layer (after body, face) for characters that have extras
    internal static bool HasUniqueSprite(SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, JollyCoop.JollyMenu.SymbolButtonTogglePupButton self)
    {
        if (self.symbol.fileName.Contains("on"))
            return orig(self);
        
        return self.symbolNameOff.Contains("noir") || 
               self.symbolNameOff.Contains("poacher") || 
               orig(self);
    }

    // inject file names for our characters
    internal static string GetPupButtonOffName(JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
    {
        var playerClass = self.JollyOptions(self.index).playerClass;
        if (playerClass == null || self.JollyOptions(self.index).isPup) return orig(self);
        if (!self.JollyOptions(self.index).isPup)
        {
            return playerClass.value switch
            {
                "Friend" => "friend_pup_off",
                "NoirCatto" => "noir_pup_off",
                "FriendDragonslayer" => "poacher_pup_off",
                _ => orig(self)
            };
        }
        return orig(self);
    }
    
    // Fix for default colors in jolly select menu
    internal static Color JollyUniqueColorMenu(On.PlayerGraphics.orig_JollyUniqueColorMenu orig, SlugcatStats.Name slugname, SlugcatStats.Name reference, int playernumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != JollyColorMode.DEFAULT)
            return orig(slugname, reference, playernumber);
            
        if (slugname == Plugin.NoirName) return CharacterThings.NoirThings.NoirCatto.NoirWhite;
        if (slugname == Plugin.DragonName) return Custom.hexToColor("735a7f");
        return orig(slugname, reference, playernumber);
    }

    // Fix for default colors in jolly select menu
    internal static Color JollyFaceColorMenu(On.PlayerGraphics.orig_JollyFaceColorMenu orig, SlugcatStats.Name slugname, SlugcatStats.Name reference, int playernumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != JollyColorMode.DEFAULT)
            return orig(slugname, reference, playernumber);
            
        if (slugname == Plugin.NoirName) return CharacterThings.NoirThings.NoirCatto.NoirBlueEyesDefault;
        if (slugname == Plugin.DragonName) return Custom.hexToColor("6d3868");
        if (slugname == Plugin.FriendName) return Custom.hexToColor("101010");
        return orig(slugname, reference, playernumber);
    }
    #endregion
}
