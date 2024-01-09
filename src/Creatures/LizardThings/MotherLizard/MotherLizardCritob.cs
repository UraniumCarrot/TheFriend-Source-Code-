using System.Collections.Generic;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using MoreSlugcats;
using RWCustom;
using TheFriend.DragonRideThings;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.MotherLizard;
public class MotherLizardCritob : Critob
{
    public MotherLizardCritob() : base(CreatureTemplateType.MotherLizard)
    {
        Icon = new SimpleIcon("Kill_Green_Lizard", new(0.9f, .5f, .5f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(0.8f, 0.8f);
        RegisterUnlock(KillScore.Configurable(50), SandboxUnlockID.MotherLizard);
    }
    public override int ExpeditionScore() => 50;
    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;
    public override string DevtoolsMapName(AbstractCreature acrit) => "MthrL";
    public override IEnumerable<string> WorldFileAliases() => new[] { "motherlizard" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Lizards
    };
    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        s.Eats(CreatureTemplate.Type.CicadaA, 0.3f);
        s.Ignores(CreatureTemplate.Type.LizardTemplate);
        s.Ignores(CreatureTemplate.Type.LanternMouse);
        s.Eats(CreatureTemplate.Type.BigSpider, 0.4f);
        s.Eats(CreatureTemplate.Type.EggBug, 0.6f);
        s.Eats(CreatureTemplate.Type.JetFish, 0.05f);
        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);
        s.Eats(CreatureTemplate.Type.Centipede, 0.9f);
        s.Eats(CreatureTemplate.Type.Centiwing, 0.4f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.3f);
        s.Attacks(CreatureTemplate.Type.SmallNeedleWorm, 0.15f);
        s.Eats(CreatureTemplate.Type.DropBug, 0.4f);
        s.Eats(CreatureTemplate.Type.RedCentipede, 0.7f);
        s.Attacks(CreatureTemplate.Type.TentaclePlant, 0.4f);
        s.Ignores(CreatureTemplate.Type.Hazer);
        s.Attacks(CreatureTemplate.Type.Vulture, 0.5f);
        s.Attacks(CreatureTemplate.Type.KingVulture, 0.5f);
        s.Attacks(CreatureTemplate.Type.MirosBird, 0.5f);
        s.Attacks(CreatureTemplate.Type.RedLizard, 0.8f);
        s.Attacks(CreatureTemplate.Type.GreenLizard, 0.3f);
        s.AttackedBy(CreatureTemplate.Type.GreenLizard, 0.5f);
        s.Rivals(CreatureTemplate.Type.RedLizard, 0.2f);
        s.Attacks(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1f);
        s.IgnoredBy(CreatureTemplate.Type.LizardTemplate);
        s.Rivals(Type, 0.05f);
        s.Rivals(CreatureTemplateType.MotherLizard, 0.05f);
        s.IsInPack(CreatureTemplateType.YoungLizard, 1f);
        s.FearedBy(CreatureTemplate.Type.BlueLizard, 0.5f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.5f);
        s.Eats(CreatureTemplate.Type.BlueLizard, 0.5f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.5f);
        s.Ignores(CreatureTemplate.Type.Leech);
        s.Ignores(CreatureTemplate.Type.SmallCentipede);
        s.Eats(CreatureTemplate.Type.Scavenger, 0.6f);
        s.Attacks(CreatureTemplate.Type.Snail, 0.5f);
        s.Eats(CreatureTemplate.Type.SpitterSpider, 0.5f);
        s.Eats(CreatureTemplateType.SnowSpider, 0.5f);
        s.Ignores(CreatureTemplate.Type.GarbageWorm);
        s.Ignores(CreatureTemplate.Type.VultureGrub);
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.6f);
        s.HasDynamicRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.6f);
        s.FearedBy(CreatureTemplate.Type.BigSpider, 0.4f);
        s.FearedBy(CreatureTemplate.Type.DropBug, 0.4f);
        s.FearedBy(CreatureTemplate.Type.Vulture, 0.4f);
        s.IgnoredBy(CreatureTemplate.Type.KingVulture);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 0.3f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 0.8f);
        s.FearedBy(CreatureTemplate.Type.JetFish, 0.1f);
        s.EatenBy(CreatureTemplate.Type.BigEel, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Centipede, 0.9f);
        s.FearedBy(CreatureTemplate.Type.Centiwing, 0.4f);
        s.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 0.3f);
        s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);
        s.FearedBy(CreatureTemplate.Type.RedCentipede, 0.5f);
        s.EatenBy(CreatureTemplate.Type.TentaclePlant, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Hazer, 0.8f);
        s.FearedBy(CreatureTemplate.Type.MirosBird, 0.5f);
        s.IgnoredBy(CreatureTemplate.Type.Leech);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 0.4f);
        s.FearedBy(CreatureTemplate.Type.Snail, 1f);
        s.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.3f);
        s.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        s.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 0.9f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);
    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Lizard(acrit, acrit.world);
    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);
    public override void LoadResources(RainWorld rainWorld) { }
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.GreenLizard;

    public static void MotherLizardCtor(Lizard self, AbstractCreature abstractCreature, World world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        self.effectColor = Custom.HSL2RGB(
            Custom.WrappedRandomVariation(0.9f, 0.1f, 0.6f), 
            1f,
            Custom.ClampedRandomVariation(0.85f, 0.15f, 0.2f));
        self.GetLiz().IsRideable = true;
        Random.state = state;
    }

    public static SoundID MotherLizardVoice(Lizard self, SoundID res)
    {
        var array = new[] { "A", "B", "C", "D", "E" };
        var list = new List<SoundID>();
        for (int i = 0; i < array.Length; i++)
        {
            var soundID = SoundID.None;
            var text2 = "Lizard_Voice_Red_" + array[i];
            if (SoundID.values.entries.Contains(text2))
                soundID = new(text2);
            if (soundID != SoundID.None && soundID.Index != -1 && self.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                list.Add(soundID);
        }
        if (list.Count == 0)
            res = SoundID.None;
        else
            res = list[Random.Range(0, list.Count)];
        return res;
    }
}

