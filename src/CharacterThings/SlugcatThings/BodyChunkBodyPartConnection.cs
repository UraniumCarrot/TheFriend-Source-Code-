using RWCustom;
using UnityEngine;

namespace TheFriend.SlugcatThings;

public class BodyChunkBodyPartConnection
{
    private const float BaseFakeWeight = 0.7f / 2f; //Base player chunk weight

    public BodyChunk chunk;
    public BodyPart part;
    public float distance;
    public float elasticity;
    public float weightSymmetry;
    public bool active;
    public PhysicalObject.BodyChunkConnection.Type type;

    public BodyChunkBodyPartConnection(BodyChunk bodyChunk, BodyPart bodyPart, float distance, PhysicalObject.BodyChunkConnection.Type type, float elasticity, float weightSymmetry, float bodyPartFakeWeight = BaseFakeWeight)
    {
        chunk = bodyChunk;
        part = bodyPart;
        this.distance = distance;
        this.type = type;
        this.elasticity = elasticity;
        if (weightSymmetry == -1f)
            this.weightSymmetry = bodyPartFakeWeight / (bodyChunk.mass + bodyPartFakeWeight);
        else
            this.weightSymmetry = weightSymmetry;
        active = true;
    }
    
    public void Update()
    {
        if (!active)
            return;

        var currentDistance = Vector2.Distance(chunk.pos, part.pos);
        if (type == PhysicalObject.BodyChunkConnection.Type.Normal ||
            (type == PhysicalObject.BodyChunkConnection.Type.Pull && currentDistance > distance) ||
            (type == PhysicalObject.BodyChunkConnection.Type.Push && currentDistance < distance))
        {
            var a = Custom.DirVec(chunk.pos, part.pos);
            chunk.pos -= (distance - currentDistance) * a * weightSymmetry * elasticity;
            chunk.vel -= (distance - currentDistance) * a * weightSymmetry * elasticity;
            part.pos += (distance - currentDistance) * a * (1f - weightSymmetry) * elasticity;
            part.vel += (distance - currentDistance) * a * (1f - weightSymmetry) * elasticity;
        }
    }
}