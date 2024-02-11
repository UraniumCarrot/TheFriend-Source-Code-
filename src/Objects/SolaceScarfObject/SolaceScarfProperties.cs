using Fisobs.Properties;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfProperties : ItemProperties
{
    // Scavengers DNI
    public override void ScavCollectScore(Scavenger _, ref int score)
        => score = 0;

    public override void ScavWeaponPickupScore(Scavenger _, ref int score)
        => score = 0;

    public override void ScavWeaponUseScore(Scavenger _, ref int score)
        => score = 0;
}