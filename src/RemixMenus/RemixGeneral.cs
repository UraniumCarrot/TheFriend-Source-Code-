using System.Collections.Generic;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using TheFriend.RemixMenus.CustomRemixObjects;
using UnityEngine;

namespace TheFriend.RemixMenus;
public partial class RemixMain
{
    public static Configurable<bool> GeneralNoFamine;
    public static Configurable<bool> GeneralFaminesForAll;
    public static Configurable<bool> GeneralExpeditionFamine;

    public static Configurable<bool> GeneralLocalizedLizRep;
    public static Configurable<bool> GeneralLocalizedLizRepForAll;
    
    public static Configurable<bool> GeneralSolaceBlizzTimer;
    
    public static Configurable<bool> GeneralLizRideAll;
    public static Configurable<bool> GeneralLizRepMeterForAll;
    
    public static Configurable<bool> GeneralIntroRollBlizzard;
    public static Configurable<bool> GeneralCharselectSnow;
    public static Configurable<bool> GeneralCharCustomHeights;

    public static Configurable<string> pageValue;
    public static Configurable<string> cosmetic;

    private static OpListBox page;
    private static OpResourceSelector slugcatSettings;

    public List<UIelement> GenFamineList;
    public List<UIelement> GenLizardList;
    public List<UIelement> GenMiscelList;

    public void RemixGeneral()
    {
        GeneralExpeditionFamine = config.Bind("SolaceExpeditionFamine", false, new ConfigAcceptableList<bool>(true, false));
        GeneralNoFamine = config.Bind("SolaceNoFamine", false, new ConfigAcceptableList<bool>(true, false));
        GeneralFaminesForAll = config.Bind("SolaceFaminesForAll", false, new ConfigAcceptableList<bool>(true, false));
        
        GeneralLocalizedLizRep = config.Bind("SolaceLocalizedLizRep", true, new ConfigAcceptableList<bool>(true, false));
        GeneralLocalizedLizRepForAll = config.Bind("SolaceLocalizedLizRepForAll", false, new ConfigAcceptableList<bool>(true, false));
        
        GeneralSolaceBlizzTimer = config.Bind("SolaceBlizzTimer", false, new ConfigAcceptableList<bool>(true, false));
        
        GeneralLizRideAll = config.Bind("SolaceLizRideAll", false, new ConfigAcceptableList<bool>(true, false));
        GeneralLizRepMeterForAll = config.Bind("SolaceLizRepMeterForAll", false, new ConfigAcceptableList<bool>(true, false));
        
        GeneralIntroRollBlizzard = config.Bind("SolaceIntroRollBlizzard", true, new ConfigAcceptableList<bool>(true, false));
        GeneralCharselectSnow = config.Bind("SolaceCharselectSnow", true, new ConfigAcceptableList<bool>(true, false));
        GeneralCharCustomHeights = config.Bind("SolaceCharHeights", false, new ConfigAcceptableList<bool>(true, false));

        pageValue = config.Bind<string>(null, "Lizards");
        cosmetic = config.Bind<string>(null, "Survivor");
    }
    
    public void RemixGeneralUpdate()
    {
        Debug.Log("slugcatSettings value: " + slugcatSettings.value + ", _value: " + slugcatSettings._value + ", labeltext: " + slugcatSettings._lblText + ", labellist" + slugcatSettings._lblList);

        
        
        if (GenMiscelList.Count > 0)
        {
            foreach (UIelement elem in GenMiscelList)
            {
                if (page._value == StrMiscel) elem.Show();
                else elem.Hide();
            }
        }
        if (GenLizardList.Count > 0)
        {
            foreach (UIelement elem in GenLizardList)
            {
                if (page._value == StrLizard) elem.Show();
                else elem.Hide();
            }
        }
        if (GenFamineList.Count > 0)
        {
            foreach (UIelement elem in GenFamineList)
            {
                if (page._value == StrFamine) elem.Show();
                else elem.Hide();
            }
        }
    }

    public OpTab OpTabGeneral;
    public OpContainer GenSprites;

    public void RemixGeneralInit()
    { 
        OpTabGeneral = new OpTab(this, "General");
        GenSprites = new OpContainer(Vector2.zero);
        
        page = new OpListBox(
            pageValue,
            new Vector2(column-30, row-20),
            100,
            new List<ListItem>
            {
                new ListItem(StrLizard),
                new ListItem(StrFamine),
                new ListItem(StrMiscel)
            });

        slugcatSettings = new OpResourceSelector(
            cosmetic, 
            new Vector2(gencolumn + (columnMult - 30), row + 25), 
            100,
            (OpResourceSelector.SpecialEnum)6);
        
        tabsList.Add(OpTabGeneral);
        OpTabGeneral.AddItems(GenSprites);
    }

    public const float gencolumn = column + 80;
    public const string StrLizard = "Lizards";
    public const string StrMiscel = "Misc";
    public const string StrFamine = "Famines";
    
    public void RemixGeneralLayout()
    {
        //GenSprites.container.AddChild();

        OpTabGeneral.AddItems(page,slugcatSettings,
            new OpLabel(gencolumn,row-(rowMult*4),"Global",true) { alpha = 0.5f }
            );

        GeneralFamine();
        GeneralMiscel();
        GeneralLizard();
        
        foreach (UIelement elem in GenLizardList) if (elem.tab != OpTabGeneral) OpTabGeneral.AddItems(elem);
        foreach (UIelement elem in GenFamineList) if (elem.tab != OpTabGeneral) OpTabGeneral.AddItems(elem);
        foreach (UIelement elem in GenMiscelList) if (elem.tab != OpTabGeneral) OpTabGeneral.AddItems(elem);
    }

    public void GeneralFamine()
    {
        GenFamineList.AddRange(new UIelement[]
        {
            
        });
        foreach (UIelement elem in GenFamineList.ToArray())
            if (elem is OpCheckboxLabelled item && !GenFamineList.Contains(item.Label))
                GenFamineList.Add(item.Label);
    }

    public void GeneralMiscel()
    {
        GenMiscelList.AddRange(new UIelement[]
        {
            
        });
        foreach (UIelement elem in GenMiscelList.ToArray())
            if (elem is OpCheckboxLabelled item && !GenMiscelList.Contains(item.Label))
                GenMiscelList.Add(item.Label);
    }

    public void GeneralLizard()
    {
        GenLizardList.AddRange(new UIelement[]
        {
            new OpLabel(
                    gencolumn,
                    row + 25,  
                    "Editing settings for:", true) 
                { alpha = 0.5f },
            new OpCheckboxLabelled(GeneralLizRideAll, gencolumn, row, "Global Rides", 0)
            {
                description =
                    Translate("Allows all lizards to be ridden if tamed (excluding Young and Mother lizards)"),
            }
        });
        foreach (UIelement elem in GenLizardList.ToArray())
            if (elem is OpCheckboxLabelled item && !GenLizardList.Contains(item.Label))
                GenLizardList.Add(item.Label);
    }
    
    
    
    
    
    
    
}