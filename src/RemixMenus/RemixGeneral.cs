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

    public static Configurable<bool> GeneralLocalizedLizRep;
    public static Configurable<bool> GeneralLocalizedLizRepForAll;
    
    public static Configurable<bool> GeneralLizRideAll;
    public static Configurable<bool> GeneralLizRepMeterForAll;
    
    //public static Configurable<bool> GeneralIntroRollBlizzard;
    //public static Configurable<bool> GeneralCharselectSnow;
    //public static Configurable<bool> GeneralCharCustomHeights;
    public static Configurable<bool> GeneralSolaceBlizzTimer;
    public static Configurable<bool> GeneralSolaceTitleCards;

    public static Configurable<string> pageValue;
    //public static Configurable<string> cosmetic;

    private static OpListBox page;
    //private static OpResourceSelector slugcatSettings;

    public List<UIelement> GenFamineList;
    public List<UIelement> GenLizardList;
    public List<UIelement> GenMiscelList;

    public void RemixGeneral()
    {
        GeneralNoFamine = config.Bind("SolaceNoFamine", false, new ConfigAcceptableList<bool>(true, false));
        GeneralFaminesForAll = config.Bind("SolaceFaminesForAll", false, new ConfigAcceptableList<bool>(true, false));
        
        GeneralLocalizedLizRep = config.Bind("SolaceLocalizedLizRep", true, new ConfigAcceptableList<bool>(true, false));
        GeneralLocalizedLizRepForAll = config.Bind("SolaceLocalizedLizRepForAll", false, new ConfigAcceptableList<bool>(true, false));
        
        GeneralLizRideAll = config.Bind("SolaceLizRideAll", false, new ConfigAcceptableList<bool>(true, false));
        GeneralLizRepMeterForAll = config.Bind("SolaceLizRepMeterForAll", false, new ConfigAcceptableList<bool>(true, false));
        
        //GeneralIntroRollBlizzard = config.Bind("SolaceIntroRollBlizzard", true, new ConfigAcceptableList<bool>(true, false));
        //GeneralCharselectSnow = config.Bind("SolaceCharselectSnow", true, new ConfigAcceptableList<bool>(true, false));
        //GeneralCharCustomHeights = config.Bind("SolaceCharHeights", false, new ConfigAcceptableList<bool>(true, false));
        GeneralSolaceBlizzTimer = config.Bind("SolaceBlizzTimer", false, new ConfigAcceptableList<bool>(true, false));
        GeneralSolaceTitleCards = config.Bind("SolaceTitleCards", true, new ConfigAcceptableList<bool>(true, false));

        pageValue = config.Bind<string>(null, "Famines");
        //cosmetic = config.Bind<string>(null, "Survivor");
    }
    
    public void RemixGeneralUpdate()
    {
        GenMiscelList.ForEach(x => { if (page.value == StrMiscel) x?.Show(); else x?.Hide();});
        GenLizardList.ForEach(x => { if (page.value == StrLizard) x?.Show(); else x?.Hide();});
        GenFamineList.ForEach(x => { if (page.value == StrFamine) x?.Show(); else x?.Hide();});
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

        //slugcatSettings = new OpResourceSelector(
        //    cosmetic, 
        //    new Vector2(gencolumn + (columnMult - 30), row-(rowMult*12)), 
        //    100,
        //    (OpResourceSelector.SpecialEnum)6);
        
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

        OpTabGeneral.AddItems(page,/*slugcatSettings,*/
            new OpLabel(gencolumn,row+25,"Universal",true) { alpha = 0.8f }
            //new OpLabel(
            //        gencolumn,
            //        row-(rowMult*12),  
            //        "Editing settings for:", true) 
            //    { alpha = 0.8f }
            );
        //var rowseparator1 = MakeLine(new Vector2(gencolumn*2.72f,row-(rowMult*11)), false);
        //GenSprites.container.AddChild(rowseparator1);

        GeneralFamine();
        GeneralMiscel();
        GeneralLizard();
        
        foreach (UIelement elem in GenLizardList) if (elem.tab != OpTabGeneral) OpTabGeneral.AddItems(elem);
        foreach (UIelement elem in GenFamineList) if (elem.tab != OpTabGeneral) OpTabGeneral.AddItems(elem);
        foreach (UIelement elem in GenMiscelList) if (elem.tab != OpTabGeneral) OpTabGeneral.AddItems(elem);
    }

    public void GeneralFamine()
    { // TODO: Test NoFamine
        GenFamineList.AddRange(new UIelement[]
        {
            new OpCheckboxLabelled(GeneralFaminesForAll, gencolumn, row, "Global Famines")
            {
                description =
                    Translate("Gives famine mechanics and creatures to every character")
            },
            new OpCheckboxLabelled(GeneralNoFamine, gencolumn, row-(rowMult), "Famine Disable")
            {
                description =
                    Translate("Stops famine mechanics in all modes except for Expedition mode when using the Famished burden, may be broken")
            }
        });
        foreach (UIelement elem in GenFamineList.ToArray())
            if (elem is OpCheckboxLabelled item && !GenFamineList.Contains(item.Label))
                GenFamineList.Add(item.Label);
    }

    public void GeneralMiscel()
    { // TODO: implement these
        GenMiscelList.AddRange(new UIelement[]
        {
            //new OpCheckboxLabelled(GeneralCharselectSnow, gencolumn, row, "Snow in Character Select")
            //{
            //    description =
            //        Translate("Solace characters (or characters in cold areas) have snow on their Character Select page")
            //},
            //new OpCheckboxLabelled(GeneralIntroRollBlizzard, gencolumn, row-(rowMult), "Intro Roll Blizzard")
            //{
            //    description =
            //        Translate("Replaces the intro's rain with a blizzard effect")
            //},
            //new OpCheckboxLabelled(GeneralCharCustomHeights, gencolumn, row-(rowMult*2), "Custom Height")
            //{
            //    description =
            //        Translate("Gives some characters a custom height. Disable if physics break too much")
            //},
            new OpCheckboxLabelled(GeneralSolaceBlizzTimer, gencolumn, row, "Blizzard Cycle Timer")
            {
                description =
                    Translate("Makes Saint and Solace characters able to see the cycle timer")
            },
            new OpCheckboxLabelled(GeneralSolaceTitleCards, gencolumn, row-(rowMult), "Solace Title Cards")
            {
                description =
                    Translate("Replaces all Downpour title cards with Solace ones")
            }
        });
        foreach (UIelement elem in GenMiscelList.ToArray())
            if (elem is OpCheckboxLabelled item && !GenMiscelList.Contains(item.Label))
                GenMiscelList.Add(item.Label);
    }

    public void GeneralLizard()
    {
        GenLizardList.AddRange(new UIelement[]
        {
            new OpCheckboxLabelled(GeneralLizRideAll, gencolumn, row, "Global Rides")
            {
                description =
                    Translate("Allows any lizard to be ridden if tamed")
            },
            new OpCheckboxLabelled(GeneralLizRepMeterForAll, gencolumn, row-(rowMult), "Universal Reputation Meter")
            {
                description =
                    Translate("Shows all characters, not just Solace ones, their lizard reputation in the current region")
            },
            new OpCheckboxLabelled(GeneralLocalizedLizRepForAll, gencolumn, row-(rowMult*2)-25, "Localized Reputation")
            {
                description =
                    Translate("Makes all characters, not just Solace ones, have lizard reputation that is kept entirely within the current region")
            }
        });
        foreach (UIelement elem in GenLizardList.ToArray())
            if (elem is OpCheckboxLabelled item && !GenLizardList.Contains(item.Label))
                GenLizardList.Add(item.Label);
    }
}