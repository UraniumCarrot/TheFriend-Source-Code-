using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public abstract partial class HuntQuestThings
{
    public class ObservableTargetCollection<T> : ObservableCollection<T>
    {
        public readonly HuntQuest Owner;
        public ObservableTargetCollection(HuntQuest owner)
        {
            Owner = owner;
        }
    }

    public class HuntQuest
    {
        public readonly ObservableTargetCollection<CreatureTemplate.Type> Targets;

        public HuntQuest(List<CreatureTemplate.Type> targets) : this()
        {
            foreach (var target in targets) Targets.Add(target);
        }
        public HuntQuest()
        {
            Targets = new ObservableTargetCollection<CreatureTemplate.Type>(this);
        }

        public bool Completed => !Targets.Any();

        public static IconSymbol.IconSymbolData SymbolDataFromType(CreatureTemplate.Type type)
        {
            return !(type == CreatureTemplate.Type.Centipede) ? //why, rain world
                new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0):
                new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 2);
        }

        public static List<HuntQuest> GenerateQuests(StoryGameSession session)
        {
            return new List<HuntQuest>()
            {
                new HuntQuest(new List<CreatureTemplate.Type>()
                {
                    CreatureTemplate.Type.YellowLizard,
                    CreatureTemplate.Type.PinkLizard,
                    CreatureTemplate.Type.BlueLizard
                }),
                new HuntQuest(new List<CreatureTemplate.Type>()
                {
                    CreatureTemplate.Type.VultureGrub
                })
            };
        }
    }
}