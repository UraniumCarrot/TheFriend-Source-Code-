using System.Collections.Generic;
using HUD;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.HudThings;

public class GenericWorldHud
{
    public abstract class GenericUI : HudPart
    {
        public FSprite[] sprites;
        public HUDCircle[] circles;

        public int spriteAmount;
        public int circleAmount;

        public Vector2 pos;
        public Vector2 lastPos;
        
        public float fade;
        public float lastFade;
        public Player owner;

        public bool hideInGates;
        public bool InAGate;
        
        public bool hideInShelter;
        public bool InAShelter;

        public bool tieToGeneralHud;
        public bool GeneralHudVisible;

        public bool tieToHypothermia;
        public bool HypothermiaVisible;

        public bool SpritesExist = false;

        public bool CaresAboutRain;
        public bool CaresAboutReinforce;

        public GenericUI(HUD.HUD hud, FContainer fContainer, Player player) : base(hud)
        {
            owner = player;
            pos = hud.karmaMeter.pos;
            lastPos = pos;

            fade = 1;
            lastFade = fade;
        }
        
        public void MakeSprites(FContainer fContainer)
        { // ALWAYS call MakeSprites AFTER defining amounts, but BEFORE giving sprites their names
          // Call in constructor class
            circles = new HUDCircle[circleAmount];
            sprites = new FSprite[spriteAmount];
            for (int i = 0; i < circles.Length; i++) 
                circles[i] = new HUDCircle(
                hud, 
                HUDCircle.SnapToGraphic.smallEmptyCircle, 
                fContainer, 
                0);
            for (int i = 0; i < sprites.Length; i++)
                sprites[i] = new FSprite("Futile_White");
            
            foreach (FSprite i in sprites)
                fContainer.AddChild(i);
            SpritesExist = true;
        }
        public override void Update()
        {
            base.Update();
            if (!SpritesExist) return;

            InAShelter = owner.room?.abstractRoom.shelter != null;
            InAGate = owner.room?.regionGate != null;
            GeneralHudVisible = hud.foodMeter.fade > 0;
            
            pos = hud.karmaMeter.pos;
            pos += (CaresAboutRain && owner.GetGeneral().RainTimerExists) ? new Vector2(0,10 * hud.rainMeter.fade) : Vector2.zero;
            pos += (CaresAboutReinforce && hud.karmaMeter.showAsReinforced) ? new Vector2(0, 10) : Vector2.zero;

            if ((hideInShelter && InAShelter) || 
                (hideInGates && InAGate) || 
                (tieToGeneralHud && !GeneralHudVisible))
            {
                if (fade > 0) fade -= 0.15f;
            }
            else if (fade < 1) fade += 0.1f;
        }
        public virtual void UpdatePositions()
        { // ALWAYS call UpdatePositions AFTER making changes to pos in update methods
          // Or... just override it
            lastPos = pos;
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();
                circles[i].pos = pos;
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].SetPosition(pos);
            }
        }
        
        
        

        public override void ClearSprites()
        { // DO NOT TOUCH.
            base.ClearSprites();
            foreach (FSprite i in this.sprites)
                i.RemoveFromContainer();
            SpritesExist = false;
        }
        public Vector2 DrawPos(float timestacker)
        { // DO NOT TOUCH.
            return Vector2.Lerp(lastPos, pos, timestacker);
        }
        public override void Draw(float timestacker)
        { // DO NOT TOUCH.
            base.Draw(timestacker);
            if (!SpritesExist) return;
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Draw(timestacker);
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].x = lastPos.x;
                sprites[i].y = lastPos.y;
            }
        }
    }
}