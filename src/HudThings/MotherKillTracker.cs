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
                .TryGet(Plugin.MothersKilled, out List<string> regionsKilledInStr))
            return;
        Color col = new Color(0.5f, 0.1f, 0.15f);

        this.tracker = tracker;
        this.currentSave = currentSave;
        this.container = container;
        circleSprites = new FSprite[regionsKilledInStr.Count];

        for (int a = 0; a < regionsKilledInStr.Count; a++)
        {
            int ind = tracker.displayRegions.IndexOf(regionsKilledInStr[a]);
            circleSprites[a] = new FSprite("MonkA");
            container.AddChild(circleSprites[a]);
            circleSprites[a].MoveBehindOtherNode(tracker.regionIcons[ind]);

            circleSprites[a].isVisible = true;
            circleSprites[a].color = col;
            circleSprites[a].scale = 0.6f;
        }
    }
    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        if (!circleSprites.Any(i => i.x == 0)) return;
        if (!currentSave.miscWorldSaveData.GetSlugBaseData()
                .TryGet(Plugin.MothersKilled, out List<string> regionsKilledInStr))
            return;
        for (int a = 0; a < regionsKilledInStr.Count; a++)
        {
            int ind = tracker.displayRegions.IndexOf(regionsKilledInStr[a]);
            circleSprites[a].x = tracker.regionIcons[ind].x;
            circleSprites[a].y = tracker.regionIcons[ind].y;
        }
    }
}