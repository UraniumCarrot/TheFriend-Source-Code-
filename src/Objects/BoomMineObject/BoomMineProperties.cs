using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Properties;

namespace Solace.Objects.BoomMineObject;

public class BoomMineProperties : ItemProperties
{
    // Scavengers DNI
    public override void ScavCollectScore(Scavenger scavenger, ref int score)
        => score = 0;

    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        => score = 0;

    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        => score = 0;
    // Player stuff
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;
    }
}
