using System.Runtime.CompilerServices;
using MoreSlugcats;

namespace TheFriend.CharacterThings.BelieverThings;

public static class BelieverCWT
{
    public class Believer
    {
        public Believer(AbstractCreature player)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<AbstractCreature, Believer> CWT = new();
    public static Believer GetBeliever(this Player player) => CWT.GetValue(player.abstractCreature, _ => new(player.abstractCreature));

    public static bool TryGetBeliever(this AbstractCreature crit, out Believer data)
    {
        var template = crit.creatureTemplate.type;
        if (template == CreatureTemplate.Type.Slugcat || template == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            if (crit.realizedCreature is Player player && player.SlugCatClass == Plugin.BelieverName)
            {
                data = player.GetBeliever();
                return true;
            }
        data = null;
        return false;
    }
    public static bool TryGetBeliever(this Player player, out Believer data) => player.abstractCreature.TryGetBeliever(out data);
    public static bool TryGetBeliever(this Creature player, out Believer data) => player.abstractCreature.TryGetBeliever(out data);
}