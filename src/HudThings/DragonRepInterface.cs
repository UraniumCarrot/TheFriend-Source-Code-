using HUD;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheFriend.HudThings;

public static class DragonRepInterface
{
    public static void Apply()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }
    public static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam) // Forces new hud to work
    {
        orig(self, cam);
        if (Plugin.LizRep() && ((self.owner as Player).room.world.game.StoryCharacter == Plugin.FriendName || (self.owner as Player).room.world.game.StoryCharacter == Plugin.DragonName || Plugin.LizRepAll())) self.AddPart(new DragonUI(self, self.fContainers[1], self.owner as Player));
    }

    public class DragonUI : HudPart
    {
        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 custPos; // = (!Plugin.ShowCycleTimer()) ? : new Vector2(-40,30);

        public int remainVisibleCounter;
        public float fade;
        public float lastFade;
        public float reputation;

        public HUDCircle[] circles;
        public FSprite dragonSprite;
        public FSprite friendSprite;

        public DragonUI(HUD.HUD hud, FContainer fContainer, Player player) : base(hud)
        {
            custPos = hud.rainMeter.circles[0].sprite.scale == 0 &&
                (player.room.game.StoryCharacter == Plugin.FriendName ||
                player.room.game.StoryCharacter == Plugin.DragonName ||
                player.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint) ? new Vector2(-35, 25) : new Vector2(-40, 30);
            pos = hud.karmaMeter.pos + custPos;
            lastPos = pos;
            circles = new HUDCircle[3];
            dragonSprite = new FSprite("DragonSlayerB");
            friendSprite = new FSprite("FriendB");
            for (int i = 0; i < circles.Length; i++) circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            fContainer.AddChild(dragonSprite);
            fContainer.AddChild(friendSprite);
            fade = hud.karmaMeter.fade;
            lastFade = fade;
        }
        public override void Update()
        {
            if (hud.owner as Player != null && (hud.owner as Player).room != null)
            {
                reputation = (hud.owner as Player).room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Lizards, (hud.owner as Player).room.world.region.regionNumber, 0);
            }

            lastPos = pos;
            lastFade = fade;
            if (hud.HideGeneralHud)
            {
                fade = 0f;
            }
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();
                circles[i].fade = hud.karmaMeter.fade;
                circles[i].pos = hud.karmaMeter.pos + custPos;
            }
            circles[0].thickness = 2f;
            circles[0].rad = 10f;
            circles[1].thickness = 1.5f;
            circles[1].rad = 13.5f;
            circles[2].thickness = 1.5f;
            circles[2].rad = 6f;

            dragonSprite.scale = 0.5f;
            dragonSprite.x = hud.karmaMeter.pos.x + custPos.x;
            dragonSprite.y = hud.karmaMeter.pos.y + custPos.y;

            friendSprite.scale = 0.5f;
            friendSprite.x = hud.karmaMeter.pos.x + custPos.x;
            friendSprite.y = hud.karmaMeter.pos.y + custPos.y;

            if (reputation >= -0.5 && reputation <= 0.5) circles[2].visible = true; // Monk
            else circles[2].visible = false;
            if (reputation < -0.5) dragonSprite.isVisible = true; // Dragonslayer
            else dragonSprite.isVisible = false;
            if (reputation > 0.5) friendSprite.isVisible = true; // Friend
            else friendSprite.isVisible = false;
            if (Mathf.Abs(reputation) == 1) circles[1].visible = true; // Max/Min
            else circles[1].visible = false;
        }
        public override void Draw(float timeStacker)
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Draw(timeStacker);
            }
            dragonSprite.x = circles[0].lastPos.x;
            dragonSprite.y = circles[0].lastPos.y;
            dragonSprite.alpha = circles[0].fade;

            friendSprite.x = circles[0].lastPos.x;
            friendSprite.y = circles[0].lastPos.y;
            friendSprite.alpha = circles[0].fade;
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            if (dragonSprite != null) dragonSprite.RemoveFromContainer();
            if (friendSprite != null) friendSprite.RemoveFromContainer();
        }

        public Vector2 DrawPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }
    }
}
