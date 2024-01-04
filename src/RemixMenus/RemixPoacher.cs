using Menu.Remix.MixedUI;
using TheFriend.RemixMenus.CustomRemixObjects;
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
        
        var columnseparator1 = MakeLine(new Vector2((column + (columnMult * 1.5f))/2,320), true);
        var columnseparator2 = MakeLine(new Vector2((column + (columnMult * 3.54f))/2,320), true);
        
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
            new OpLabel(charcolumn, row + 25, "Movement", true) { alpha = 0.5f },
            new OpCheckboxLabelled(PoacherFoodParkour, charcolumn, row, "Food Parkour") 
            { 
                description =  
                    Translate("Allows some food items to affect Poacher's movement when held") 
            },
            new OpCheckboxLabelled(PoacherJumpNerf, charcolumn, row-(rowMult),"Jump Nerf") 
            { 
                description =
                    Translate("Makes Poacher's jumps accurate to that of a pup's") 
            }
            );
    }
    public void PoacherOpTabCombat()
    {
        OpTabPoacher.AddItems(
            new OpLabel(charcolumn + (columnMult * 0.95f), row + 25, "Combat", true) { alpha = 0.5f },
            new OpCheckboxLabelled(PoacherBackspear, charcolumn + (columnMult * 0.95f), row, "Backspear Enable")
            { 
                description = 
                    Translate("Allows Poacher to use a backspear like they could earlier in the mod's development") 
            }
            );
    }
    public void PoacherOpTabOther()
    {
        OpTabPoacher.AddItems(
            new OpLabel(charcolumn + (columnMult * 1.96f),row + 25, "Other", true) { alpha = 0.5f },
            new OpCheckboxLabelled(PoacherPupActs, charcolumn + (columnMult * 1.96f), row, "Pup Acts") 
            { 
                description =  
                    Translate("Allows Poacher to have food preferences and reactions") 
            },
            new OpCheckboxLabelled(PoacherFreezeFaster, charcolumn + (columnMult * 1.96f), row-(rowMult),"Legacy Cold") 
            { 
                description =
                    Translate("Makes Poacher get cold as fast as they did in earlier versions of the mod") 
            }
            );
    }
    

}