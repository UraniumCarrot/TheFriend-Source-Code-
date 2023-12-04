using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;
using UnityEngine;

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
        public Poacher(Player player)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<Player, Poacher> CWT = new();
    public static Poacher GetPoacher(this Player player) => CWT.GetValue(player, _ => new(player));

    public static bool TryGetPoacher(this Player player, out Poacher data)
    {
        if (player.SlugCatClass == Plugin.DragonName)
        {
            data = player.GetPoacher();
            return true;
        }
        data = null;
        return false;
    }
    
}