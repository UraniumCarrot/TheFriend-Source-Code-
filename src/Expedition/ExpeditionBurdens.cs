using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Expedition;
using Menu;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using SlugBase;
using UnityEngine;

namespace TheFriend.Expedition;

public class ExpeditionBurdens
{
    public const string famine = "bur-famined";

    #region ExpeditionProgression
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
            float score = 0;
            if (SlugBaseCharacter.TryGet(player, out var chara) && 
                SlugBase.Features.PlayerFeatures.Diet.TryGet(chara, out var diet))
            {
                if (diet.Corpses > 0 || 
                    diet.CreatureOverrides.TryGetValue(CreatureTemplate.Type.LizardTemplate, out var liz) && liz > 0)
                    score = 10f;
                if (diet.Meat <= 0 && diet.Corpses <= 0)
                    score = 120f; // vegetarians who use this burden are SCREWED lol. if they beat it, they've earned it
            }
            
            switch (player.value)
            {
                case nameof(MoreSlugcatsEnums.SlugcatStatsName.Gourmand): score = 40f; break;
                case nameof(MoreSlugcatsEnums.SlugcatStatsName.Artificer): score = 10f; break;
                case nameof(MoreSlugcatsEnums.SlugcatStatsName.Spear): score = 5f; break;
                case nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint): score = 120f; break;
                case nameof(SlugcatStats.Name.Red): score = 10f; break;
            }
            if (score == 0) score = 80f;
            return score;
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
        if (key == famine) return ExpeditionProgression.IGT.Translate("FAMISHED");
        return orig(key);
    }
    #endregion
    #region UnlockDialog
    public static void UnlockDialogOnSetUpBurdenDescriptions(On.Menu.UnlockDialog.orig_SetUpBurdenDescriptions orig, UnlockDialog self)
    {
        orig(self);
        var a = ExpeditionProgression.BurdenName(famine) + " +" +
                ExpeditionProgression.BurdenScoreMultiplier(famine) +
                "%";
        
        self.burdenNames.Add(a);
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
    public static void UnlockDialogOnUpdate(On.Menu.UnlockDialog.orig_Update orig, UnlockDialog self)
    {
        orig(self);
        var burden = self.GetBurden();
        if (burden.faminedBurden.Selected || burden.faminedBurden.IsMouseOverMe)
        {
            self.perkNameLabel.text = self.burdenNames
                [self.burdenNames.IndexOf(self.burdenNames.Find(i => i.Contains("FAMISHED")))];
            self.perkDescLabel.text = self.burdenDescriptions
                [self.burdenDescriptions.IndexOf(self.burdenDescriptions.Find(i => i.Contains("famine")))];
        }
    }
    public static void UnlockDialogOnctor(On.Menu.UnlockDialog.orig_ctor orig, UnlockDialog self, ProcessManager manager, ChallengeSelectPage owner)
    {
        orig(self, manager, owner);
        self.GetBurden().faminedBurden.nextSelectable[3] = self.cancelButton;
    }
    public static void UnlockDialogOnctor(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(x => x.MatchStfld<UnlockDialog>(nameof(UnlockDialog.blindedBurden)));
        c.GotoPrev(MoveType.After, x => x.MatchLdcI4(1));
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((UnlockDialog self) =>
        {
            Vector2 vector = new Vector2(680f - (ModManager.MSC ? 325f : 248f), 310f);
            float spot = 170f;
            self.GetBurden().faminedBurden = new BigSimpleButton(
                self, 
                self.pages[0], 
                self.Translate(ExpeditionProgression.BurdenName(famine)), 
                famine, 
                vector + new Vector2(-spot,-15f), 
                new Vector2(150f, 50f), 
                FLabelAlignment.Center, 
                bigText: true);
            self.GetBurden().faminedBurden.buttonBehav.greyedOut = !ExpeditionData.unlockables.Contains(famine);
            self.pages[0].subObjects.Add(self.GetBurden().faminedBurden);
        });
    }
    #endregion
    
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
