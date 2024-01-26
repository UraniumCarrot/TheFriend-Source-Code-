using System;
using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;
using MoreSlugcats;
using TheFriend.Objects;
using UnityEngine;

namespace TheFriend.SlugcatThings;

public static class SlugcatGeneralCWT
{
    public class GeneralCWT
    {        
        // Graphical variables
        public int customSprite1;
        public int customSprite2;
        public int customSprite3;
        public Color customColor1;
        public Color customColor2;

        public Vector2 scarfPos;
        public float scarfRotation;
        public bool wearingAScarf;
        public FNode head;
        
        // General player variables
        public bool squint;
        public bool iHaveSenses;
        
        public bool JustGotMoonMark;
        public bool MoonMarkPassed;
        public int MarkExhaustion;
        public bool isRidingLizard;
        public float pointDir0;
        public float pointDir1;
        public int spearRotationTimer;
        public bool RainTimerExists;
        
        
        public readonly Player.InputPackage[] UnchangedInputForLizRide;
        public GenericObjectStick rideStick;
        public Lizard dragonSteed;
        public int riderAnimChangeTimer;
        public int glanceDir;
        public GeneralCWT(AbstractCreature crit)
        {
            var player = crit.realizedCreature as Player;
            UnchangedInputForLizRide = new Player.InputPackage[player.input.Length];
            riderAnimChangeTimer = 20;
        }
    }
    
    public static readonly ConditionalWeakTable<AbstractCreature, GeneralCWT> CWT = new();
    //public static GeneralCWT GetGeneral(this AbstractCreature crit) => CWT.GetValue(crit, _ => new(crit));
    public static GeneralCWT GetGeneral(this Player crit) => CWT.GetValue(crit.abstractCreature, _ => new GeneralCWT(crit.abstractCreature));
    
    public static bool TryGetGeneral(this AbstractCreature crit, out GeneralCWT data)
    {
        var template = crit.creatureTemplate.type;
        if (template == CreatureTemplate.Type.Slugcat || template == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            if (crit.realizedCreature is Player pl)
            {
                data = pl.GetGeneral();
                return true;
            }
        data = null;
        return false;
    }
    public static bool TryGetGeneral(this Player player, out GeneralCWT data) => player.abstractCreature.TryGetGeneral(out data);
    public static bool TryGetGeneral(this Creature player, out GeneralCWT data) => player.abstractCreature.TryGetGeneral(out data);
}