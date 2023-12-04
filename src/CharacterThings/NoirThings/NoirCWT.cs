using System.Runtime.CompilerServices;

namespace TheFriend.NoirThings;

public static class NoirCWT
{
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