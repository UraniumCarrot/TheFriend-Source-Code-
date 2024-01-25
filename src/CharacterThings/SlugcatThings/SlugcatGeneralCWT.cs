using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;
using OverseerHolograms;
using TheFriend.Creatures.LizardThings.DragonRideThings;
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
        public GeneralCWT(Player player)
        {
            UnchangedInputForLizRide = new Player.InputPackage[player.input.Length];
            riderAnimChangeTimer = 20;
        }
    }
    
    public static readonly ConditionalWeakTable<Player, GeneralCWT> CWT = new();
    public static GeneralCWT GetGeneral(this Player player) => CWT.GetValue(player, _ => new(player));
}