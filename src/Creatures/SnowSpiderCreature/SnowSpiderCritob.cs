using System.Collections.Generic;
using DevInterface;
using MoreSlugcats;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;

namespace TheFriend.Creatures.SnowSpiderCreature;

public class SnowSpiderCritob : Critob
{
    public SnowSpiderCritob() : base(CreatureTemplateType.SnowSpider)
    {
        Icon = new SimpleIcon("Kill_BigSpider", new(0.5f, .7f, 1f));
        LoadedPerformanceCost = 65f;
        ShelterDanger = (ShelterDanger)1;
        SandboxPerformanceCost = new(0.5f, 0.6f);
        RegisterUnlock(KillScore.Configurable(10), SandboxUnlockID.SnowSpider, null, 0);
    }
    public override int ExpeditionScore() => 10;
    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;
    public override string DevtoolsMapName(AbstractCreature acrit) => "SnwDr";
    public override IEnumerable<string> WorldFileAliases() => new[] { "snowspider" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.All
    };
    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate s = new CreatureFormula(CreatureTemplate.Type.BigSpider, Type, "SnowSpider")
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            HasAI = true,
            InstantDeathDamage = 7,
            DamageResistances = new()
            {
                Base = 6.5f,
                Explosion = 0.25f,
                Water = 7f,
                Stab = 0.4f,
            },
            StunResistances = new()
            {
                Base = 30f,
                Stab = 30f,
                Bite = 30f,
                Blunt = 1f,
                Explosion = 0.25f,
                Electric = 0.25f,
            },

            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.PinkLizard),
            TileResistances = new()
            {
                Air = new PathCost(100f, PathCost.Legality.Unallowed),
                Ceiling = new PathCost(100f, PathCost.Legality.Unallowed),
                Wall = new PathCost(100f, PathCost.Legality.Unallowed),

                Climb = new PathCost(1f, PathCost.Legality.Allowed),
                OffScreen = new PathCost(1f, PathCost.Legality.Allowed),
                Floor = new PathCost(1f, PathCost.Legality.Allowed),
                Corridor = new PathCost(1f, PathCost.Legality.Allowed),
            },
            ConnectionResistances = new()
            {
                Standard = new PathCost(1f, PathCost.Legality.Allowed),
                OpenDiagonal = new PathCost(1f, PathCost.Legality.Allowed),
                ReachOverGap = new PathCost(1f, PathCost.Legality.Allowed),
                ReachUp = new PathCost(1f, PathCost.Legality.Allowed),
                ReachDown = new PathCost(1f, PathCost.Legality.Allowed),
                SemiDiagonalReach = new PathCost(1f, PathCost.Legality.Allowed),
                DropToClimb = new PathCost(1f, PathCost.Legality.Allowed),
                DropToFloor = new PathCost(1f, PathCost.Legality.Allowed),
                DropToWater = new PathCost(1f, PathCost.Legality.Allowed),
                ShortCut = new PathCost(1f, PathCost.Legality.Allowed),
                NPCTransportation = new PathCost(1f, PathCost.Legality.Allowed),
                OffScreenMovement = new PathCost(1f, PathCost.Legality.Allowed),
                BetweenRooms = new PathCost(1f, PathCost.Legality.Allowed),
                Slope = new PathCost(1f, PathCost.Legality.Allowed),
                CeilingSlope = new PathCost(1f, PathCost.Legality.Allowed),

            }
        }.IntoTemplate();
        s.BlizzardAdapted = true;
        s.BlizzardWanderer = true;
        s.meatPoints = 10;
        s.dangerousToPlayer = 0.8f;
        return s;
    }
    public override void EstablishRelationships()
    {
        Relationships s;
        s = new Relationships(Type);
        s.Eats(CreatureTemplate.Type.CicadaA, 1f);
        s.Eats(CreatureTemplate.Type.LizardTemplate, 1f);
        s.Eats(CreatureTemplate.Type.LanternMouse, 1f);
        s.Eats(CreatureTemplate.Type.Centipede, 1f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        s.Eats(CreatureTemplate.Type.DropBug, 1f);
        s.Eats(CreatureTemplate.Type.EggBug, 1f);
        s.Eats(CreatureTemplate.Type.Slugcat, 1f);
        s.Eats(CreatureTemplate.Type.Scavenger, 1f);
        s.Eats(CreatureTemplate.Type.JetFish, 0.5f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1f);
        s.Eats(CreatureTemplate.Type.Snail, 0.5f);

        s.Fears(CreatureTemplate.Type.MirosBird, 1f);
        s.Fears(CreatureTemplate.Type.RedCentipede, 1f);
        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.Fears(CreatureTemplate.Type.Vulture, 1f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 1f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 1f);

        s.FearedBy(CreatureTemplate.Type.CicadaA, 0.5f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 1f);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        s.FearedBy(CreatureTemplate.Type.DropBug, 1f);
        s.FearedBy(CreatureTemplate.Type.EggBug, 1f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 1f);
        s.FearedBy(CreatureTemplate.Type.JetFish, 0.5f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1f);

        s.AttackedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        s.AttackedBy(CreatureTemplateType.MotherLizard, 1f);

        s.EatenBy(CreatureTemplate.Type.LizardTemplate, 1f);
        s.EatenBy(CreatureTemplate.Type.PoleMimic, 1f);
        s.EatenBy(CreatureTemplate.Type.TentaclePlant, 1f);
        s.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        s.EatenBy(CreatureTemplate.Type.Centipede, 1f);
        s.EatenBy(CreatureTemplate.Type.Leech, 1f);
    }
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new SnowSpiderAI(acrit, acrit.world);
    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new SnowSpider(acrit);
    public override void LoadResources(RainWorld rainWorld) { }
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.BigSpider;
}
