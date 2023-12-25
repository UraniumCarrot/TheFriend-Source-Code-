using System.Collections.Generic;
using System.Linq;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    public static class HuntQuestTemplates
    {
        public static List<HuntQuest> FromKarma(int karma)
        {
            var quests = new List<HuntQuest>();
            switch (karma)
            {
                case 0:
                    foreach (var ingredient in Ingredients0)
                    {
                        quests.Add(new HuntQuest(ingredient.ToList()));
                    }
                    return quests;
            }

            //Something went wrong
            Plugin.LogSource.LogError("HuntQuest: Invalid karma, returning default template");
            return new List<HuntQuest>() { new HuntQuest(new List<CreatureTemplate.Type>() { CreatureTemplate.Type.VultureGrub }) };
        }


        public static List<CreatureTemplate.Type[]> Ingredients0 = new List<CreatureTemplate.Type[]>()
        {
            // new[] { CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.SmallCentipede },
            // new[] { CreatureTemplate.Type.SmallNeedleWorm, CreatureTemplate.Type.SmallNeedleWorm },
            // new[] { CreatureTemplate.Type.LanternMouse },
            // new[] { HuntQuest.HuntCicada },
            // new[] { HuntQuest.HuntEggbug },

            new []{ CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.SmallNeedleWorm, CreatureTemplate.Type.LanternMouse },
            new []{ HuntQuest.HuntCicada, CreatureTemplate.Type.SmallCentipede, HuntQuest.HuntEggbug },
            new []{ CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.JetFish },
            new []{ CreatureTemplate.Type.SmallCentipede }
        };


    }
}