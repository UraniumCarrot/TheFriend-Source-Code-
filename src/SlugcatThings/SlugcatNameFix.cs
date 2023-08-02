namespace TheFriend.SlugcatThings;
using static Plugin;

public class SlugcatNameFix
{
    public static void Apply()
    {
        On.SlugcatStats.getSlugcatName += SlugcatStatsOngetSlugcatName;
        On.Menu.Menu.Translate_string += MenuOnTranslate_string;
    }

    private static string SlugcatStatsOngetSlugcatName(On.SlugcatStats.orig_getSlugcatName orig, SlugcatStats.Name name)
    {
        if (name == NoirName) return "Noir";
        return orig(name);
    }
    private static string MenuOnTranslate_string(On.Menu.Menu.orig_Translate_string orig, Menu.Menu self, string s)
    {
        if (s == "The Noir") return "The Stalker";
        return orig(self, s);
    }
}