using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
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
        {
            FriendGraphics.FriendGraphicsUpdate(self);
        }
        else if (self.player.TryGetDeluge(out var deluge))
        {
            self.player.GetDeluge().tailtip = Vector2.Lerp(self.tail[self.tail.Length - 2].pos, self.tail[self.tail.Length - 1].pos, 0.5f);
            self.player.GetDeluge().tailtip2 = Vector2.Lerp(self.tail[self.tail.Length - 2].lastPos, self.tail[self.tail.Length - 1].lastPos, 0.5f);
        }
    }
    
    public static Color GraphicsModule_HypothermiaColorBlend(On.GraphicsModule.orig_HypothermiaColorBlend orig, GraphicsModule self, Color oldCol)
    { // Poacher hypothermia color fix
        if (self.owner is Player player && player.TryGetPoacher(out var poacher))
        {
            //Color b = new Color(0f, 0f, 0f, 0f);
            float hypothermia = (self.owner.abstractPhysicalObject as AbstractCreature).Hypothermia;
            Color b = !(hypothermia < 1f) ? Color.Lerp(new Color(0.8f, 0.8f, 1f), new Color(0.15f, 0.15f, 0.3f), hypothermia - 1f) : Color.Lerp(oldCol, new Color(0.8f, 0.8f, 1f), hypothermia);
            return Color.Lerp(oldCol, b, 0.92f);
        }
        return orig(self, oldCol);
    }
    
    public static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    { // Implement CustomTail
        orig(self, ow);
        if (self.player.TryGetFriend(out var friend))
        {
            if (self.RenderAsPup)
            {
                self.tail[0] = new TailSegment(self, 8f, 2f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 6f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 4f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 2f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
            }
            else
            {
                self.tail[0] = new TailSegment(self, 9f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 7f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 4f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 1f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
            }
            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();
        }
    }
    
    public static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    { // Detail Sprites init
        orig(self, sLeaser, rCam);
        if (self.player.TryGetPoacher(out var poacher))
        {
            Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + 3);
            self.player.GetGeneral().customSprite1 = sLeaser.sprites.Length - 3;
            self.player.GetGeneral().customSprite2 = sLeaser.sprites.Length - 2;
            self.player.GetGeneral().customSprite3 = sLeaser.sprites.Length - 1;

            // Set default sprites
            sLeaser.sprites[self.player.GetGeneral().customSprite1] = new FSprite("dragonskull2A0");
            sLeaser.sprites[self.player.GetGeneral().customSprite2] = new FSprite("dragonskull2A11");
            sLeaser.sprites[self.player.GetGeneral().customSprite3] = new FSprite("dragonskull3A7");
            self.AddToContainer(sLeaser, rCam, null);
        }
        else if (self.player.TryGetBeliever(out var believer))
        {
            Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
            self.player.GetGeneral().customSprite1 = sLeaser.sprites.Length - 1;
            sLeaser.sprites[self.player.GetGeneral().customSprite1] = new FSprite("ForeheadSpotsA0");
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.player.TryGetPoacher(out var poacher))
        {
            PoacherGraphics.PoacherPalette(self, sLeaser, rCam, palette);
        }
        else if (self.player.TryGetBeliever(out var believer))
        {
            BelieverGraphics.BelieverPalette(self,sLeaser,rCam,palette);
        }
    }

    // Fix layering and force to render
    public static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (self.player.TryGetPoacher(out var poacher))
        {
            if (self.player.GetGeneral().customSprite1 < sLeaser.sprites.Length)
            {
                if (newContainer == null) newContainer = rCam.ReturnFContainer("Midground");
                newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite1]);
                newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite2]);
                newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite3]);
                sLeaser.sprites[self.player.GetGeneral().customSprite2].MoveBehindOtherNode(sLeaser.sprites[0]);
                sLeaser.sprites[self.player.GetGeneral().customSprite3].MoveInFrontOfOtherNode(sLeaser.sprites[self.player.GetGeneral().customSprite1]);
            }
        }
        else if (self.player.TryGetBeliever(out var believer))
        {
            if (self.player.GetGeneral().customSprite1 < sLeaser.sprites.Length)
            {
                if (newContainer == null) newContainer = rCam.ReturnFContainer("Midground");
                newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite1]);
                sLeaser.sprites[self.player.GetGeneral().customSprite1].MoveBehindOtherNode(sLeaser.sprites[9]);
            }
        }
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
        {
            if (!self.RenderAsPup)
            {
                if (!head.element.name.Contains("Friend") && 
                    head.element.name.StartsWith("HeadA")) 
                    head.SetElementByName("Friend" + head.element.name);
                if (!legs.element.name.Contains("Friend") && 
                    legs.element.name.StartsWith("LegsA")) 
                    legs.SetElementByName("Friend" + legs.element.name);
            }
        }
        else if (self.player.TryGetBeliever(out var believer))
        {
            if (!self.RenderAsPup)
            {
                if (!head.element.name.Contains("HeadB") && 
                    head.element.name.StartsWith("HeadA")) 
                    head.SetElementByName("HeadB" + (head.element.name.Remove(0,5)));
            }
            if (face.element.name.StartsWith("Face")) 
                sLeaser.sprites[self.player.GetGeneral().customSprite1].SetElementByName("ForeheadSpots" + (face.element.name.Remove(0,4)));
            sLeaser.sprites[self.player.GetGeneral().customSprite1].SetPosition(face.GetPosition());
            sLeaser.sprites[self.player.GetGeneral().customSprite1].scaleX = face.scaleX;
        }
        else if (self.player.TryGetPoacher(out var poacher))
        {
            PoacherGraphics.PoacherThinness(self, sLeaser, rCam, timeStacker, camPos);
            PoacherGraphics.PoacherAnimator(self, sLeaser, rCam, timeStacker, camPos);
        }
        else if (self.player.TryGetDeluge(out var deluge))
        {
            DelugeGraphics.DelugeGraphicsUpdate(self, sLeaser);
        }
    }

    public enum colormode
    {
        set,
        mult,
        add
    }
    public static Color ColorMaker(
        float hue, float sat, float val, 
        colormode hueMode, colormode satMode, colormode valMode, 
        Color origCol = new Color(), Vector3 origHSL = new Vector3())
    {
        // Negative floats can be used to preserve the original value.  
        Vector3 color = Custom.ColorToVec3(Color.black);
        if (origCol != Color.black) color = Custom.RGB2HSL(origCol);
        if (origHSL != Vector3.zero) color = origHSL;
        float newhue = color.x;
        float newsat = color.y;
        float newval = color.z;
        
        color.x = hueMode switch
        {
            colormode.set => newhue = (hue < 0) ? color.x : hue,
            colormode.add => newhue += (hue < 0) ? 0 : hue,
            colormode.mult => newhue *= (hue < 0) ? 1 : hue,
            _ => newhue = 0
        };
        color.y = satMode switch
        {
            colormode.set => newsat = (sat < 0) ? color.y : sat,
            colormode.add => newsat += (sat < 0) ? 0 : sat,
            colormode.mult => newsat *= (sat < 0) ? 1 : sat,
            _ => newsat = 0
        };
        color.z = valMode switch
        {
            colormode.set => newval = (val < 0) ? color.z : val,
            colormode.add => newval += (val < 0) ? 0 : val,
            colormode.mult => newval *= (val < 0) ? 1 : val,
            _ => newval = 0
        };
        
        color.x = newhue;
        color.y = newsat;
        color.z = newval;

        if (color == Vector3.zero) return Color.magenta;
        return Custom.Vec3ToColor(color);
    }
    
}
