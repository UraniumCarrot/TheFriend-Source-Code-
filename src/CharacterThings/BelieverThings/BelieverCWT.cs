using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;
using UnityEngine;

namespace TheFriend.CharacterThings.BelieverThings;

public static class BelieverCWT
{
    public class Believer
    {
        public Believer(Player player)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<Player, Believer> CWT = new();
    public static Believer GetBeliever(this Player player) => CWT.GetValue(player, _ => new(player));

    public static bool TryGetBeliever(this Player player, out Believer data)
    {
        if (player.SlugCatClass == Plugin.BelieverName)
        {
            data = player.GetBeliever();
            return true;
        }
        data = null;
        return false;
    }
    
}