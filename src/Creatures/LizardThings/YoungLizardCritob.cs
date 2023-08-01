using System.Collections.Generic;
using DevInterface;
using MoreSlugcats;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;
using Fisobs.Properties;

namespace Solace.Creatures.LizardThings;
public class YoungLizardCritob : Critob
{
    public YoungLizardCritob() : base(CreatureTemplateType.YoungLizard)
    {
        Icon = new SimpleIcon("Kill_Standard_Lizard", new(.85f, 1f, .85f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(0.3f, 0.3f);
        RegisterUnlock(KillScore.Configurable(2), SandboxUnlockID.YoungLizard);
        Hooks.Apply();
    }
    public override int ExpeditionScore() => 3;
    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;
    public override string DevtoolsMapName(AbstractCreature acrit) => "YngL";
    public override IEnumerable<string> WorldFileAliases() => new[] { "younglizard" };
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
        s.Eats(CreatureTemplate.Type.LanternMouse, 0.4f);
        s.Eats(CreatureTemplate.Type.Fly, 0.4f);
        s.Fears(CreatureTemplate.Type.BigSpider, 0.4f);
        s.Fears(CreatureTemplateType.SnowSpider, 0.4f);
        s.Eats(CreatureTemplate.Type.EggBug, 0.6f);
        s.Fears(CreatureTemplate.Type.JetFish, 0.2f);
        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);
        s.Fears(CreatureTemplate.Type.Centipede, 0.9f);
        s.Ignores(CreatureTemplate.Type.Centiwing);
        s.Ignores(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.3f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.7f);
        s.Fears(CreatureTemplate.Type.DropBug, 0.4f);
        s.Fears(CreatureTemplate.Type.RedCentipede, 0.9f);
        s.Fears(CreatureTemplate.Type.TentaclePlant, 0.9f);
        s.Eats(CreatureTemplate.Type.Hazer, 0.6f);
        s.Fears(CreatureTemplate.Type.Vulture, 0.5f);
        s.Fears(CreatureTemplate.Type.KingVulture, 0.5f);
        s.Fears(CreatureTemplate.Type.MirosBird, 0.5f);
        s.Fears(CreatureTemplate.Type.GreenLizard, 0.3f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1f);
        s.IgnoredBy(CreatureTemplate.Type.LizardTemplate);
        s.EatenBy(CreatureTemplate.Type.GreenLizard, 0.8f);
        s.IsInPack(CreatureTemplateType.MotherLizard, 1f);
        s.IsInPack(CreatureTemplateType.YoungLizard, 1f);
        s.Ignores(CreatureTemplate.Type.Leech);
        s.Eats(CreatureTemplate.Type.SmallCentipede, 0.6f);
        s.Fears(CreatureTemplate.Type.Scavenger, 0.6f);
        s.Eats(CreatureTemplate.Type.Snail, 0.3f);
        s.Fears(CreatureTemplate.Type.SpitterSpider, 1f);
        s.Ignores(CreatureTemplate.Type.GarbageWorm);
        s.Eats(CreatureTemplate.Type.VultureGrub, 0.6f);
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.6f);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 0.3f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 0.8f);
        s.FearedBy(CreatureTemplate.Type.JetFish, 0.1f);
        s.EatenBy(CreatureTemplate.Type.BigEel, 0.5f);
        s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);
        s.EatenBy(CreatureTemplate.Type.TentaclePlant, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Hazer, 0.8f);
        s.IgnoredBy(CreatureTemplate.Type.Leech);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 0.4f);
        s.FearedBy(CreatureTemplate.Type.Snail, 1f);
        s.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        s.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        s.HasDynamicRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.6f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.9f);
        s.MakesUncomfortable(CreatureTemplate.Type.Slugcat, 0.9f);
    }
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new YoungLizardAI(acrit, acrit.world);
    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Lizard(acrit, acrit.world);
    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);
    public override void LoadResources(RainWorld rainWorld) { }
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.PinkLizard;
    public override ItemProperties Properties(Creature crit)
    {
        if (crit is Lizard youngLizard)
        {
            return new YoungLizardProperties(youngLizard);
        }
        return null;
    }
}