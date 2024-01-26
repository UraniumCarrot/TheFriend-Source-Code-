using System.Runtime.CompilerServices;
using MoreSlugcats;

namespace TheFriend.PoacherThings;

public static class PoacherCWT
{
    public class Poacher
    {
        public int flicker;
        public int favoriteFoodTimer;
        public int sleepCounter;
        public bool IsSkullVisible;
        public bool IsInIntro;
        public bool isMakingPoppers;
        public Poacher(AbstractCreature player)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<AbstractCreature, Poacher> CWT = new();
    public static Poacher GetPoacher(this Player player) => CWT.GetValue(player.abstractCreature, _ => new(player.abstractCreature));
    
    public static bool TryGetPoacher(this AbstractCreature crit, out Poacher data)
    {
        var template = crit.creatureTemplate.type;
        if (template == CreatureTemplate.Type.Slugcat || template == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            if (crit.realizedCreature is Player player && player.SlugCatClass == Plugin.DragonName)
            {
                data = player.GetPoacher();
                return true;
            }
        data = null;
        return false;
    }
    public static bool TryGetPoacher(this Player player, out Poacher data) => player.abstractCreature.TryGetPoacher(out data);
    public static bool TryGetPoacher(this Creature player, out Poacher data) => player.abstractCreature.TryGetPoacher(out data);
}