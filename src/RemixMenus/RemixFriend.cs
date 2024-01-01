using System;
using System.Linq;
using Menu.Remix.MixedUI;
using UnityEngine;
using TheFriend.RemixMenus.CustomRemixObjects;

namespace TheFriend.RemixMenus;
public partial class RemixMain
{
    public static Configurable<bool> FriendAutoCrouch;
    public static Configurable<bool> FriendPoleCrawl;
    public static Configurable<bool> FriendBackspear;
    public static Configurable<bool> FriendUnNerf;
    public static Configurable<bool> FriendRepLock; 
    
    public void RemixFriend()
    {
        FriendAutoCrouch = config.Bind("FriendAutoCrouch", true, new ConfigAcceptableList<bool>(true, false));
        FriendPoleCrawl = config.Bind("FriendPoleCrawl", true, new ConfigAcceptableList<bool>(true, false));
        FriendUnNerf = config.Bind("FriendUnNerf", false, new ConfigAcceptableList<bool>(true, false));
        FriendBackspear = config.Bind("FriendBackspear", false, new ConfigAcceptableList<bool>(true, false));
        FriendRepLock = config.Bind("FriendRepLock", true, new ConfigAcceptableList<bool>(true, false));
    }

    public void RemixFriendUpdate()
    {
        
    }
    
    public OpTab OpTabFriend;
    public OpContainer FriendSprites;
    public void RemixFriendInit()
    {
        OpTabFriend = new OpTab(this, "Friend");
        FriendSprites = new OpContainer(Vector2.zero);
        
        tabsList.Add(OpTabFriend);
        OpTabFriend.AddItems(FriendSprites);
    }

    public void RemixFriendLayout()
    {
        FriendOpTabMovement();
        FriendOpTabCombat();
        FriendOpTabOther();

        var columnseparator1 = MakeLine(new Vector2((column + (columnMult * 1.5f))/2,300), true);
        var columnseparator2 = MakeLine(new Vector2((column + (columnMult * 3.54f))/2,300), true);

        FriendSprites.container.AddChild(
            new FSprite("symbolfriend")
            {
                scale = 1.45f,
                alpha = 0.05f,
                x = 300,
                y = 300
            }
        );
        FriendSprites.container.AddChild(columnseparator1);
        FriendSprites.container.AddChild(columnseparator2);
    }
    
    public void FriendOpTabMovement()
    {
        OpTabFriend.AddItems(
            new OpLabel(charcolumn, row + 25, "Movement", true) { alpha = 0.5f },
            
            new OpCheckboxLabelled(FriendUnNerf, charcolumn, row, "Legacy Stats", 0) 
            { 
                description =  
                Translate("Changes various Friend stats to be as they were earlier in the mod's development") 
            },
            new OpCheckboxLabelled(FriendAutoCrouch, charcolumn, row-(rowMult),"Auto Crouch", 0) 
            { 
                description =
                Translate("Makes Friend crouch automatically after a standing jump, prevented by holding Up") 
            },
            new OpCheckboxLabelled(FriendPoleCrawl, charcolumn, row-(rowMult*2),"Pole Crawl", 0) 
            { 
                description = 
                Translate("Allows Friend to crawl along the tops of poles") 
            }
        );
    }
    public void FriendOpTabCombat()
    {
        OpTabFriend.AddItems(
            new OpLabel(charcolumn + (columnMult * 0.95f), row + 25, "Combat", true) { alpha = 0.5f },
            
            new OpCheckboxLabelled(FriendBackspear, charcolumn + (columnMult * 0.95f), row, "Backspear Enable", 0)
            { 
                description = 
                Translate("Allows Friend to use a backspear like they could earlier in the mod's development") 
            }
        );
    }

    public void FriendOpTabOther()
    {
        OpTabFriend.AddItems(
            new OpLabel(charcolumn + (columnMult * 1.96f),row + 25, "Other", true) { alpha = 0.5f },
            
            new OpCheckboxLabelled(FriendRepLock, charcolumn + (columnMult * 1.96f), row, "Rep Lock", 0)
            { 
                description = 
                Translate("Stops changes to lizard reputation happening on cycle 0 for Friend. Lizard reputation may break if disabled")
            }
        );
    }
}