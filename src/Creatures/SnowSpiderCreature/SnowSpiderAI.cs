using RWCustom;

namespace Solace.Creatures.SnowSpiderCreature;

public class SnowSpiderAI : BigSpiderAI
{
    public SnowSpiderAI(AbstractCreature crit, World world) : base(crit, world)
    {
    }
    public override void Update()
    {
        base.Update();
        bug.runSpeed = 0f;
        if (bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0f, 0.01f, 1f / 60f);
        if (behavior == Behavior.Idle && !bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.15f, 0.01f, 1f / 60f);
        if (behavior == Behavior.Hunt && !bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.15f, 0.01f, 0.1f);
        if (behavior == Behavior.Flee && !bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.2f, 0.01f, 0.1f);
    }
}
