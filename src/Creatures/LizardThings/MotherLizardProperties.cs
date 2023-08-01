using Fisobs.Properties;

namespace Solace.Creatures.LizardThings;
public class MotherLizardProperties : ItemProperties
{
    public readonly Lizard motherLizard;

    public MotherLizardProperties(Lizard motherLizard)
    {
        this.motherLizard = motherLizard;
    }
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;
        //if ((player?.GetPoacher()?.dragonSteed as Lizard).GetLiz().boolseat0) grabability = Player.ObjectGrabability.CantGrab;
    }
}