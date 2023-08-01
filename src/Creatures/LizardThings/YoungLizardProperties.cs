using Fisobs.Properties;

namespace Solace.Creatures.LizardThings;
public class YoungLizardProperties : ItemProperties
{
    public readonly Lizard youngLizard;

    public YoungLizardProperties(Lizard youngLizard)
    {
        this.youngLizard = youngLizard;
    }
}
