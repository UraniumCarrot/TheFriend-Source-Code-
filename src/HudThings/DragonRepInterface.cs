using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using UnityEngine;
using SlugBase.SaveData;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges;
using FadeCircle = On.HUD.FadeCircle;
using RainMeter = On.HUD.RainMeter;

namespace TheFriend.HudThings;

public static class DragonRepInterface
{
    public static void Apply()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }

    public static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    { // Forces new hud elements to work
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

        //public int remainVisibleCounter;
        public int symbolInd;
        public bool ingate;
        public bool MotherKilledHere;
        public bool Drawn;

        public float fade;
        public float lastFade;
        public float reputation;
        public Player owner;

        public HUDCircle[] circles;
        public FSprite dragonSprite;
        public FSprite friendSprite;
        public FSprite motherSprite;
        public FSprite noneSprite;

        public DragonUI(HUD.HUD hud, FContainer fContainer, Player player) : base(hud)
        {
            owner = player;
            pos = hud.karmaMeter.pos;
            lastPos = pos;
            circles = new HUDCircle[3];
            dragonSprite = new FSprite("DragonSlayerB");
            friendSprite = new FSprite("FriendB");
            motherSprite = new FSprite("MotherB");
            noneSprite = new FSprite("guardHead");
            for (int i = 0; i < circles.Length; i++) circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            fContainer.AddChild(dragonSprite);
            fContainer.AddChild(friendSprite);
            fContainer.AddChild(motherSprite);
            fContainer.AddChild(noneSprite);
            fade = 1;
            lastFade = fade;
        }
        public override void Update()
        {
            // Setup values
            bool maxed = Mathf.Abs(reputation) > 0.999 || symbolInd == 3;
            Drawn = owner.GetPoacher().RainTimerExists;
            ingate = owner.room?.regionGate != null;
            if (FriendWorldState.customLock) MotherKilledHere = true;
            else MotherKilledHere = false;
            
            // Fade
            if (fade < 1 && hud.showKarmaFoodRain) fade += 0.1f;
            else if (fade > 0 && !hud.showKarmaFoodRain) fade -= 0.15f;
            if (fade < 0) fade = 0;
            if (hud.HideGeneralHud) fade = 0f; // ???
            lastFade = fade;
            
            // Position
            pos = hud.karmaMeter.pos + new Vector2(0, 40) + // base height
                  (hud.karmaMeter.showAsReinforced ? new Vector2(0, 10) : Vector2.zero); // gains more height when karma is reinforced
            if (Drawn) pos += new Vector2(0, 10 * hud.rainMeter.fade); // gains more height when rain meter is visible
            pos += circles[1].visible ? new Vector2(0,3) : Vector2.zero; // gains more height if at max/min rep
            lastPos = pos;

            // Circle properties
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();
                circles[i].fade = fade;
                circles[i].pos = pos;
            }
            circles[0].thickness = 2f;
            circles[0].rad = 10f;
            circles[1].thickness = 1.5f;
            circles[1].rad = 13.5f;
            circles[2].thickness = 1.5f;
            circles[2].rad = 6f;

            // Sprite properties
            dragonSprite.scale = 0.5f;
            dragonSprite.x = pos.x;
            dragonSprite.y = pos.y;

            friendSprite.scale = 0.5f;
            friendSprite.x = pos.x;
            friendSprite.y = pos.y;
            
            motherSprite.scale = 0.5f;
            motherSprite.x = pos.x;
            motherSprite.y = pos.y;
            
            noneSprite.scaleX = 0.2f;
            noneSprite.scaleY = 0.07f;
            noneSprite.x = pos.x;
            noneSprite.y = pos.y;
            
            // Symbol visibility
            if (!MotherKilledHere && !ingate)
            {
                switch (reputation)
                {
                    default: 
                        symbolInd = 0; break; // Neutral
                    case <= -0.5f: 
                        symbolInd = 1; break; // Negative
                    case >= 0.5f:
                        symbolInd = 2; break; // Positive
                }
            }
            else
            {
                if (MotherKilledHere && !ingate) symbolInd = 3;
                else symbolInd = 4;
            }

            switch (symbolInd)
            {
                case 0: circles[2].visible = true; break;
                case 1: dragonSprite.isVisible = true; break;
                case 2: friendSprite.isVisible = true; break;
                case 3: motherSprite.isVisible = true; break;
                case >= 4: noneSprite.isVisible = true; break;
            }

            if (symbolInd != 0) circles[2].visible = false;
            if (symbolInd != 1) dragonSprite.isVisible = false;
            if (symbolInd != 2) friendSprite.isVisible = false;
            if (symbolInd != 3) motherSprite.isVisible = false;
            if (symbolInd < 4) noneSprite.isVisible = false;
            
            if (maxed && !ingate) circles[1].visible = true; // Max/Min
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
            dragonSprite.alpha = fade;

            friendSprite.x = circles[0].lastPos.x;
            friendSprite.y = circles[0].lastPos.y;
            friendSprite.alpha = fade;
            
            motherSprite.x = circles[0].lastPos.x;
            motherSprite.y = circles[0].lastPos.y;
            motherSprite.alpha = circles[0].fade * Random.Range(0.7f,1f);
            
            noneSprite.x = circles[0].lastPos.x;
            noneSprite.y = circles[0].lastPos.y;
            noneSprite.alpha = fade;
            
            circles[1].fade = motherSprite.isVisible ? motherSprite.alpha : fade;
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            if (dragonSprite != null) dragonSprite.RemoveFromContainer();
            if (friendSprite != null) friendSprite.RemoveFromContainer();
            if (motherSprite != null) motherSprite.RemoveFromContainer();
            if (noneSprite != null) noneSprite.RemoveFromContainer();
        }

        public Vector2 DrawPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }
    }
}
