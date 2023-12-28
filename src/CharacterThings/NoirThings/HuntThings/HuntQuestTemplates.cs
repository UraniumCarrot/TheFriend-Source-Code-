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
                    quests.AddRange(Ingredients0.Select(ingredient => new HuntQuest(ingredient.ToList())));
                    return quests;
                case 1:
                    quests.AddRange(Ingredients1.Select(ingredient => new HuntQuest(ingredient.ToList())));
                    return quests;
                case 2:
                    //return FromIngredientDictionary(Ingredients2);
                    quests.AddRange(Ingredients2Alt.Select(ingredient => new HuntQuest(ingredient.ToList())));
                    return quests;
            }

            //Something went wrong
            Plugin.LogSource.LogError("HuntQuest: Invalid karma, returning default template");
            return new List<HuntQuest>() { new HuntQuest(new List<CreatureTemplate.Type>() { CreatureTemplate.Type.LanternMouse }) };
        }


        //Karma 1, Progression to Karma 2
        public static List<CreatureTemplate.Type[]> Ingredients0 = new List<CreatureTemplate.Type[]>()
        {
            new[] { HuntQuest.HuntCicada },
            new[] { CreatureTemplate.Type.SmallCentipede },
            //new[] { CreatureTemplate.Type.SmallNeedleWorm },
            //new[] { CreatureTemplate.Type.LanternMouse },
            //new[] { HuntQuest.HuntEggbug },
        };

        //Karma 2, Progression to Karma 3
        public static List<CreatureTemplate.Type[]> Ingredients1 = new List<CreatureTemplate.Type[]>()
        {
            new []{ CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.SmallCentipede, },
            new []{ HuntQuest.HuntCicada, HuntQuest.HuntCicada, },
            new []{ HuntQuest.HuntCicada, CreatureTemplate.Type.SmallCentipede, },
            new []{ HuntQuest.HuntCentipede, CreatureTemplate.Type.SmallCentipede, },
            new []{ CreatureTemplate.Type.YellowLizard, }
        };

        //Karma 3, Progression to Karma 4
        public static Dictionary<CreatureTemplate.Type[], int> Ingredients2 = new Dictionary<CreatureTemplate.Type[], int>()
        {
            { new []{ CreatureTemplate.Type.JetFish }, 2 },
            { new []{ CreatureTemplate.Type.LanternMouse }, 4 },
            { new []{ HuntQuest.HuntCicada }, 2 },
            { new []{ HuntQuest.HuntCentipede }, 2 },
            { new []{ CreatureTemplate.Type.YellowLizard }, 1 },
            { new []{ CreatureTemplate.Type.Salamander }, 1 },
            { new []{ HuntQuest.HuntSpider }, 1 },
            { new []{ CreatureTemplateType.SnowSpider }, 1 },
        };

        public static List<HuntQuest> FromIngredientDictionary(Dictionary<CreatureTemplate.Type[], int> dictionary)
        {
            var templates = new List<CreatureTemplate.Type>[] { new(), new(), new(), new(), new() };
            var validIndexes = new int[] { 0, 1, 2, 3, 4 };
            var filledIndexes = new List<int>();

            while (dictionary.Keys.Any())
            {
                foreach (var item in dictionary.ToArray())
                {
                    var rand = UnityEngine.Random.Range(0, validIndexes.Except(filledIndexes).Count());
                    if (item.Value > 0)
                    {
                        templates[validIndexes[rand]].AddRange(item.Key);
                        dictionary[item.Key]--;
                        filledIndexes.Add(rand);
                    }
                    else
                    {
                        dictionary.Remove(item.Key);
                    }
                    if (filledIndexes.Count >= 5)
                        filledIndexes.Clear();
                }
            }

            return templates.Select(targets => new HuntQuest(targets)).ToList();
        }

        public static List<CreatureTemplate.Type[]> Ingredients2Alt = new List<CreatureTemplate.Type[]>()
        {
            new []{ CreatureTemplate.Type.JetFish, HuntQuest.HuntCicada, CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.LanternMouse },
            new []{ CreatureTemplate.Type.LanternMouse, HuntQuest.HuntSpider, HuntQuest.HuntCentipede },
            new []{ HuntQuest.HuntCicada, CreatureTemplate.Type.Centiwing, HuntQuest.HuntCentipede },
            new []{ CreatureTemplateType.SnowSpider, HuntQuest.HuntCentipede, CreatureTemplate.Type.YellowLizard },
            new []{ CreatureTemplate.Type.Salamander, CreatureTemplate.Type.JetFish },
        };

    }
}