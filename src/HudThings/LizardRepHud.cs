using UnityEngine;
using TheFriend.WorldChanges;

namespace TheFriend.HudThings;

public class LizardRepHud
{
    public class LizardUI : GenericWorldHud.GenericUI
    {
        public float reputation;
        public bool repMaxed;
        public int activeSymbol;
        
        public LizardUI(HUD.HUD hud, FContainer fContainer, Player player) : base(hud, fContainer, player)
        {
            tieToGeneralHud = true;
            CaresAboutRain = true;
            CaresAboutReinforce = true;
            
            spriteAmount = 5;
            circleAmount = 3;
            
            MakeSprites(fContainer);
            
            var atlas = Futile.atlasManager;
            sprites[0].element = atlas.GetElementWithName("DragonSlayerB"); // Rep is negative
            sprites[1].element = atlas.GetElementWithName("FriendB"); // Rep is positive
            sprites[2].element = atlas.GetElementWithName("MotherB"); // Mother lizard killed in region
            sprites[3].element = atlas.GetElementWithName("guardHead"); // Rep is hidden
            sprites[4].element = atlas.GetElementWithName("Futile_White"); // Neutral rep stand-in since it normally uses a HUDCircle
            sprites[4].isVisible = false;
            
            LizUIStats();
        }

        public override void Update()
        {
            base.Update();
            ReputationUpdate();
            
            pos += new Vector2(0, 40);
            pos += (repMaxed) ? new Vector2(0,3) : Vector2.zero;
            fade = hud.karmaMeter.fade;
            Debug.Log(sprites[0].GetPosition() + "," + circles[0].pos);
            SymbolUpdate();
            UpdatePositions();

            if (InAGate) activeSymbol = 3;
            else if (FriendWorldState.customLock) activeSymbol = 2;
            else
            {
                switch (reputation)
                {
                    default: 
                        activeSymbol = 4; break; // Neutral
                    case <= -0.5f: 
                        activeSymbol = 0; break; // Negative
                    case >= 0.5f:
                        activeSymbol = 1; break; // Positive
                }
            }
        }

        public void SymbolUpdate()
        {
            // This fade system allows symbols to smoothly transition between eachother at the cost of being less straightforward
            foreach (FSprite i in sprites)
            {
                int index = sprites.IndexOf(i);

                if (index == activeSymbol)
                {
                    if (sprites[index].alpha < fade)
                        sprites[index].alpha += 0.15f;
                    if (index == 2) sprites[index].alpha = circles[0].fade * Random.Range(0.7f,1f);
                }
                else if (sprites[index].alpha > 0) 
                    sprites[index].alpha -= 0.1f;
            }
            
            // Max Rep ring
            if (repMaxed)
            {
                if (circles[1].fade < fade)
                    circles[1].fade += 0.15f;
                if (activeSymbol == 2) circles[1].fade = sprites[2].alpha;
            }
            else if (circles[1].fade > 0) 
                circles[1].fade -= 0.1f;

            // Neutral Rep ring
            if (activeSymbol == 4) 
                circles[2].fade = circles[0].fade;
            else if (circles[2].fade > 0) 
                circles[2].fade -= 0.1f;

            // Base rep ring
            circles[0].fade = fade;
        }
        
        public void LizUIStats()
        {
            circles[0].thickness = 2f;
            circles[0].rad = 10f;
            circles[1].thickness = 1.5f;
            circles[1].rad = 13.5f;
            circles[2].thickness = 1.5f;
            circles[2].rad = 6f;

            foreach (FSprite i in sprites)
            {
                if (sprites.IndexOf(i) == 3) continue;
                i.scale = 0.5f;
            }
            sprites[3].scaleX = 0.2f;
            sprites[3].scaleY = 0.07f;
        }

        public void ReputationUpdate()
        {
            if (owner.room != null)
                reputation = owner.room.game.session.creatureCommunities.LikeOfPlayer(
                    CreatureCommunities.CommunityID.Lizards, 
                    owner.room.world.RegionNumber,
                    owner.playerState.playerNumber);
            repMaxed = (Mathf.Abs(reputation) > 0.999 || activeSymbol == 2) && activeSymbol != 3;
        }
    }
}