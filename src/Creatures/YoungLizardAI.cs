using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheFriend.Creatures;

public class YoungLizardAI : LizardAI
{
    public YoungLizardAI(AbstractCreature crit, World world) : base(crit, world)
    {
        //this.yellowAI = new YellowAI(this);
        //base.AddModule(yellowAI);
    }
    public override void Update()
    {
        base.Update();
        if (behavior != Behavior.FollowFriend)
        {
            for (int j = 0; j < tracker.CreaturesCount; j++)
            {
                if (tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplateType.MotherLizard && lizard.GetLiz().mother == null) lizard.GetLiz().mother = tracker.GetRep(j).representedCreature;
            }
            if (lizard.GetLiz().mother != null) creature.abstractAI.SetDestination(lizard.GetLiz().mother.pos);
        }
    }
}
