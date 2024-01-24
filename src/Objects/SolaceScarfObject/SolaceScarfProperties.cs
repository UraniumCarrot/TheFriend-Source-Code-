using Fisobs.Properties;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfProperties : ItemProperties
{
    // Scavengers DNI
    public override void ScavCollectScore(Scavenger scavenger, ref int score)
        => score = 0;

    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        => score = 0;

    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        => score = 0;
}