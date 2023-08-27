using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using UnityEngine;
using SlugBase.SaveData;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges;

namespace TheFriend.HudThings;

public static class DragonRepInterface
{
    public static void Apply()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }
    public static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam) // Forces new hud elements to work
    {
        orig(self, cam);
        if (Plugin.LizRep() && 
            ((self.owner as Player)?.room.world.game.StoryCharacter == Plugin.FriendName || 
             (self.owner as Player)?.room.world.game.StoryCharacter == Plugin.DragonName || 
             Plugin.LizRepAll())) 
            self.AddPart(new DragonUI(self, self.fContainers[1], self.owner as Player));
    }

    public class DragonUI : HudPart
    {
        public Vector2 pos;
        public Vector2 lastPos;

        public int remainVisibleCounter;

        public float fade;
        public float lastFade;
        public float reputation;
        public Player owner;

        public HUDCircle[] circles;
        public FSprite dragonSprite;
        public FSprite friendSprite;
        public FSprite motherSprite;

        public DragonUI(HUD.HUD hud, FContainer fContainer, Player player) : base(hud)
        {
            owner = player;
            pos = hud.karmaMeter.pos;
            lastPos = pos;
            circles = new HUDCircle[3];
            dragonSprite = new FSprite("DragonSlayerB");
            friendSprite = new FSprite("FriendB");
            motherSprite = new FSprite("MotherB");
            for (int i = 0; i < circles.Length; i++) circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            fContainer.AddChild(dragonSprite);
            fContainer.AddChild(friendSprite);
            fContainer.AddChild(motherSprite);
            fade = hud.karmaMeter.fade;
            lastFade = fade;
        }
        public override void Update()
        {
            bool rainvisible = false;
            if (hud.owner is Player pl && (hud.owner as Player)?.room != null)
            {
                reputation = pl.room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Lizards, pl.room.world.region.regionNumber, 0);
                pl.room.world.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().TryGet("MothersKilledInRegion", out List<int> regionsKilledIn);
                
                if (regionsKilledIn.Contains(pl.room.world.RegionNumber))
                    motherSprite.isVisible = true; // Mother
                else motherSprite.isVisible = false;
                
                if (pl.room.world.rainCycle.AmountLeft > 0 && 
                    (Plugin.ShowCycleTimer() ||
                     (!FriendWorldState.SolaceWorldstate &&
                      pl.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Saint)))
                    rainvisible = true;
            }
            
            pos = hud.karmaMeter.pos + (hud.karmaMeter.showAsReinforced ? new Vector2(0, 10) : Vector2.zero) + // gains more height when karma is reinforced
                  new Vector2(0, 40); // base height
            pos += rainvisible ? new Vector2(0, 10) : Vector2.zero; // gains more height when rain meter is visible
            pos += circles[1].visible ? new Vector2(0,5) : Vector2.zero; // gains more height if at max/min rep
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
                circles[i].pos = pos;
            }
            circles[0].thickness = 2f;
            circles[0].rad = 10f;
            circles[1].thickness = 1.5f;
            circles[1].rad = 13.5f;
            circles[2].thickness = 1.5f;
            circles[2].rad = 6f;

            dragonSprite.scale = 0.5f;
            dragonSprite.x = pos.x;
            dragonSprite.y = pos.y;

            friendSprite.scale = 0.5f;
            friendSprite.x = pos.x;
            friendSprite.y = pos.y;
            
            motherSprite.scale = 0.5f;
            motherSprite.x = pos.x;
            motherSprite.y = pos.y;

            if (!motherSprite.isVisible)
            {
                if (reputation >= -0.5 && reputation <= 0.5) circles[2].visible = true; // Monk
                else circles[2].visible = false;
                if (reputation < -0.5) dragonSprite.isVisible = true; // Dragonslayer
                else dragonSprite.isVisible = false;
                if (reputation > 0.5) friendSprite.isVisible = true; // Friend
                else friendSprite.isVisible = false;
            }
            if (Mathf.Abs(reputation) == 1 || motherSprite.isVisible) circles[1].visible = true; // Max/Min
            else circles[1].visible = false;
            if (motherSprite.isVisible)
            {
                circles[2].visible = false;
                dragonSprite.isVisible = false;
                friendSprite.isVisible = false;
            }
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
            
            motherSprite.x = circles[0].lastPos.x;
            motherSprite.y = circles[0].lastPos.y;
            motherSprite.alpha = circles[0].fade * Random.Range(0.7f,1f);
            circles[1].fade = motherSprite.isVisible ? motherSprite.alpha : circles[0].fade;
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            if (dragonSprite != null) dragonSprite.RemoveFromContainer();
            if (friendSprite != null) friendSprite.RemoveFromContainer();
            if (motherSprite != null) motherSprite.RemoveFromContainer();
        }

        public Vector2 DrawPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }
    }
}
