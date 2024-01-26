using System.Runtime.CompilerServices;
using MoreSlugcats;

namespace TheFriend.CharacterThings.DelugeThings;

public static class DelugeCWT
{
    public class Deluge
    {
        public DataPearl pearl;
        public PhysicalObject lookTarget;
        
        public bool PearlWasTaken;
        public bool sprinting;
        public bool siezing;
        public bool AmIIdling;

        public int sprintParticleTimer;
        public int GracePeriod;
        public int OverloadLooper;
        public int Overload;
        public int Exhaustion;
        public int Sieze;

        //TODO: Make these values configurable
        public const int SiezeLimit = 100; // How many ticks until Deluge siezes if they don't stop what they're doing
        public const int ExhaustionSiezeThreshold = 400; // The point Deluge will start gaining sieze
        public const int ExhaustionStillnessThreshold = 100; // The point Deluge has to stay still to regain stamina
        public const int ExhaustionLimit = 500; // When Deluge is incapable of sprinting more
        public const int OverloadLimit = 1000; // How much overload is allowed to build up over time
        public const int OverloadIntensity = 2; // How intense overload effects are allowed to get

        public Deluge(AbstractCreature player)
        {
            GracePeriod = 500;
        }
    }
    public static readonly ConditionalWeakTable<AbstractCreature, Deluge> CWT = new();
    public static Deluge GetDeluge(this Player player) => CWT.GetValue(player.abstractCreature, _ => new(player.abstractCreature));

    public static bool TryGetDeluge(this AbstractCreature crit, out Deluge data)
    {
        var template = crit.creatureTemplate.type;
        if (template == CreatureTemplate.Type.Slugcat || template == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            if (crit.realizedCreature is Player player && player.SlugCatClass == Plugin.DelugeName)
            {
                data = player.GetDeluge();
                return true;
            }
        data = null;
        return false;
    }
    public static bool TryGetDeluge(this Player player, out Deluge data) => player.abstractCreature.TryGetDeluge(out data);
    public static bool TryGetDeluge(this Creature player, out Deluge data) => player.abstractCreature.TryGetDeluge(out data);
}