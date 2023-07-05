using System.Collections.Generic;
using System.Linq;
using IL.RWCustom;
using Menu;
using MoreSlugcats;
using SlugBase.SaveData;
using UnityEngine;

namespace TheFriend.HudThings;

public class MotherKillTracker : PositionedMenuObject
{
    public static void Apply()
    {
        
    }

    public FSprite[] circleSprites;
    public FContainer container;
    public CollectiblesTracker tracker;
    public SaveState currentSave;

    public MotherKillTracker(Menu.Menu menu, MenuObject owner, Vector2 pos, SaveState currentSave, FContainer container, CollectiblesTracker tracker) : base(menu, owner, pos)
    {
        if (!currentSave.miscWorldSaveData.GetSlugBaseData()
                .TryGet("MothersKilledInRegionStr", out List<string> regionsKilledInStr))
            return;
        Color col = new Color(0.2f, 0.1f, 0.15f);

        this.tracker = tracker;
        this.currentSave = currentSave;
        this.container = container;
        circleSprites = new FSprite[regionsKilledInStr.Count];

        for (int a = 0; a < regionsKilledInStr.Count; a++)
        {
            circleSprites[a] = new FSprite("Circle20");
            container.AddChild(circleSprites[a]);
            circleSprites[a].MoveBehindOtherNode(tracker.regionIcons[a]);

            for (int b = 0; b < tracker.displayRegions.Count; b++)
            {
                if (!regionsKilledInStr.Contains(tracker.displayRegions[b])) break;
                circleSprites[b].isVisible = true;
                circleSprites[b].x = tracker.regionIcons[b].x;
                circleSprites[b].y = tracker.regionIcons[b].y;
                circleSprites[b].color = Color.red;
                circleSprites[b].scale = 0.5f;
                Debug.Log($"circleSprites[{a}] visibility is " + circleSprites[b].isVisible + ", represented is " + regionsKilledInStr[a] + ", list length is " + regionsKilledInStr.Count);
            }
        }
        /*
        for (int i = 0; i < circleSprites.Length; i++)
        {
            circleSprites[i] = new FSprite("Circle20");
            container.AddChild(circleSprites[i]);
            circleSprites[i].MoveBehindOtherNode(tracker.regionIcons[i]);
            circleSprites[i].isVisible = regionsKilledInStr[i] == tracker.displayRegions[i];
            // Code above this point works fine
            
            circleSprites[i].x = tracker.regionIcons[i].x;
            circleSprites[i].y = tracker.regionIcons[i].y;
            circleSprites[i].color = Color.red;
            circleSprites[i].scale = 0.5f;
            Debug.Log($"circleSprites[{i}] visibility is " + circleSprites[i].isVisible + ", represented is " + regionsKilledInStr[i] + ", list length is " + regionsKilledInStr.Count);
        }*/
    }
}