using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using DevInterface;
using MoreSlugcats;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using Fisobs.Properties;

namespace TheFriend.Creatures;

public class PebblesLLCritob : Critob
{
    public PebblesLLCritob() : base(CreatureTemplateType.PebblesLL)
    {
        Icon = new SimpleIcon("Kill_Daddy", new(1f, 0.3f, 0.9f));
        LoadedPerformanceCost = 65f;
        SandboxPerformanceCost = new(0.5f, 0.6f);
        //RegisterUnlock(KillScore.Configurable(0), SandboxUnlockID.PebblesLL, null, 0);
    }
    public override int ExpeditionScore() => 0;
    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;
    public override string DevtoolsMapName(AbstractCreature acrit) => "FPDLL";
    public override IEnumerable<string> WorldFileAliases() => new[] { "pebbleslonglegs" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.All
    };
    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate s = new CreatureFormula(CreatureTemplate.Type.BrotherLongLegs, base.Type, "PebblesLongLegs")
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            HasAI = true
        }
        .IntoTemplate();
        return s;
    }
    public override void EstablishRelationships()
    {
    }
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new PebblesLLAI(acrit, acrit.world);
    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new PebblesLL(acrit, acrit.world);
    public override CreatureState CreateState(AbstractCreature acrit) => new DaddyLongLegs.DaddyState(acrit);
    public override void LoadResources(RainWorld rainWorld) { }
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.DaddyLongLegs;
}
