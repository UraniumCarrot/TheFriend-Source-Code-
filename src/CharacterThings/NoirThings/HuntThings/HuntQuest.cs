using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheFriend.WorldChanges;
using UnityEngine;
using static MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType;

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

        //-----

        public static CreatureTemplate.Type TypeTranslator(CreatureTemplate.Type type)
        {
            if (type == CreatureTemplate.Type.Centipede || type == CreatureTemplate.Type.Centiwing || type == AquaCenti)
                return HuntCentipede;
            if (type == CreatureTemplate.Type.CicadaA || type == CreatureTemplate.Type.CicadaB)
                return HuntCicada;
            if (type == CreatureTemplate.Type.EggBug || type == FireBug)
                return HuntEggbug;
            if (type == CreatureTemplate.Type.DaddyLongLegs || type == TerrorLongLegs)
                return HuntDLL;

            if (type == HuntCentipede)
                return CreatureTemplate.Type.Centipede;
            if (type == HuntCicada)
                return CreatureTemplate.Type.CicadaA;
            if (type == HuntEggbug)
                return CreatureTemplate.Type.EggBug;
            if (type == HuntDLL)
                return CreatureTemplate.Type.DaddyLongLegs;

            return type;
        }

        public static readonly CreatureTemplate.Type HuntCentipede = new CreatureTemplate.Type(nameof(HuntCentipede));
        public static readonly CreatureTemplate.Type HuntCicada = new CreatureTemplate.Type(nameof(HuntCicada));
        public static readonly CreatureTemplate.Type HuntEggbug = new CreatureTemplate.Type(nameof(HuntEggbug));
        public static readonly CreatureTemplate.Type HuntDLL = new CreatureTemplate.Type(nameof(HuntDLL));
    }

    private static void CreatureSymbolOnctor(On.CreatureSymbol.orig_ctor orig, CreatureSymbol self, IconSymbol.IconSymbolData icondata, FContainer container)
    {
        if (icondata.critType == HuntQuest.HuntCentipede || icondata.critType == HuntQuest.HuntCicada ||
            icondata.critType == HuntQuest.HuntEggbug || icondata.critType == HuntQuest.HuntDLL)
        {
            var type = icondata.critType;
            icondata.critType = HuntQuest.TypeTranslator(type);

            orig(self, icondata, container);

            self.myColor = CreatureSymbol.ColorOfCreature(icondata);
            self.spriteName = CreatureSymbol.SpriteNameOfCreature(icondata);
            self.graphWidth = Futile.atlasManager.GetElementWithName(self.spriteName).sourcePixelSize.x;

            if (type == HuntQuest.HuntCicada)
                self.myColor = new Color(0.55f, 0.85f, 0.95f);
            if (type == HuntQuest.HuntCentipede)
                self.myColor = Color.HSVToRGB(FamineWorld.defCentiColor, FamineWorld.defCentiSat, 1f);

            self.iconData.critType = type;
            return;
        }
        orig(self, icondata, container);

        if (icondata.critType == CreatureTemplate.Type.SmallCentipede)
            self.myColor = Color.HSVToRGB(FamineWorld.defCentiColor, FamineWorld.defCentiSat, 1f);
        if (icondata.critType == CreatureTemplate.Type.LanternMouse)
            self.myColor = new Color(0.4f, 0.85f, 0.95f);
    }
}