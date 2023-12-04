using System.Runtime.CompilerServices;
using TheFriend.NoirThings;
using Color = UnityEngine.Color;
using UnityEngine;

namespace TheFriend.SlugcatThings;
public static class PoacherClass
{
    // Technically this class is meant to hold variables used only for Poacher, but there's nothing barring me from using it with players in general. So i will. :)
    public class Poacher
    {
        public Poacher(Player player)
        {
        }
    }
    public static readonly ConditionalWeakTable<Player, Poacher> CWT = new();
    public static Poacher GetPoached(this Player player) => CWT.GetValue(player, _ => new(player));
    
    // Noir's CWT
    public static readonly ConditionalWeakTable<Player, NoirCatto.NoirData> NoirDeets = new ConditionalWeakTable<Player, NoirCatto.NoirData>();
    public static NoirCatto.NoirData GetNoir(this Player player) => NoirDeets.GetValue(player, _ => new(player));
    public static bool TryGetNoir(this Player player, out NoirCatto.NoirData noirData)
    {
        if (player.SlugCatClass == Plugin.NoirName)
        {
            noirData = GetNoir(player);
            return true;
        }

        noirData = null;
        return false;
    }
}

public static class OverseerTracking
{
    public class Overseerer
    {
        public bool IAmAGuide;
        public Overseerer()
        {

        }
    }
    public static readonly ConditionalWeakTable<Overseer, Overseerer> CWT = new();
    public static Overseerer SeerData(this Overseer seer) => CWT.GetValue(seer, _ => new());

}
