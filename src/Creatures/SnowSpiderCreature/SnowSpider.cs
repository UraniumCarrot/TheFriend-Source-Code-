using System;
using MoreSlugcats;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;

namespace Solace.Creatures.SnowSpiderCreature;

public class SnowSpider : BigSpider
{
    public SnowSpider(AbstractCreature acrit) : base(acrit, acrit.world)
    {
        var state = Random.state;
        Random.InitState(acrit.ID.RandomSeed);
        yellowCol = Color.Lerp(new Color(0.3f, 0.9f, 1f), new Color(0.6f, 0.55f, 1f), Random.value);
        Random.state = state;
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        try
        {
            if (room != null && !dead)
            {
                foreach (IProvideWarmth heat in room.blizzardHeatSources)
                {
                    if (heat != null)
                    {
                        float heatpos = Vector2.Distance(firstChunk.pos, heat.Position());
                        if (heat.loadedRoom == room && heatpos < heat.range)
                        {
                            float heateffect = Mathf.InverseLerp(heat.range, heat.range * 0.2f, heatpos);
                            State.health -= Mathf.Lerp(heat.warmth * heateffect, 0f, HypothermiaExposure);
                        }
                    }
                }
                State.health -= 0.0014705883f;
                State.health = Mathf.Min(1f, State.health + (Submersion >= 0.1f ? 0 : HypothermiaExposure * 0.008f));
            }
            if (graphicsModule is SnowSpiderGraphics gr) gr.bodyThickness = SnowSpiderGraphics.originalBodyThickness + State.health;
        }
        catch (Exception) { Debug.Log("Solace: Harmless exception occurred in SnowSpider.Update"); }
    }
    public override Color ShortCutColor()
    {
        return yellowCol;
    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new SnowSpiderGraphics(this);
        graphicsModule.Reset();
    }
}
