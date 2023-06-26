using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using IL.MoreSlugcats;
using RWCustom;
using MoreSlugcats;
using System.Linq;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using HUD;
using JollyCoop;
using System.Globalization;
using CoralBrain;
using System.Collections.Generic;
using System.IO;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Fisobs.Core;
using On;
using System.Security;
using System.Runtime.InteropServices.WindowsRuntime;
using MonoMod;
using IL;
using TheFriend.WorldChanges;
using TheFriend.Creatures;
using TheFriend.Objects.BoulderObject;
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.BoomMineObject;
using BepInEx.Logging;
using System.Net;
using SlugBase;
using SlugBase.DataTypes;
using ind = Player.AnimationIndex;
using bod = Player.BodyModeIndex;
using JollyColorMode = Options.JollyColorMode;
using Vector2 = UnityEngine.Vector2;
using System.Numerics;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace TheFriend
{
    [BepInPlugin(MOD_ID, "The Friend", "0.2.0.1")]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "thefriend";

        public PlayerFeature<float> SuperJump = PlayerFloat("friend/super_jump");
        public PlayerFeature<float> SuperCrawl = PlayerFloat("friend/super_crawl");
        public PlayerFeature<bool> SuperSlide = PlayerBool("friend/super_slide");
        static readonly PlayerFeature<bool> FriendHead = PlayerBool("friend/friendhead");
        static readonly PlayerFeature<bool> IsFriendChar = PlayerBool("friend/is_the_friend");
        static readonly PlayerFeature<bool> CustomTail = PlayerBool("friend/fancytail");
        static readonly PlayerFeature<bool> MaulEnabled = PlayerBool("friend/maul");

        #region hooks
        // Add hooks
        public static ManualLogSource LogSource { get; private set; }
        public void OnEnable()
        {
            LogSource = Logger;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Misc class hooks
            FriendWorldState.Apply();
            FriendCrawl.Apply();
            DragonCrafts.Apply();
            SLOracleHandler.Apply();
            FamineWorld.Apply();
            SnowSpiderGraphics.Apply();
            PebblesLL.Apply();
            DragonClassFeatures.Apply();
            DragonRepInterface.Apply();

            // Gameplay changes
            On.Player.Update += Player_Update;
            On.Player.Stun += Player_Stun;
            On.Weapon.HitThisObject += Weapon_HitThisObject;
            On.Player.WallJump += Player_WallJump;
            On.Player.Jump += Player_Jump;
            On.Player.ctor += Player_ctor;
            On.Player.Grabability += Player_Grabability;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.Grabbed += Player_Grabbed;
            On.Player.UpdateBodyMode += Player_UpdateBodyMode;
            On.Player.UpdateAnimation += Player_UpdateAnimation;
            On.Player.HeavyCarry += Player_HeavyCarry;
            On.Player.checkInput += Player_checkInput;
            On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;
            On.SlugcatStats.ctor += SlugcatStats_ctor;

            // Misc gameplay changes
            On.DangleFruit.Update += DangleFruit_Update;
            On.LanternMouse.Update += LanternMouse_Update;
            On.MoreSlugcats.DandelionPeach.Update += DandelionPeach_Update;

            // Graphics
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.GraphicsModule.HypothermiaColorBlend += GraphicsModule_HypothermiaColorBlend;

            // Hud changes
            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
            On.HUD.RainMeter.Draw += RainMeter_Draw;
            On.HUD.RainMeter.ctor += RainMeter_ctor;
            On.HUD.RainMeter.Update += RainMeter_Update;

            // Misc IL
            IL.Player.ThrowObject += Player_ThrowObject;
            IL.Player.UpdateAnimation += ilcontext =>
            {
                var cursor = new ILCursor(ilcontext);
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(18.1f)))
                {
                    return;
                }

                cursor.MoveAfterLabels();
                cursor.RemoveRange(2);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((Player player) =>
                {
                    if (player is Player)
                    {
                        return 14f;
                    }
                    else
                    {
                        return 18.1f;
                    }
                });
                cursor.Emit(OpCodes.Stloc, 24);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((Player player) =>
                {
                    if (SuperSlide.TryGet(player, out bool slideup) && slideup)
                    {
                        return 22f;
                    }
                    else
                    {
                        return 28f;
                    }
                });
                cursor.Emit(OpCodes.Stloc, 25);
            };

            On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
            {
                orig(self, newlyDisabledMods);
                for (var i = 0; i < newlyDisabledMods.Length; i++)
                {
                    if (newlyDisabledMods[i].id == "The Friend")
                    {
                        //if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.PebblesLL))
                        //    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.PebblesLL);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MotherLizard))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MotherLizard);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.YoungLizard))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.YoungLizard);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SnowSpider))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.SnowSpider);
                        if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.UnlockLittleCracker))
                            MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.UnlockLittleCracker);
                        //if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.UnlockBoulder))
                        //    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.UnlockBoulder);
                        CreatureTemplateType.UnregisterValues();
                        SandboxUnlockID.UnregisterValues();
                        break;
                    }
                }
            };
            Content.Register(new PebblesLLCritob());
            Content.Register(new MotherLizardCritob());
            Content.Register(new YoungLizardCritob());
            Content.Register(new SnowSpiderCritob());
            Content.Register(new BoulderFisob());
            Content.Register(new LittleCrackerFisob());
            Content.Register(new BoomMineFisob());
        }

        public void LoadResources(RainWorld rainWorld)
        {
            MachineConnector.SetRegisteredOI("thefriend", new Options());
            Futile.atlasManager.LoadAtlas("atlases/friendsprites");
            Futile.atlasManager.LoadAtlas("atlases/friendlegs");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull2");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull3");
        }
        public static void Player_ThrowObject(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<Player.AnimationIndex>("Flip")))
            {
                return;
            }

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(1)))
            {
                return;
            }

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(player =>
            {
                return (IsFriendChar.TryGet(player, out var isFriend) && isFriend && !(player.bodyChunks[1].ContactPoint.y == -1) && !(player.bodyMode == Player.BodyModeIndex.Crawl || player.standing));
            });
            cursor.Emit(OpCodes.Or);
        }
        #endregion

        public static readonly SlugcatStats.Name FriendName = new SlugcatStats.Name("Friend", false); // Makes Friend's campaign more accessible to me
        public static readonly SlugcatStats.Name DragonName = new SlugcatStats.Name("FriendDragonslayer", false); // Makes Poacher's campaign more accessible to me

        #region graphics
        // Friend cosmetic movement
        public void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
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
        // Poacher hypothermia color fix
        public Color GraphicsModule_HypothermiaColorBlend(On.GraphicsModule.orig_HypothermiaColorBlend orig, GraphicsModule self, Color oldCol)
        {
            if (self.owner is Player player && player.slugcatStats.name == DragonName)
            {
                Color b = new Color(0f, 0f, 0f, 0f);
                float hypothermia = (self.owner.abstractPhysicalObject as AbstractCreature).Hypothermia;
                b = ((!(hypothermia < 1f)) ? Color.Lerp(new Color(0.8f, 0.8f, 1f), new Color(0.15f, 0.15f, 0.3f), hypothermia - 1f) : Color.Lerp(oldCol, new Color(0.8f, 0.8f, 1f), hypothermia));
                return Color.Lerp(oldCol, b, 0.92f);
            }
            return orig(self, oldCol);
        }
        // Implement CustomTail
        public void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (CustomTail.TryGet(self.player, out bool hasCustomTail) && hasCustomTail)
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
        // Poacher skull init
        public void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
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
        Color blackColor;
        public void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
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
                    color2 = (mono) ? new Color(light?0:1,light?0:1,light?0:1) : new Color(color.b, color.r, color.g);
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
        public void LoadDefaultColors(Player player, int playerNumber)
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
        public void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
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
        void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) 
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
            if (FriendHead.TryGet(self.player, out var hasFriendHead) && hasFriendHead)
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
                    sLeaser.sprites[skullpos1 + h].rotation = Mathf.Lerp(sLeaser.sprites[3].rotation, sLeaser.sprites[3].rotation*8.5f, self.player.sleepCurlUp*0.1f);
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
                    case > 0.9f: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7");  break;
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

        #endregion
        #region gameplay
        // Makes player ride lizard
        public void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            for (int i = 0; i < 2; i++)
            {
                if (self?.grasps[i]?.grabbed is Lizard liz && liz.GetLiz() != null && liz.GetLiz().IsRideable && !liz.dead && !liz.Stunned && liz?.AI?.LikeOfPlayer(liz?.AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) > 0)
                {
                    if (!liz.GetLiz().boolseat0) 
                    { 
                        self.grasps[i].Release(); 
                        self.GetPoacher().isRidingLizard = true;
                        DragonRiding.DragonRidden(liz, self); 
                        liz.GetLiz().boolseat0 = true;
                        self.GetPoacher().dragonSteed = liz;
                        liz.GetLiz().rider = self;
                    }
                }
            }
            if (self.slugcatStats.name == DragonName)
            {
                if (self.craftingObject && self.GetPoacher().isMakingPoppers && self.grasps.Count(i => i.grabbed is FirecrackerPlant) == 1 && self.swallowAndRegurgitateCounter < 70) self.swallowAndRegurgitateCounter = 70;
            }
        }
        // Lizard mount will not be hit by owner's weapons
        public bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
        {
            if (obj is Lizard liz && liz.GetLiz() != null && liz.GetLiz().IsBeingRidden && self.thrownBy is Player pl && pl.GetPoacher().dragonSteed == liz) return false;
            else return orig(self, obj);
        }
        // Poacher skull flicker
        public void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
        {
            orig(self, grasp);
            if ((grasp.grabber is Lizard || grasp.grabber is Vulture || grasp.grabber is BigSpider || grasp.grabber is DropBug) && self.slugcatStats.name == DragonName)
            {
                self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
                self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.firstChunk, false, 1, 1);
            }
        }
        // Poacher skull flicker
        public void Player_Stun(On.Player.orig_Stun orig, Player self, int st)
        {
            if (self.slugcatStats.name == DragonName && self.stunDamageType == Creature.DamageType.Blunt && !self.Stunned)
            {
                if (self.bodyMode == bod.Crawl) { self.firstChunk.vel.y += 10; self.animation = ind.Flip; }
                self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
                self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.firstChunk, false, 1, 1);
            }
            orig(self, st);
        }
        // Lizard grabability
        public Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is Lizard liz)
            {
                if (LizRide() && liz.Template.type != CreatureTemplateType.YoungLizard)
                {
                    if (liz.GetLiz() != null && liz.GetLiz().IsRideable)
                    {
                        if (liz?.Template?.type != CreatureTemplateType.MotherLizard && liz?.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Attacks && liz?.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats && liz?.AI?.friendTracker?.friend != null && liz?.AI?.friendTracker?.friendRel?.like < 0.5f && !liz.dead && !liz.Stunned) return Player.ObjectGrabability.CantGrab;
                        if ((liz.GetLiz().IsBeingRidden || self.GetPoacher().grabCounter > 0 || liz?.AI?.LikeOfPlayer(liz?.AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) < 0) && !liz.dead && !liz.Stunned) return Player.ObjectGrabability.CantGrab;
                        self.GetPoacher().grabCounter = 15;
                        return Player.ObjectGrabability.OneHand;
                    }
                }
                else if (liz.Template.type == CreatureTemplateType.YoungLizard)
                {
                    for (int i = 0; i < self?.grasps?.Count(); i++) if ((self?.grasps[i]?.grabbed as Creature)?.Template?.type == CreatureTemplateType.YoungLizard) return Player.ObjectGrabability.CantGrab;
                    return Player.ObjectGrabability.OneHand;
                }
                return orig(self, obj);
            }
            if (self.slugcatStats.name == DragonName && self.GetPoacher().IsInIntro && obj is Weapon) return Player.ObjectGrabability.CantGrab;
            return orig(self, obj);
        }
        // Makes Poacher get cold faster, Poacher food preferences, riding lizards, moon mark
        public void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self == null || self.room == null) { Debug.Log("Solace: Player returned null, cancelling PlayerUpdate code"); return; }

            if (self.GetPoacher().JustGotMoonMark && !self.GetPoacher().MoonMarkPassed) 
            {
                Debug.Log("Solace: PlayerUpdate JustGotMoonMark check passed");
                self.Stun(20);
                self.GetPoacher().MarkExhaustion = (int)((1/self.slugcatStats.bodyWeightFac)*200); self.GetPoacher().MoonMarkPassed = true;
            }
            if (self.GetPoacher().MarkExhaustion > 0 && self.GetPoacher().JustGotMoonMark)
            {
                Debug.Log("Solace: PlayerUpdate MarkExhaustion check passed");
                self.GetPoacher().MarkExhaustion--;
                self.exhausted = true;
                self.aerobicLevel = (self.slugcatStats.bodyWeightFac < 0.5f) ? 1.5f : 1.1f;
                (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 0.2f;
            }

            Vector2 pointPos = new Vector2(self.input[0].x*50, self.input[0].y*50) + self.bodyChunks[0].pos;
            var graph = self.graphicsModule as PlayerGraphics;
            var hand = ((pointPos - self.mainBodyChunk.pos).x < 0 || self?.grasps[0]?.grabbed is Spear) ? 0 : 1;
            if (self?.grasps[1]?.grabbed is Spear) hand = 1;
            var nothand = (hand == 1) ? 0 : 1;

            for (int i = 0; i < 2; i++)
            {
                if (self?.grasps[i]?.grabbed is Spear && !(self?.grasps[0]?.grabbed == self?.grasps[1]?.grabbed)) hand = i;
            }

            // Poacher
            if (self.slugcatStats.name == DragonName)
            {
                if (self.input[0].y < 1 || !self.input[0].pckp) self.GetPoacher().isMakingPoppers = false;
                self.Hypothermia += self.HypothermiaGain * (PoacherFreezeFaster() ? 1.2f : 0.2f);
                FamineWorld.PoacherEats(self);
                if (self.dangerGraspTime > 0)
                {
                    self.stun = 0;
                    if (self.input[0].thrw) self.ThrowToGetFree(eu);
                    if (self.input[0].pckp) self.DangerGraspPickup(eu);
                }
            }

            // Dragonriding
            if (self.GetPoacher().grabCounter > 0)
            {
                self.GetPoacher().grabCounter--;
                for (int i = 0; i < 2; i++)
                    if (self.bodyChunks[i].vel.y < -10) self.bodyChunks[i].vel.y = -10;
            }
            if (self.GetPoacher().isRidingLizard && (self.GetPoacher().dragonSteed as Lizard).GetLiz() != null) 
            {
                var liz = self?.GetPoacher()?.dragonSteed as Lizard;
                try 
                {
                    self.standing = true;
                    if (liz.animation != Lizard.Animation.Lounge && 
                        liz.animation != Lizard.Animation.PrepareToLounge &&
                        liz.animation != Lizard.Animation.ShootTongue &&
                        liz.animation != Lizard.Animation.Spit &&
                        liz.animation != Lizard.Animation.HearSound &&
                        liz.animation != Lizard.Animation.PreyReSpotted &&
                        liz.animation != Lizard.Animation.PreySpotted &&
                        liz.animation != Lizard.Animation.ThreatReSpotted &&
                        liz.animation != Lizard.Animation.ThreatSpotted) liz.JawOpen = 0;
                    DragonRiding.DragonRiderSafety(self, self.GetPoacher().dragonSteed, (self.GetPoacher().dragonSteed as Lizard).GetLiz().seat0);
                    if ((self?.input[0].y < 0 && self.input[0].pckp) ||
                        (self?.GetPoacher()?.dragonSteed as Lizard).AI?.LikeOfPlayer((self?.GetPoacher()?.dragonSteed as Lizard).AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) <= 0 ||
                        self.dead ||
                        self.Stunned ||
                        (self?.room != self?.GetPoacher()?.dragonSteed?.room && self.room != null))
                        DragonRiding.DragonRideReset(self.GetPoacher().dragonSteed, self);
                }
                catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.Update LizRide" + e); }

                // Pointing (ONWARDS, STEED!)
                try
                {
                    graph.LookAtPoint(pointPos, 0f);
                    graph.hands[hand].absoluteHuntPos = pointPos;
                    if (self.GetPoacher().dragonSteed != null) graph.hands[nothand].absoluteHuntPos = self.GetPoacher().dragonSteed.firstChunk.pos;
                    graph.hands[hand].reachingForObject = true;
                    graph.hands[nothand].reachingForObject = true;
                }
                catch (Exception) { Debug.Log("Solace: Harmless exception happened in Player.Update riderHand"); }
            }

            // Friend stuff
            if (self.slugcatStats.name == FriendName) 
            {
                AbstractCreature guide0;
                AbstractCreature guide1;
                if (self?.room?.world?.overseersWorldAI?.playerGuide == null && !self.room.world.game.IsArenaSession)
                {
                    WorldCoordinate pos = new WorldCoordinate(self.room.world.offScreenDen.index, -1, -1, 0);
                    guide0 = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, pos, new EntityID());
                    guide1 = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, pos, new EntityID());
                    self.room.world.GetAbstractRoom(pos).entitiesInDens.Add(guide0);
                    self.room.world.GetAbstractRoom(pos).entitiesInDens.Add(guide1);
                    guide0.ignoreCycle = true; 
                    guide1.ignoreCycle = true;
                    (guide0.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(0); 
                    (guide1.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(1);
                    (guide0.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(self.room.abstractRoom.index); 
                    (guide1.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
                    if (self.GetPoacher().Wiggy == null) self.GetPoacher().Wiggy = guide0.realizedCreature as Overseer;
                    if (self.GetPoacher().Iggy == null) self.GetPoacher().Iggy = guide1.realizedCreature as Overseer;
                }
                // overseer code made with HUGE help from Leo, creator of the Lost!
                if (self.animation != ind.RocketJump && self.GetPoacher().HighJumped) self.GetPoacher().HighJumped = false;
            }
        }
        // Spear pointing while on a lizard
        public void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
        {
            orig(self, actuallyViewed, eu);
            try
            {
                if (self != null && self.GetPoacher().dragonSteed != null && self.GetPoacher().isRidingLizard)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (self?.grasps[i] != null && self?.grasps[i]?.grabbed != null && self?.grasps[i]?.grabbed is Weapon)
                        {
                            float rotation = (i == 1) ? self.GetPoacher().pointDir1+90 : self.GetPoacher().pointDir0+90f;
                            Vector2 vec = Custom.DegToVec(rotation);
                            (self?.grasps[i]?.grabbed as Weapon).setRotation = vec; //new Vector2(self.input[0].x*10, self.input[0].y*10);
                            (self?.grasps[i]?.grabbed as Weapon).rotationSpeed = 0f;
                        }
                    }
                }
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
        }
        // Allows Poacher to carry things that they couldn't usually
        public bool Player_HeavyCarry(On.Player.orig_HeavyCarry orig, Player self, PhysicalObject obj)
        {
            if (obj is Creature young && young.Template.type == CreatureTemplateType.YoungLizard) return false;
            else if (obj is Lizard mother && mother.GetLiz() != null && mother.GetLiz().IsRideable) return true;
            if (self.slugcatStats.name == DragonName)
            {
                if (obj is Creature crit && crit is not Hazer && crit is not VultureGrub && crit is not Snail && crit is not SmallNeedleWorm && crit is not TubeWorm) return orig(self, obj);
                else if (obj is MoreSlugcats.DandelionPeach || obj is DangleFruit) 
                {
                    if (!PoacherFoodParkour()) return false;
                    else return true;
                }
                else return false;
            }
            else return orig(self, obj);
        }
        public void DandelionPeach_Update(On.MoreSlugcats.DandelionPeach.orig_Update orig, MoreSlugcats.DandelionPeach self, bool eu)
        {
            orig(self, eu);
            if (!PoacherFoodParkour()) return;
            if (self.grabbedBy.Count > 0)
            {
                for (int i = 0; i < self.grabbedBy.Count; i++)
                {
                    if (self.grabbedBy[i].grabber is Player player && player.slugcatStats.name == DragonName)
                    {
                        if (player.animation == ind.None && player.bodyMode != bod.Stand) { self.firstChunk.mass = 0.34f; }
                        else self.firstChunk.mass = 0f;
                    }
                }
            }
            else self.firstChunk.mass = 0.34f;
        }
        public void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
        {
            orig(self, eu);
            if (self.grabbedBy.Count > 0)
            {
                for (int i = 0; i < self.grabbedBy.Count; i++)
                {
                    if (self.grabbedBy[i].grabber is Player player && player.slugcatStats.name == DragonName)
                    {
                        if (player.animation == ind.None && player.bodyMode != bod.Stand) { self.bodyChunks[0].mass = 0.2f; self.bodyChunks[1].mass = 0.2f; }
                        else { self.bodyChunks[0].mass = 0f; self.bodyChunks[1].mass = 0f; }
                    }
                }
            }
            else { self.bodyChunks[0].mass = 0.4f / 2f; self.bodyChunks[1].mass = 0.4f / 2f; }
        }
        public void DangleFruit_Update(On.DangleFruit.orig_Update orig, DangleFruit self, bool eu)
        {
            orig(self, eu);
            if (!PoacherFoodParkour()) return;
            if (self.grabbedBy.Count > 0)
            {
                for (int i = 0; i < self.grabbedBy.Count; i++)
                {
                    if (self.grabbedBy[i].grabber is Player player && player.slugcatStats.name == DragonName)
                    {
                        if (player.animation == ind.None && player.bodyMode != bod.Stand) { self.firstChunk.mass = 0.2f; }
                        else self.firstChunk.mass = 0f;
                    }
                }
            }
            else self.firstChunk.mass = 0.2f;
        }

        // Implement Maul
        public bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
        {
            if (SlugBase.SlugBaseCharacter.TryGet(slugcatNum, out var chara) && MaulEnabled.TryGet(chara, out var canMaul) && canMaul)
                return true;
            else
                return orig(slugcatNum);
        }
        // Implement Backspear
        public void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            try 
            {
                if (self.slugcatStats.name == FriendName && FriendBackspear() == true && self != null)
                {
                    self.spearOnBack = new Player.SpearOnBack(self);
                }
                if (self.slugcatStats.name == DragonName && self != null)
                {
                    self.setPupStatus(true);
                    self.GetPoacher().isPoacher = true;
                    self.GetPoacher().IsSkullVisible = true;
                    if (PoacherBackspear() == true) self.spearOnBack = new Player.SpearOnBack(self);
                }
            }
            catch(Exception e) { Debug.Log("Solace: Player.ctor hook failed" + e); }
        }
        // Implement SuperCrawl
        public void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);

            if (!self.standing && SuperCrawl.TryGet(self, out var boost))
            {
                if (self.superLaunchJump >= 20) self.GetPoacher().longjump = true;
                if (self.bodyMode == bod.Crawl)
                {
                    self.dynamicRunSpeed[0] += boost;
                    self.dynamicRunSpeed[1] += boost;
                }
                if (self.bodyMode == bod.Default)
                {
                    if (!self.GetPoacher().HighJumped)
                    {
                        self.dynamicRunSpeed[0] += boost - 1f;
                        self.dynamicRunSpeed[1] += boost - 1f;
                    }
                }
                if (self.GetPoacher().longjump == true && self.superLaunchJump == 0)
                {
                    if (self.GetPoacher().WantsUp) 
                    {
                        self.animation = ind.RocketJump;
                        self.GetPoacher().HighJumped = true;
                        self.bodyChunks[0].vel.y *= 4; 
                        self.bodyChunks[0].vel.x *= 0.3f;
                        self.bodyChunks[1].vel.x *= 0.3f;
                    }
                    else
                    {
                        self.bodyChunks[0].vel.x *= 1.1f;
                        self.bodyChunks[1].vel.x *= 1.1f;
                    }
                    self.GetPoacher().longjump = false;
                }
            }
        }
        // Implement SuperJump
        public void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            if (self.animation != ind.Roll && 
                !self.standing && 
                self.slugcatStats.name == FriendName && 
                Mathf.Abs(self.firstChunk.vel.x) > 3)
            { 
                if (self.bodyChunks[1].contactPoint.y == 0 || 
                    self.input.Count(i => i.jmp) == 9) 
                    return; 
            }
            orig(self);
            if (SuperJump.TryGet(self, out var power))
            {
                if (FriendUnNerf() == true) self.jumpBoost += 3f;
                else if (self.bodyMode == bod.Crawl) self.jumpBoost *= 1f + power / 2;
                else self.jumpBoost += power + 0.25f;

                if ((!(self.input[0].y > 0) && FriendAutoCrouch() == true))
                {
                    self.standing = false;
                }
            }
        }
        // Attempted ledgegrab annoyance fix
        public void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
        {
            orig(self);
            if (self.slugcatStats.name == FriendName)
            {
                if (self.animation == ind.LedgeGrab && self.input[0].y < 1) { self.standing = false; self.bodyMode = bod.Crawl; }
                if (self.animation == ind.StandOnBeam && self.input[0].y < 1 && PoleCrawl() == true) 
                { 
                    self.dynamicRunSpeed[0] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
                    self.dynamicRunSpeed[1] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
                }
            }
        }
        // Fix walljump bug
        public void Player_WallJump(On.Player.orig_WallJump orig, Player self, int direction)
        {
            orig(self, direction);
            if (self.slugcatStats.name == FriendName) self.standing = false;
        }
        // Some Friend Unnerfs
        public void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);
            if (slugcat == FriendName && FriendUnNerf() == true) self.poleClimbSpeedFac = 6f;
            if (slugcat == FriendName && FriendUnNerf() == true) self.runspeedFac = 0.8f;
        }
        // Friend leap changes
        public void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);
            if (self.slugcatStats.name != FriendName) return;
            if (self.GetPoacher().longjump && self.input[0].y == 0) self.GetPoacher().WantsUp = false;
            if (self.GetPoacher().longjump && self.input[0].y > 0)
            {
                self.GetPoacher().WantsUp = true;
                self.input[0].y = 0;
                self.input[0].x = 0;
            }
            if (self.input[0].y < 0 && self.superLaunchJump != 0)
            {
                self.superLaunchJump = 0;
                self.killSuperLaunchJumpCounter = 0;
                self.GetPoacher().WantsUp = false;
            }
            if (!self.GetPoacher().longjump) self.GetPoacher().WantsUp = false;
        }
        // Hud changes
        public void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            orig(self, package);
            if (self.IsSleepScreen && (package.characterStats.name == FriendName || package.characterStats.name == DragonName))
            {
                if (self.soundLoop != null) self.soundLoop.Destroy();
                self.mySoundLoopID = MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
            }
        } // Improved sleep screen
        public void RainMeter_Update(On.HUD.RainMeter.orig_Update orig, RainMeter self)
        {
            orig(self);
            if (((self.hud.owner as Player).slugcatStats.name == FriendName ||
                (self.hud.owner as Player).slugcatStats.name == DragonName) &&
                self.hud.map.RegionName != "HR") self.halfTimeShown = true;
        } // Makes solace rain timer function like Saint's
        public void RainMeter_ctor(On.HUD.RainMeter.orig_ctor orig, RainMeter self, HUD.HUD hud, FContainer fContainer)
        {
            orig(self, hud, fContainer);
            if (((self.hud.owner as Player).slugcatStats.name == FriendName ||
                (self.hud.owner as Player).slugcatStats.name == DragonName) &&
                self.hud.map.RegionName != "HR") self.halfTimeShown = true;
        } // Makes solace rain timer function like Saint's
        public void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
        {
            orig(self, timeStacker);
            if ((self.hud.owner as Player).slugcatStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint) // if showing timer is true
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    if (ShowCycleTimer()) self.circles[i].Draw(timeStacker);
                    else self.circles[i].sprite.scale = 0;
                }
            }
            if (((self.hud.owner as Player).slugcatStats.name == FriendName ||
                (self.hud.owner as Player).slugcatStats.name == DragonName) &&
                self.hud.map.RegionName != "HR" && !ShowCycleTimer()) // if showing timer is false
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].rad = 0;
                    self.circles[i].lastRad = 0;
                    self.circles[i].fade = 0;
                    self.circles[i].lastFade = 0;
                    self.circles[i].sprite.scale = 0;
                }
            }
        } // Makes rain timer visible or not
        #endregion




        #region options
        // Options code
        public bool FriendAutoCrouch()
        {
            if (Options.FriendAutoCrouch.Value == true) return true;
            else return false;
        }
        public static bool PoleCrawl()
        {
            if (Options.PoleCrawl.Value == true) return true;
            else return false;
        }
        public static bool FriendUnNerf()
        {
            if (Options.FriendUnNerf.Value == true) return true;
            else return false;
        }
        public static bool FriendBackspear()
        {
            if (Options.FriendBackspear.Value == true) return true;
            else return false;
        }
        public static bool FriendRepLock()
        {
            if (Options.FriendRepLock.Value == true) return true;
            else return false;
        }

        public static bool PoacherBackspear()
        {
            if (Options.PoacherBackspear.Value == true) return true;
            else return false;
        }
        public static bool PoacherPupActs()
        {
            if (Options.PoacherPupActs.Value == true) return true;
            else return false;
        }
        public static bool PoacherFreezeFaster()
        {
            if (Options.PoacherFreezeFaster.Value == true) return true;
            else return false;
        }
        public static bool PoacherFoodParkour()
        {
            if (Options.PoacherFoodParkour.Value == true) return true;
            else return false;
        }

        public static bool NoFamine()
        {
            if (Options.NoFamine.Value == true) return true;
            else return false;
        }
        public static bool ExpeditionFamine()
        {
            if (Options.ExpeditionFamine.Value == true) return true;
            else return false;
        }
        public static bool FaminesForAll()
        {
            if (Options.FaminesForAll.Value == true) return true;
            else return false;
        }

        public static bool LizRep()
        {
            if (Options.LizRepMeter.Value == true) return true;
            else return false;
        }
        public static bool LizRepAll()
        {
            if (Options.LizRepMeterForAll.Value == true) return true;
            else return false;
        }
        public static bool LizRide()
        {
            if (Options.LizRide.Value == true) return true;
            else return false;
        }
        public static bool LizRideAll()
        {
            if (Options.LizRideAll.Value == true) return true;
            else return false;
        }
        public static bool LocalLizRep()
        {
            if (Options.LocalizedLizRep.Value == true) return true;
            else return false;
        }
        public static bool LocalLizRepAll()
        {
            if (Options.LocalizedLizRepForAll.Value == true) return true;
            else return false;
        }
        public static bool ShowCycleTimer()
        {
            if (Options.SolaceBlizzTimer.Value == true) return true;
            else return false;
        }
        #endregion
    }
}