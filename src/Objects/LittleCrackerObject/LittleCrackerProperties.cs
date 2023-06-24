using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Properties;

namespace TheFriend.Objects.LittleCrackerObject;

public class LittleCrackerProperties : ItemProperties
{
    // Scavengers DNI
    public override void ScavCollectScore(Scavenger scavenger, ref int score)
        => score = 3;

    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        => score = 3;

    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        => score = 2;
    // Player stuff
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;
    }
}
