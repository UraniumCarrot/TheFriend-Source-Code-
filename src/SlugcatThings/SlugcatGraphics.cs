using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JollyCoop;
using RWCustom;
using HUD;
using System.Threading.Tasks;
using UnityEngine;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;
using JollyColorMode = Options.JollyColorMode;

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

    public static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    { // Spear pointing while riding a lizard
        orig(self, actuallyViewed, eu);
        try
        {
            if (self != null && self.GetPoacher().dragonSteed != null && self.GetPoacher().isRidingLizard)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self?.grasps[i] != null && self?.grasps[i]?.grabbed != null && self?.grasps[i]?.grabbed is Weapon)
                    {
                        float rotation = (i == 1) ? self.GetPoacher().pointDir1 + 90 : self.GetPoacher().pointDir0 + 90f;
                        Vector2 vec = Custom.DegToVec(rotation);
                        (self?.grasps[i]?.grabbed as Weapon).setRotation = vec; //new Vector2(self.input[0].x*10, self.input[0].y*10);
                        (self?.grasps[i]?.grabbed as Weapon).rotationSpeed = 0f;
                    }
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
    }
    public static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    { // Friend cosmetic movement
        orig(self);
        if (self.player.slugcatStats.name == FriendName)
        {
            if ((self.player.bodyMode == bod.Crawl || self.player.standing || self.player.bodyMode == bod.Stand) && !self.player.GetPoacher().isRidingLizard && self.player.bodyMode != bod.Default)
            {
                self.tail[self.tail.Length - 1].vel.y += (self.player.standing || self.player.bodyMode == bod.Stand) ? 0.9f : 1f;
                self.tail[self.tail.Length - 3].vel.y -= (self.player.bodyMode != bod.Crawl && Mathf.Abs(self.player.firstChunk.vel.x) > 3.2f) ? 2.5f : 0f;
                if (self.player.GetPoacher().WantsUp) self.tail[self.tail.Length - 1].vel.y += 0.2f;
            }
            if (self.player.GetPoacher().WantsUp) self.head.vel.y += 1;
        }
    }
    
    public static Color GraphicsModule_HypothermiaColorBlend(On.GraphicsModule.orig_HypothermiaColorBlend orig, GraphicsModule self, Color oldCol)
    { // Poacher hypothermia color fix
        if (self.owner is Player player && player.slugcatStats.name == DragonName)
        {
            Color b = new Color(0f, 0f, 0f, 0f);
            float hypothermia = (self.owner.abstractPhysicalObject as AbstractCreature).Hypothermia;
            b = ((!(hypothermia < 1f)) ? Color.Lerp(new Color(0.8f, 0.8f, 1f), new Color(0.15f, 0.15f, 0.3f), hypothermia - 1f) : Color.Lerp(oldCol, new Color(0.8f, 0.8f, 1f), hypothermia));
            return Color.Lerp(oldCol, b, 0.92f);
        }
        return orig(self, oldCol);
    }
    
    public static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    { // Implement CustomTail
        orig(self, ow);
        if (Plugin.CustomTail.TryGet(self.player, out bool hasCustomTail) && hasCustomTail)
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
    { // Poacher skull init
        orig(self, sLeaser, rCam);
        if (self.player.slugcatStats.name == DragonName)
        {
            Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + 3);
            self.player.GetPoacher().skullpos1 = sLeaser.sprites.Length - 3;
            self.player.GetPoacher().skullpos2 = sLeaser.sprites.Length - 2;
            self.player.GetPoacher().skullpos3 = sLeaser.sprites.Length - 1;

            // Set default sprites
            sLeaser.sprites[self.player.GetPoacher().skullpos1] = new FSprite("dragonskull2A0");
            sLeaser.sprites[self.player.GetPoacher().skullpos2] = new FSprite("dragonskull2A11");
            sLeaser.sprites[self.player.GetPoacher().skullpos3] = new FSprite("dragonskull3A7");
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    static Color blackColor;
    public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.player.slugcatStats.name != DragonName) return;
        if (self.player.room?.game?.IsArenaSession == null) return;
        blackColor = palette.blackColor;
        int playerNumber = !self.player.room.game.IsArenaSession && (self.player.playerState.playerNumber == 0) ? -1 : self.player.playerState.playerNumber;
        LoadDefaultColors(self.player, playerNumber);
        Color color;
        Color color2;
        if (ModManager.CoopAvailable && self.useJollyColor)
        {
            Color j = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
            if (Custom.rainWorld.options.jollyColorMode == JollyColorMode.AUTO)
            {
                color = new Color(j.r - 0.6f, j.b - 0.6f, j.g - 0.6f);
                color2 = new Color(color.b + 0.2f, color.r + 0.2f, color.g + 0.2f);
                self.player.GetPoacher().customColor = color2;
                self.player.GetPoacher().customColor2 = color;
            }
            else if (Custom.rainWorld.options.jollyColorMode == JollyColorMode.CUSTOM)
            {
                color = j;
                bool mono = Custom.RGB2HSL(color).y == 0;
                bool light = Custom.RGB2HSL(color).z > 0.5f;
                color2 = (mono) ? new Color(light ? 0 : 1, light ? 0 : 1, light ? 0 : 1) : new Color(color.b, color.r, color.g);
                self.player.GetPoacher().customColor = color;
                self.player.GetPoacher().customColor2 = color2;
            }
        }
        else if (PlayerGraphics.CustomColorsEnabled())
        {
            color = PlayerGraphics.CustomColorSafety(2);
            color2 = PlayerGraphics.CustomColorSafety(3);
            self.player.GetPoacher().customColor = color;
            self.player.GetPoacher().customColor2 = color2;
        }
    }
    public static void LoadDefaultColors(Player player, int playerNumber)
    {
        if (SlugBaseCharacter.TryGet(player.slugcatStats.name, out SlugBaseCharacter chara) && chara.Features.TryGet(PlayerFeatures.CustomColors, out ColorSlot[] customColor))
        {
            if (customColor.Length > 3 && player.GetPoacher().isPoacher == true)
            {
                player.GetPoacher().customColor = customColor[2].GetColor(playerNumber);
                player.GetPoacher().customColor2 = customColor[3].GetColor(playerNumber);
            }
        }
    }

    // Fix layering and force to render
    public static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (self.player.slugcatStats.name == DragonName)
        {
            if (self.player.GetPoacher().skullpos1 < sLeaser.sprites.Length)
            {
                if (newContainer == null) newContainer = rCam.ReturnFContainer("Midground");
                newContainer.AddChild(sLeaser.sprites[self.player.GetPoacher().skullpos1]);
                newContainer.AddChild(sLeaser.sprites[self.player.GetPoacher().skullpos2]);
                newContainer.AddChild(sLeaser.sprites[self.player.GetPoacher().skullpos3]);
                sLeaser.sprites[self.player.GetPoacher().skullpos2].MoveBehindOtherNode(sLeaser.sprites[0]);
                sLeaser.sprites[self.player.GetPoacher().skullpos3].MoveInFrontOfOtherNode(sLeaser.sprites[self.player.GetPoacher().skullpos1]);
            }
        }
    }
    // Implement FriendHead, Poacher graphics
    public static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        var head = sLeaser.sprites[3];
        var legs = sLeaser.sprites[4];
        self.player.GetPoacher().glanceDir = Mathf.RoundToInt(head.scaleX);
        self.player.GetPoacher().pointDir0 = sLeaser.sprites[5].rotation;
        self.player.GetPoacher().pointDir1 = sLeaser.sprites[6].rotation;


        if (self.player.GetPoacher().dragonSteed != null)
        {
            sLeaser.sprites[4].isVisible = false;
        }
        if (Plugin.FriendHead.TryGet(self.player, out var hasFriendHead) && hasFriendHead)
        {
            if (!self.RenderAsPup)
            {
                if (!head.element.name.Contains("Friend") && head.element.name.StartsWith("HeadA")) head.SetElementByName("Friend" + head.element.name);
                if (!legs.element.name.Contains("Friend") && legs.element.name.StartsWith("LegsA")) legs.SetElementByName("Friend" + legs.element.name);
            }
        }
        if (self.player.slugcatStats.name == DragonName)
        {
            var poacher = self.player.GetPoacher();
            var skullpos1 = poacher.skullpos1;
            var skullpos2 = poacher.skullpos2;
            var skullpos3 = poacher.skullpos3;
            var flicker = poacher.flicker;
            var headToBody = (new Vector2(sLeaser.sprites[1].x, sLeaser.sprites[1].y) - new Vector2(sLeaser.sprites[3].x, sLeaser.sprites[3].y)).normalized;
            var skullPos = new Vector2(sLeaser.sprites[3].x + headToBody.x * 7.5f, sLeaser.sprites[3].y + headToBody.y * 7.5f);
            float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
            Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
            Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
            float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));

            // For thinness
            sLeaser.sprites[0].scaleX = 0.8f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, self.malnourished), 0.05f, num) * num2, 0.15f, self.player.sleepCurlUp);
            sLeaser.sprites[1].scaleX = 0.8f + self.player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * self.malnourished;
            for (int i = 0; i < 2; i++)
            {
                float num9 = 4.5f / ((float)self.hands[i].retractCounter + 1f);
                Vector2 vector10 = Vector2.Lerp(self.hands[i].lastPos, self.hands[i].pos, timeStacker);
                Vector2 vector11 = vector + Custom.RotateAroundOrigo(new Vector2((-1f + 2f * (float)i) * (num9 * 0.6f), -3.5f), Custom.AimFromOneVectorToAnother(vector2, vector));
                sLeaser.sprites[5 + i].element = Futile.atlasManager.GetElementWithName("PlayerArm" + Mathf.RoundToInt(Mathf.Clamp(Vector2.Distance(vector10, vector11) / 2f, 0f, 12f)));
                sLeaser.sprites[5 + i].rotation = Custom.AimFromOneVectorToAnother(vector10, vector11) + 90f;
            }

            // Redone skull animation code
            Color origColor = poacher.customColor;
            Color origColor2 = poacher.customColor2;
            Color origColorDark = Color.Lerp(origColor, blackColor, 0.2f);
            var atlas = Futile.atlasManager;
            string dragon = "dragonskull2A";
            string dragon3 = "dragonskull3A";
            float mainscale = 1.2f;
            float mainX = self.player.flipDirection * mainscale;
            float mainY = mainscale;
            float roll = self.player.rollDirection * mainscale;
            float speed = Mathf.Sign(self.player.firstChunk.vel.x) * mainscale;
            var headname = sLeaser.sprites[3].element.name;
            for (int h = 0; h < 3; h++)
            {
                sLeaser.sprites[skullpos1 + h].rotation = Mathf.Lerp(sLeaser.sprites[3].rotation, sLeaser.sprites[3].rotation * 8.5f, self.player.sleepCurlUp * 0.1f);
                sLeaser.sprites[skullpos1 + h].scaleX = mainX;
                sLeaser.sprites[skullpos1 + h].scaleY = mainY;
                sLeaser.sprites[skullpos1 + h].x = skullPos.x;
                sLeaser.sprites[skullpos1 + h].y = skullPos.y;
            }
            if (flicker > 0)
            {
                poacher.flicker--;
                Color white = Color.Lerp(Color.white, blackColor, 0.3f);
                float blink = UnityEngine.Random.value;
                if (blink > 0.5f) { sLeaser.sprites[skullpos1].color = Color.white; sLeaser.sprites[skullpos2].color = white; sLeaser.sprites[skullpos3].color = white; }
                else { sLeaser.sprites[skullpos1].color = origColor; sLeaser.sprites[skullpos2].color = origColorDark; sLeaser.sprites[skullpos3].color = origColor2; }
            }
            else { sLeaser.sprites[skullpos1].color = origColor; sLeaser.sprites[skullpos2].color = origColorDark; sLeaser.sprites[skullpos3].color = origColor2; }

            //layer2 visibility
            if (sLeaser.sprites[skullpos1].element.name != "dragonskull2A7")
            {
                sLeaser.sprites[skullpos2].isVisible = true;
                if (sLeaser.sprites[skullpos1].element.name == "dragonskull2A4") sLeaser.sprites[skullpos2].element = atlas.GetElementWithName(dragon + "11");
                else if (sLeaser.sprites[skullpos1].element.name == "dragonskull2A5") sLeaser.sprites[skullpos2].element = atlas.GetElementWithName(dragon + "12");
                else sLeaser.sprites[skullpos2].element = atlas.GetElementWithName(dragon + "10");
            }
            else sLeaser.sprites[skullpos2].isVisible = false;

            //layer3 animation match
            var element = sLeaser.sprites[skullpos1].element.name;
            if (element == "dragonskull2A17") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "17"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A16") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "16"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A15") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "15"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A14") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "14"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A9") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "9"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A8") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "8"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A7") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "7"); sLeaser.sprites[skullpos3].isVisible = true; }
            else if (element == "dragonskull2A5") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "5"); sLeaser.sprites[skullpos3].isVisible = true; }
            else sLeaser.sprites[skullpos3].isVisible = false;



            //default frames
            string str = headname.Substring(headname.IndexOf("C"));
            switch (str)
            {
                case "C0" or "C1" or "C2" or "C3": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "0"); break;
                case "C4": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "4"); break;
                case "C5" or "C6": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); break;
                case "C14": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "14"); sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale; break;
                case "C15": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "15"); sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale; break;
                case "C16": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "16"); break;
                case "C17": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "17"); break;
                case "C7" or "C8" or "C9": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7"); sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale; break;
            }
            var a = self.player.animation;
            var b = self.player.bodyMode;
            // Dynamics
            string anim = a.value;
            if (a.value.Any())
            {
                switch (anim)
                {
                    case "Flip": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); sLeaser.sprites[skullpos1].scaleX = self.player.ThrowDirection * mainscale; break;
                    case "Roll": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); sLeaser.sprites[skullpos1].scaleX = roll; break;
                    case "RocketJump": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "0"); break;
                    case "HangUnderVerticalBeam": sLeaser.sprites[skullpos1].scaleX = -mainX; sLeaser.sprites[skullpos2].scaleX = self.player.ThrowDirection * mainscale; break;
                    case "GetUpOnBeam": sLeaser.sprites[skullpos1].scaleX = -speed; break;
                    case "AntlerClimb": sLeaser.sprites[skullpos1].scaleX = speed; break;
                    case "CorridorTurn":
                        sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7");
                        sLeaser.sprites[skullpos1].scaleX = Mathf.Sign(self.player.corridorTurnDir.Value.x) * -mainscale;
                        break;
                }
            }
            switch (self.player.sleepCurlUp)
            {
                case < 0.2f and not 0: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "4"); break;
                case > 0.2f and < 0.9f: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); break;
                case > 0.9f: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7"); break;
            }
            if (b == bod.CorridorClimb && (headname.EndsWith("C9") || headname.EndsWith("C8") || headname.EndsWith("C7") || headname.EndsWith("C6") || headname.EndsWith("C9") || headname.EndsWith("C10") || headname.EndsWith("C5")))
            {
                sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7");
                sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale;
            }
            //layer3 scale match
            sLeaser.sprites[skullpos3].scaleX = sLeaser.sprites[skullpos1].scaleX;
        }
    }
}
