using System.Collections.Generic;
using System.Linq;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace TheFriend.RemixMenus;
public partial class RemixMain : OptionInterface
{

    public List<SlugcatStats.Name> SlugcatsWithRepMeter;
    public List<SlugcatStats.Name> SlugcatsWithLocalRep;
    public List<SlugcatStats.Name> SlugcatsWithFamineOT;

    public RemixMain()
    {
        tabsList = new List<OpTab>();
        GenLizardList = new List<UIelement>();
        GenFamineList = new List<UIelement>();
        GenMiscelList = new List<UIelement>();

        SlugcatsWithRepMeter = new List<SlugcatStats.Name>();
        SlugcatsWithLocalRep = new List<SlugcatStats.Name>();
        SlugcatsWithFamineOT = new List<SlugcatStats.Name>();
        
        RemixGeneral();
        RemixFriend();
        RemixPoacher();
        RemixNoirCatto();
        RemixDeluge();
    }

    public override void Update()
    {
        base.Update();
        RemixGeneralUpdate();
        RemixFriendUpdate();
        RemixPoacherUpdate();
        RemixNoirCattoUpdate();
        RemixDelugeUpdate();
    }
    
    // X values - Width presets
    private const float column = 40;
    private const float columnMult = 220; // Multiply to pick column - gets added to column float

    private const float charcolumn = 20;

    // Y values - Height presets
    private const float row = 475;
    private const float rowMult = 30; // Multiply to pick row - gets subtracted from row float

    public List<OpTab> tabsList;
    public override void Initialize()
    {
        tabsList.Clear();
        RemixGeneralInit();
        RemixFriendInit();
        RemixPoacherInit();
        RemixNoirCattoInit();
        RemixDelugeInit();
        
        base.Initialize();
        Tabs = tabsList.ToArray();
        
        RemixGeneralLayout();
        RemixFriendLayout();
        RemixPoacherLayout();
        RemixNoirCattoLayout();
        RemixDelugeLayout();

        foreach (OpTab tab in Tabs)
        {
            tab.AddItems(new UIelement[]
            {
                new OpLabel(20, 570,Translate($"Rain World: Solace Config - {tab.name}"), true),
                new OpLabel(20, 550, Translate($"Version {Plugin.MOD_VERSION}"))
            });
        }
    }



    public FSprite MakeLine(Vector2 pos, bool vertical)
    {
        var line = new FSprite("pixel");
        line.color = Menu.MenuColorEffect.rgbMediumGrey;
        line.scaleX = (vertical) ? 2 : 450;
        line.scaleY = (vertical) ? 450 : 2;
        line.SetPosition(pos);
        return line;
    }

}