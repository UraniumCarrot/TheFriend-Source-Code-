using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using TheFriend.CharacterThings;
using TheFriend.CharacterThings.BelieverThings;
using TheFriend.CharacterThings.DelugeThings;
using TheFriend.FriendThings;
using UnityEngine;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;
using JollyColorMode = Options.JollyColorMode;
using TheFriend.PoacherThings;

namespace TheFriend.SlugcatThings;

public class SlugcatGraphics
{
    public static void Apply()
    {
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.GraphicsModule.HypothermiaColorBlend += GraphicsModule_HypothermiaColorBlend;
    }

    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;
    public static readonly SlugcatStats.Name DelugeName = Plugin.DelugeName;
    public static readonly SlugcatStats.Name BelieverName = Plugin.BelieverName;

    public static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    { // Spear pointing while riding a lizard
        orig(self, actuallyViewed, eu);
        if (self == null) return;
        try
        {
            if (self.GetGeneral().dragonSteed != null && self.GetGeneral().isRidingLizard)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i]?.grabbed != null && self.grasps[i]?.grabbed is Weapon)
                    {
                        float rotation = (i == 1) ? self.GetGeneral().pointDir1 + 90 : self.GetGeneral().pointDir0 + 90f;
                        Vector2 vec = Custom.DegToVec(rotation);
                        (self.grasps[i]?.grabbed as Weapon).setRotation = vec; //new Vector2(self.input[0].x*10, self.input[0].y*10);
                        (self.grasps[i]?.grabbed as Weapon).rotationSpeed = 0f;
                    }
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
    }
    public static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    { // Friend cosmetic movement
        orig(self);
        if (self.player.TryGetFriend(out var friend))
            FriendGraphics.FriendGraphicsUpdate(self);
    }
    
    public static Color GraphicsModule_HypothermiaColorBlend(On.GraphicsModule.orig_HypothermiaColorBlend orig, GraphicsModule self, Color oldCol)
    { // Poacher hypothermia color fix
        if (self.owner is Player player)
        {
            if (player.TryGetPoacher(out var poacher))
                return PoacherGraphics.PoacherHypothermiaColor(player, oldCol);
        }
        return orig(self, oldCol);
    }
    
    public static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    { // Implement CustomTail
        orig(self, ow);
        if (self.player.TryGetFriend(out var friend))
            FriendGraphics.FriendTailCtor(self);
    }
    
    public static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    { // Detail Sprites init
        orig(self, sLeaser, rCam);
        if (self.player.TryGetPoacher(out var poacher))
            PoacherGraphics.PoacherSpritesInit(self, sLeaser, rCam);
        
        else if (self.player.TryGetBeliever(out var believer))
            BelieverGraphics.BelieverSpritesInit(self, sLeaser, rCam);
        
    }
    public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.player.TryGetPoacher(out var poacher))
            PoacherGraphics.PoacherPalette(self, sLeaser, rCam, palette);
        
        else if (self.player.TryGetBeliever(out var believer))
            BelieverGraphics.BelieverPalette(self,sLeaser,rCam,palette);
        
    }

    // Fix layering and force to render
    public static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (self.player.TryGetPoacher(out var poacher))
            PoacherGraphics.PoacherSpritesContainer(self, sLeaser, rCam, newContainer);
        
        else if (self.player.TryGetBeliever(out var believer))
            BelieverGraphics.BelieverSpritesContainer(self, sLeaser, rCam, newContainer);
        
    }
    // Implement FriendHead, Poacher graphics
    public static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        var head = sLeaser.sprites[3];
        var legs = sLeaser.sprites[4];
        var face = sLeaser.sprites[9];
        self.player.GetGeneral().glanceDir = Mathf.RoundToInt(head.scaleX);
        self.player.GetGeneral().pointDir0 = sLeaser.sprites[5].rotation;
        self.player.GetGeneral().pointDir1 = sLeaser.sprites[6].rotation;

        if (self.player.GetGeneral().dragonSteed != null)
        {
            sLeaser.sprites[4].isVisible = false;
        }
        
        if (self.player.TryGetFriend(out var friend))
            FriendGraphics.FriendDrawSprites(self, sLeaser, head, legs);
        
        else if (self.player.TryGetBeliever(out var believer))
            BelieverGraphics.BelieverDrawSprites(self, sLeaser, head, face);
        
        else if (self.player.TryGetPoacher(out var poacher))
        {
            PoacherGraphics.PoacherThinness(self, sLeaser, rCam, timeStacker, camPos);
            PoacherGraphics.PoacherAnimator(self, sLeaser, rCam, timeStacker, camPos);
        }
        
        
        CharacterHooksAndTools.Squint(self,sLeaser);
    }
}
