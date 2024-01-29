using System.Collections.Generic;
using System.Linq;

using CT = CreatureTemplate.Type;
using static CreatureTemplate.Type;
using static MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType;
using static TheFriend.CreatureTemplateType;
using static TheFriend.CharacterThings.NoirThings.HuntThings.HuntQuestThings.HuntQuest;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    public static class HuntQuestTemplates
    {
        // TODO: balance requested threat
        public static List<HuntQuest> FromKarma(int karma)
        {
            var creatureList = karma switch
            {
                0 => GetFixedCreatureList(Karma2CreatureFixedList),
                1 => GetFixedCreatureList(Karma3CreatureFixedList),
                2 => GetRandomCreatureList(Karma4CreaturePool, 1.0f),
                3 => GetRandomCreatureList(Karma5CreaturePool, 2.0f),
                4 => GetRandomCreatureList(Karma6CreaturePool, 3.0f),
                5 => GetRandomCreatureList(Karma7CreaturePool, 4.0f),
                6 => GetFixedCreatureList(Karma8CreatureFixedList),
                7 => GetFixedCreatureList(Karma9CreatureFixedList),
                8 => GetFixedCreatureList(Karma10CreatureFixedList),
                _ => null
            };
            if (creatureList != null) return creatureList;
            
            //Something went wrong
            Plugin.LogSource.LogError("HuntQuest: Invalid karma, returning default template");
            return [new HuntQuest([CT.LanternMouse])];
        }

        private static List<HuntQuest> GetRandomCreatureList(List<Extensions.WeightedItem<CT>> creatureList, float requestedThreat, int maxCreatures = 10, int listCount = 4)
        {
            List<HuntQuest> outputList = [];
            for (var i = 0; i < listCount; i++)
            {
                var randomList = Extensions.GenerateRandomList(creatureList, maxCreatures, target: requestedThreat).ToList();
                randomList.Sort((a, b) =>
                {
                    // Move lizards to the end, so they are together
                    if (a.value.EndsWith("Lizard") && !b.value.EndsWith("Lizard"))
                        return 1;
                    if (!a.value.EndsWith("Lizard") && b.value.EndsWith("Lizard"))
                        return -1;
                    
                    // else just sort alphabetically
                    return a.value.CompareTo(b.value);
                });
                outputList.Add(new HuntQuest(randomList));
            }
            return outputList;
        }
        
        private static List<HuntQuest> GetFixedCreatureList(List<CT[]> creatureList)
        {
            return creatureList
                .Select(ingredient => new HuntQuest(ingredient.ToList()))
                .ToList();
        }


        /// Karma 1, Progression to Karma 2
        public static readonly List<CT[]> Karma2CreatureFixedList =
        [
            [HuntQuest.HuntCicada],
            [SmallCentipede],
            [HuntQuest.HuntEggBug]
            //[CT.SmallNeedleWorm], //Nonexistent in SI
            //[CT.LanternMouse], //Nonexistent in SI
        ];

        /// Karma 2, Progression to Karma 3
        public static readonly List<CT[]> Karma3CreatureFixedList =
        [
            [SmallCentipede, SmallCentipede, SmallCentipede],
            [HuntCicada, HuntCicada],
            [HuntEggBug, HuntCicada],
            [HuntEggBug, SmallCentipede],
            [YellowLizard]
        ];
        
        // Beginning of Random Creature Pools
        // "Contribution" values copied from their threat level (for those that have them), otherwise 0.10f
        // Everything is set to have the same weight for now.
        
        /// Karma 3, Progression to Karma 4
        public static readonly List<Extensions.WeightedItem<CT>> Karma4CreaturePool =
        [
            new() { Limit = 1, Weight = 10, Contribution = 0.10f, Item = CT.JetFish },
            new() { Limit = 1, Weight = 10, Contribution = 0.10f, Item = CT.LanternMouse },
            new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = HuntSpider },
            //new() { Limit = 1, Weight = 10, Contribution = 0.5f, Item = CT.BigNeedleWorm },
            new() { Limit = 2, Weight =  5, Contribution = 0.30f, Item = HuntCentipede },
            new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = YellowLizard },
            new() { Limit = 1, Weight = 10, Contribution = 0.45f, Item = PinkLizard },
            new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = Salamander }
        ];
        
        /// Karma 4, Progression to Karma 5
        public static readonly List<Extensions.WeightedItem<CT>> Karma5CreaturePool =
        [
            new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = CT.DropBug },
            new() { Limit = 1, Weight = 10, Contribution = 0.65f, Item = CT.Scavenger },
            new() { Limit = 1, Weight = 10, Contribution = 0.70f, Item = CT.Vulture },
            new() { Limit = 2, Weight =  5, Contribution = 0.40f, Item = HuntSpider },
            new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = YellowLizard },
            new() { Limit = 1, Weight = 10, Contribution = 0.45f, Item = PinkLizard },
            new() { Limit = 1, Weight = 10, Contribution = 0.35f, Item = BlueLizard },
            new() { Limit = 1, Weight = 10, Contribution = 0.45f, Item = BlackLizard },
            new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = Salamander }
        ];
        
        /// Karma 5, Progression to Karma 6
        public static readonly List<Extensions.WeightedItem<CT>> Karma6CreaturePool =
        [
            new() { Limit = 2, Weight = 18, Contribution = 0.70f, Item = CT.Vulture },
            new() { Limit = 2, Weight = 18, Contribution = 0.40f, Item = CT.DropBug },
            new() { Limit = 3, Weight = 12, Contribution = 0.65f, Item = CT.Scavenger },
            new() { Limit = 1, Weight = 36, Contribution = 0.45f, Item = GreenLizard },
            new() { Limit = 1, Weight = 36, Contribution = 0.65f, Item = CyanLizard },
            new() { Limit = 1, Weight = 36, Contribution = 0.50f, Item = WhiteLizard },
            new() { Limit = 1, Weight = 36, Contribution = 0.50f, Item = ZoopLizard }, // gonna leave this closer to other lizards contributions, cuz 0.8 threat is not realistic
            new() { Limit = 4, Weight =  8, Contribution = 0.40f, Item = YellowLizard },
            new() { Limit = 1, Weight = 36, Contribution = 0.60f, Item = MotherSpider }
        ];
        
        /// Karma 6, Progression to Karma 7
        public static readonly List<Extensions.WeightedItem<CT>> Karma7CreaturePool =
        [
            new() { Limit = 6, Weight =  6, Contribution = 0.70f, Item = CT.Scavenger },
            new() { Limit = 1, Weight = 36, Contribution = 0.40f, Item = KingVulture },
            new() { Limit = 1, Weight = 12, Contribution = 0.50f, Item = SpitterSpider },
            new() { Limit = 1, Weight = 36, Contribution = 0.65f, Item = EelLizard }, // 0.65 instead of 0.8
            new() { Limit = 1, Weight = 36, Contribution = 0.65f, Item = CyanLizard },
            new() { Limit = 1, Weight = 36, Contribution = 0.50f, Item = WhiteLizard },
        ];
        
        /// Karma 7, Progression to Karma 8
        public static readonly List<CT[]> Karma8CreatureFixedList =
        [
            [KingVulture, KingVulture],
            [RedCentipede],
            [SpitterSpider, SpitterSpider]
        ];
        
        /// Karma 8, Progression to Karma 9
        public static readonly List<CT[]> Karma9CreatureFixedList =
        [
            [MirosVulture],
            [RedLizard]
        ];
        
        /// Karma 9, Progression to Karma 10
        public static readonly List<CT[]> Karma10CreatureFixedList =
        [
            [CT.DaddyLongLegs, CT.DaddyLongLegs],
            [MirosVulture, MirosVulture],
            [CT.Deer], 
            [RedLizard, MotherLizard, CyanLizard, EelLizard, SpitLizard, WhiteLizard, BlackLizard, ZoopLizard, GreenLizard, PinkLizard, YellowLizard, BlueLizard, YoungLizard]
        ];
        
        /*
        //Karma 3, Progression to Karma 4
        public static Dictionary<CreatureTemplate.Type[], int> Ingredients2 = new Dictionary<CT[], int>()
        {
            { [CT.JetFish], 2 },
            { [CT.LanternMouse], 4 },
            { [HuntCicada], 2 },
            { [HuntCentipede], 2 },
            { [YellowLizard], 1 },
            { [Salamander], 1 },
            { [HuntSpider], 1 },
            { [SnowSpider], 1 },
        };

        public static List<HuntQuest> FromIngredientDictionary(Dictionary<CT[], int> dictionary)
        {
            var templates = new List<CreatureTemplate.Type>[] { [], [], [], [], [] };
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

        public static List<CreatureTemplate.Type[]> Ingredients2Alt =
        [
            [
                CT.JetFish, HuntCicada, CT.LanternMouse,
                CT.LanternMouse
            ],
            [CT.LanternMouse, HuntSpider, HuntCentipede],
            [HuntCicada, Centiwing, HuntCentipede],
            [SnowSpider, HuntCentipede, YellowLizard],
            [Salamander, CT.JetFish]
        ];*/

    }
}