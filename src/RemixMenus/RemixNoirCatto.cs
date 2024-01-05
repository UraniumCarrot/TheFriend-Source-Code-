using System.Linq;
using Menu.Remix.MixedUI;
using TheFriend.CharacterThings.NoirThings;
using TheFriend.RemixMenus.CustomRemixObjects;
using UnityEngine;

namespace TheFriend.RemixMenus;
public partial class RemixMain
{
    public static Configurable<bool> NoirAltSlashConditions;
    public static Configurable<bool> NoirBuffSlash;
    public static Configurable<bool> NoirAutoSlash;
    public static Configurable<bool> NoirDisableAutoCrouch;
    public static Configurable<NoirCatto.CustomStartMode> NoirUseCustomStart;
    public static Configurable<bool> NoirAttractiveMeow;
    public static Configurable<bool> NoirHideEars;
    public static Configurable<KeyCode> NoirMeowKey;
    
    public void RemixNoirCatto()
    {
        NoirAltSlashConditions = config.Bind(nameof(NoirAltSlashConditions), false);
        NoirBuffSlash = config.Bind(nameof(NoirBuffSlash), false);
        NoirAutoSlash = config.Bind(nameof(NoirAutoSlash), false);
        NoirDisableAutoCrouch = config.Bind(nameof(NoirDisableAutoCrouch), false);
        NoirUseCustomStart = config.Bind(nameof(NoirUseCustomStart), NoirCatto.CustomStartMode.Story);
        NoirAttractiveMeow = config.Bind(nameof(NoirAttractiveMeow), true);
        NoirHideEars = config.Bind(nameof(NoirHideEars), false);
        NoirMeowKey = config.Bind(nameof(NoirMeowKey), KeyCode.LeftAlt);
    }

    public void RemixNoirCattoUpdate()
    {
        
    }
    
    public OpTab OpTabNoirCatto;
    public OpContainer NoirCattoSprites;
    public void RemixNoirCattoInit()
    {
        OpTabNoirCatto = new OpTab(this, "Noir");
        NoirCattoSprites = new OpContainer(Vector2.zero);
        
        tabsList.Add(OpTabNoirCatto);
        OpTabNoirCatto.AddItems(NoirCattoSprites);
    }

    public void RemixNoirCattoLayout()
    {
        NoirOpTabMovement();
        NoirOpTabCombat();
        NoirOpTabOther();
        
        var columnseparator1 = MakeLine(new Vector2((column + (columnMult * 1.5f))/2,320), true);
        var columnseparator2 = MakeLine(new Vector2((column + (columnMult * 3.54f))/2,320), true);

        NoirCattoSprites.container.AddChild(
            new FSprite("symbolnoir")
            {
                scale = 1.4f,
                alpha = 0.05f,
                x = 300,
                y = 300
            });
        NoirCattoSprites.container.AddChild(columnseparator1);
        NoirCattoSprites.container.AddChild(columnseparator2);
    }
    
    public void NoirOpTabMovement()
    {
        OpTabNoirCatto.AddItems(
            new OpLabel(charcolumn, row + 25, "Movement", true) { alpha = 0.5f },
            new OpCheckboxLabelled(NoirDisableAutoCrouch, charcolumn, row, "Auto Crouch") 
            { 
                description =  
                    Translate("Stops NoirCatto from crouching after standing too long, prevented by holding Up") 
            }
        );
    }
    public void NoirOpTabCombat()
    {
        OpTabNoirCatto.AddItems(
            new OpLabel(charcolumn + (columnMult * 0.95f), row + 25, "Combat", true) { alpha = 0.5f },
            new OpCheckboxLabelled(NoirAutoSlash, charcolumn + (columnMult * 0.95f), row, "Auto Slash") 
            { 
                description =  
                    Translate("Auto-repeat slash when holding throw") 
            },
            new OpCheckboxLabelled(NoirBuffSlash, charcolumn + (columnMult * 0.95f), row-(rowMult),"Slash Buff") 
            { 
                description =
                    Translate("Buff NoirCatto's slash (stronger stun and damage)") 
            },
            new OpCheckboxLabelled(NoirAltSlashConditions, charcolumn + (columnMult * 0.95f), row-(rowMult*2),"Alt Slash") 
            { 
                description = 
                    Translate("Enables alternative slash conditions, which require NoirCatto's main hand to be empty") 
            },
            new OpCheckboxLabelled(NoirAttractiveMeow, charcolumn + (columnMult * 0.95f), row-(rowMult*3),"Attractive Meows") 
            { 
                description = 
                    Translate("Makes creatures react to NoirCatto's meows") 
            },
            new OpKeyBinderLabelled(NoirMeowKey,new Vector2(charcolumn + (columnMult * 0.92f), row-(rowMult*10)), new Vector2(150f, 30f), "Meow Key")
            {
                description = 
                    Translate("Configure what key makes NoirCatto meow")
            }
        );
    }
    public void NoirOpTabOther()
    {
        OpTabNoirCatto.AddItems(
            new OpLabel(charcolumn + (columnMult * 1.96f),row + 25, "Other", true) { alpha = 0.5f },
            new OpCheckboxLabelled(NoirHideEars, charcolumn + (columnMult * 1.96f), row-(rowMult),"Disable Cosmetics") 
            { 
                description =
                    Translate("Disables NoirCatto's extra sprite cosmetics, for use with DMS") 
            },
            new OpComboBoxSolid(NoirUseCustomStart, new Vector2(charcolumn + (columnMult * 1.86f),row), 150, OpResourceSelector.GetEnumNames(null, typeof(NoirCatto.CustomStartMode)).ToList())
            { 
                description = 
                    Translate("Choose whether NoirCatto's intro should be used in Story and Expedition modes, disable if broken")
            }
        );
    }

}