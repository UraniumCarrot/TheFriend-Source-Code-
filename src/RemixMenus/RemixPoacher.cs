using Menu.Remix.MixedUI;
using UnityEngine;

namespace TheFriend.RemixMenus;
public partial class RemixMain
{
    public static Configurable<bool> PoacherPupActs;
    public static Configurable<bool> PoacherBackspear;
    public static Configurable<bool> PoacherFreezeFaster;
    public static Configurable<bool> PoacherFoodParkour;
    public static Configurable<bool> PoacherJumpNerf;
    public void RemixPoacher()
    {
        PoacherPupActs = config.Bind("PoacherPupActs", true, new ConfigAcceptableList<bool>(true, false));
        PoacherBackspear = config.Bind("PoacherBackspear", false, new ConfigAcceptableList<bool>(true, false));
        PoacherFreezeFaster = config.Bind("PoacherFreezeFaster", false, new ConfigAcceptableList<bool>(true, false));
        PoacherFoodParkour = config.Bind("PoacherFoodParkour", true, new ConfigAcceptableList<bool>(true, false));
        PoacherJumpNerf = config.Bind("PoacherJumpNerf", true, new ConfigAcceptableList<bool>(true, false));
    }

    public void RemixPoacherUpdate()
    {
        
    }
    
    public OpTab OpTabPoacher;
    public OpContainer PoacherSprites;
    public void RemixPoacherInit()
    {
        OpTabPoacher = new OpTab(this, "Poacher");
        PoacherSprites = new OpContainer(Vector2.zero);
        
        tabsList.Add(OpTabPoacher);
        OpTabPoacher.AddItems(PoacherSprites);
    }

    public void RemixPoacherLayout()
    {
        PoacherOpTabMovement();
        PoacherOpTabCombat();
        PoacherOpTabOther();
        
        var columnseparator1 = MakeLine(new Vector2((column + (columnMult * 1.5f))/2,300), true);
        var columnseparator2 = MakeLine(new Vector2((column + (columnMult * 3.54f))/2,300), true);
        
        PoacherSprites.container.AddChild(
            new FSprite("symbolpoacher")
            {
                scale = 1.315f,
                alpha = 0.05f,
                x = 300,
                y = 300
            });
        PoacherSprites.container.AddChild(columnseparator1);
        PoacherSprites.container.AddChild(columnseparator2);
    }

    public void PoacherOpTabMovement()
    {
        OpTabPoacher.AddItems(
            new OpLabel(charcolumn, row + 25, "Movement", true) { alpha = 0.5f }
            );
    }
    public void PoacherOpTabCombat()
    {
        OpTabPoacher.AddItems(
            new OpLabel(charcolumn + (columnMult * 0.95f), row + 25, "Combat", true) { alpha = 0.5f }
            );
    }
    public void PoacherOpTabOther()
    {
        OpTabPoacher.AddItems(
            new OpLabel(charcolumn + (columnMult * 1.96f),row + 25, "Other", true) { alpha = 0.5f }
            );
    }
    

}