using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Expedition;
using Menu;
using Menu.Remix.MixedUI;
using MonoMod.Cil;
using RWCustom;
using SlugBase;
using UnityEngine;

namespace TheFriend.Expedition;

public class ExpeditionBurdens
{
    public const string famine = "bur-famined";

    public static void Apply()
    {
        On.Menu.UnlockDialog.UpdateBurdens += UnlockDialogOnUpdateBurdens;
        On.Menu.UnlockDialog.SetUpBurdenDescriptions += UnlockDialogOnSetUpBurdenDescriptions;
        On.Menu.UnlockDialog.ctor += UnlockDialogOnctor;
        IL.Menu.UnlockDialog.ctor += UnlockDialogOnctor;
        
        On.Expedition.ExpeditionProgression.BurdenMenuColor += ExpeditionProgressionOnBurdenMenuColor;
        On.Expedition.ExpeditionProgression.SetupBurdenGroups += ExpeditionProgressionOnSetupBurdenGroups;
        On.Expedition.ExpeditionProgression.BurdenName += ExpeditionProgressionOnBurdenName;
        On.Expedition.ExpeditionProgression.BurdenManualDescription += ExpeditionProgressionOnBurdenManualDescription;
        On.Expedition.ExpeditionProgression.BurdenScoreMultiplier += ExpeditionProgressionOnBurdenScoreMultiplier;
        On.Expedition.ExpeditionProgression.CountUnlockables += ExpeditionProgressionOnCountUnlockables;
    }

    public static void ExpeditionProgressionOnCountUnlockables(On.Expedition.ExpeditionProgression.orig_CountUnlockables orig)
    {
        ExpeditionProgression.totalBurdens += ExpeditionProgression.burdenGroups["solace"].Count;
        orig();
    }

    public static float ExpeditionProgressionOnBurdenScoreMultiplier(On.Expedition.ExpeditionProgression.orig_BurdenScoreMultiplier orig, string key)
    {
        var player = ExpeditionData.slugcatPlayer;
        if (key == famine)
        { // If this character can eat corpses, the score bonus takes a big penalty
            if (SlugBaseCharacter.TryGet(player, out var chara) && 
                SlugBase.Features.PlayerFeatures.Diet.TryGet(chara, out var diet))
            {
                if (diet.Corpses > 0 || 
                    diet.CreatureOverrides.TryGetValue(CreatureTemplate.Type.LizardTemplate, out var liz) && liz > 0)
                    return 10f;
                if (diet.Meat <= 0)
                    return 120f; // vegetarians who use this burden are SCREWED lol. if they beat it, they've earned it
            }
            
            switch (player.value.ToLower())
            {
                case "gourmand": return 40f;
                case "artificer": return 10f;
                case "red" or "hunter": return 10f;
                case "spearmaster": return 5f;
                case "saint": return 120f;
            }
            return 80f;
        }
        return orig(key);
    }
    public static string ExpeditionProgressionOnBurdenManualDescription(On.Expedition.ExpeditionProgression.orig_BurdenManualDescription orig, string key)
    {
        if (key == famine) return ExpeditionProgression.IGT.Translate(
            "Plunges the land in devestating famine, making many food sources less nutritious. Starvation may become a great familiarity."
            );
        return orig(key);
    }
    public static Color ExpeditionProgressionOnBurdenMenuColor(On.Expedition.ExpeditionProgression.orig_BurdenMenuColor orig, string key)
    {
        if (key == famine) return new Color(0.5f, 0.4f, 0.2f);
        return orig(key);
    }
    public static void ExpeditionProgressionOnSetupBurdenGroups(On.Expedition.ExpeditionProgression.orig_SetupBurdenGroups orig)
    {
        orig();
        List<string> list = new List<string> { famine };
        ExpeditionProgression.burdenGroups.Add("solace", list);
    }
    public static string ExpeditionProgressionOnBurdenName(On.Expedition.ExpeditionProgression.orig_BurdenName orig, string key)
    {
        if (key == famine) return ExpeditionProgression.IGT.Translate("STARVED");
        return orig(key);
    }
    public static void UnlockDialogOnSetUpBurdenDescriptions(On.Menu.UnlockDialog.orig_SetUpBurdenDescriptions orig, UnlockDialog self)
    {
        orig(self);
        self.burdenNames.Add(ExpeditionProgression.BurdenName(famine) + 
                             " +" + 
                             ExpeditionProgression.BurdenScoreMultiplier(famine) + 
                             "%");
        self.burdenDescriptions.Add(ExpeditionData.unlockables.Contains(famine) ? 
            ExpeditionProgression.BurdenManualDescription(famine).WrapText(bigText: false, 600f) : 
            "? ? ?");
    }
    public static void UnlockDialogOnUpdateBurdens(On.Menu.UnlockDialog.orig_UpdateBurdens orig, UnlockDialog self)
    {
        if (ExpeditionGame.activeUnlocks.Contains(famine))
        {
            Vector3 color = Custom.RGB2HSL(ExpeditionProgression.BurdenMenuColor(famine));
            self.GetBurden().faminedBurden.labelColor = new HSLColor(color.x, color.y, color.z);
        }
        else self.GetBurden().faminedBurden.labelColor = new HSLColor(1f,0f,0.35f);
        orig(self);
    }
    
    
    public static void UnlockDialogOnctor(On.Menu.UnlockDialog.orig_ctor orig, UnlockDialog self, ProcessManager manager, ChallengeSelectPage owner)
    { // this needs an IL hook to work
        self.GetBurden().faminedBurden = new BigSimpleButton(
            self, 
            self.pages[0], 
            self.Translate(ExpeditionProgression.BurdenName(famine)), 
            famine, 
            Vector2.zero, 
            new Vector2(150f, 50f), 
            FLabelAlignment.Center, 
            bigText: true);
        self.GetBurden().faminedBurden.buttonBehav.greyedOut = !ExpeditionData.unlockables.Contains(famine);
        self.pages[0].subObjects.Add(self.GetBurden().faminedBurden);
        orig(self, manager, owner);
        self.GetBurden().faminedBurden.nextSelectable[3] = self.cancelButton;
    }
    public static void UnlockDialogOnctor(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext();
    }
}

public static class DialogueBurden // Adds new button
{
    public class BurdenHolder
    {
        public BigSimpleButton faminedBurden;
        public BurdenHolder(UnlockDialog self)
        {
        }
    }
    public static readonly ConditionalWeakTable<UnlockDialog, BurdenHolder> CWT = new();
    public static BurdenHolder GetBurden(this UnlockDialog dialogue) => CWT.GetValue(dialogue, _ => new(dialogue));
}
