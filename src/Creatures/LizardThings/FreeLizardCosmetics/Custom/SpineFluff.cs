using System;
using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Custom;

public class SpineFluff : Template
{ // unfinished and non functional, do not use
    private int length;
    public List<TailSegment> hairs;
    public Vector2[] scalesPositions;
    
    public SpineFluff(LizardGraphics lGraphics, int startsprite) : base(lGraphics,startsprite)
    {
        spritesOverlap = SpritesOverlap.InFront;
        length = Mathf.CeilToInt(Random.Range(3, 8));
        hairs = new List<TailSegment>();
        Debug.Log("2");
        for (int i = 0; i < length; i++)
        {
            Debug.Log("forloop"+i);
            if (i == 0) hairs.Add(new TailSegment(lGraphics,1,1,null,1,1,1,false));
            else hairs.Add(new TailSegment(lGraphics,1,1,hairs[i-1],1,1,1,false));
        }
        Debug.Log("3");
        numberOfSprites = 1;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Debug.Log("4");
        var container = rCam.ReturnFContainer("Midground");
        sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(length, true, true);
        sLeaser.sprites[startSprite].color = Color.red;
        for (int i = startSprite; i < startSprite + length; i++)
        {
            var mesh = sLeaser.sprites[i] as TriangleMesh;
            for (int a = 0; a < mesh.verticeColors.Length; a++)
                mesh.verticeColors[a] =
                    RWCustom.Custom.HSL2RGB(Random.value, 1, 0.5f);
        }
        container.AddChild(sLeaser.sprites[startSprite]);
        Debug.Log("5");
    }

    public override void Update()
    {
        foreach (TailSegment tail in hairs)
            tail.Update();
        hairs[0].connectedPoint = lGraphics.drawPositions[0,0];
    }
}



