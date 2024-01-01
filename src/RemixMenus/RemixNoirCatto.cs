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
        
        var columnseparator1 = MakeLine(new Vector2((column + (columnMult * 1.5f))/2,300), true);
        var columnseparator2 = MakeLine(new Vector2((column + (columnMult * 3.54f))/2,300), true);

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
            new OpLabel(charcolumn, row + 25, "Movement", true) { alpha = 0.5f }
        );
    }
    public void NoirOpTabCombat()
    {
        OpTabNoirCatto.AddItems(
            new OpLabel(charcolumn + (columnMult * 0.95f), row + 25, "Combat", true) { alpha = 0.5f }
        );
    }
    public void NoirOpTabOther()
    {
        OpTabNoirCatto.AddItems(
            new OpLabel(charcolumn + (columnMult * 1.96f),row + 25, "Other", true) { alpha = 0.5f },
            new OpComboBoxSolid(NoirUseCustomStart, new Vector2(charcolumn + (columnMult * 1.96f),row), 50, OpResourceSelector.GetEnumNames(null, typeof(NoirCatto.CustomStartMode)).ToList())
            { 
                description = 
                    Translate("Makes Noir use his custom intro. Disable if story mode breaks")
            }
        );
    }

}