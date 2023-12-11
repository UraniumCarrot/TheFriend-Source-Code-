using TheFriend;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using RWCustom;

namespace TheFriend.CharacterThings.DelugeThings;

public abstract class SensoryHologram : UpdatableAndDeletable, IDrawable
{
    public abstract class SensoryHoloPart
    {
        public class Line
        {
            public Vector3 A;
            public Vector3 B;
            public Vector3 A2;
            public Vector3 B2;
            public int sprite;

            public Line(Vector3 A, Vector3 B, int sprite)
            {
                this.A = A;
                this.B = B;
                A2 = A;
                B2 = B;
                this.sprite = sprite;
            }
        }
        public SensoryHologram hologram;
        public void AddClosedPolygon(List<Vector2> vL)
		{
			for (int i = 1; i < vL.Count; i++)
			{
				AddLine(vL[i - 1], vL[i]);
			}
			AddLine(vL[vL.Count - 1], vL[0]);
		}

		public void AddClosed3DPolygon(List<Vector2> vL, float depth)
		{
			for (int i = 1; i < vL.Count; i++)
			{
				Add3DLine(vL[i - 1], vL[i], depth);
			}
			Add3DLine(vL[vL.Count - 1], vL[0], depth);
		}

		public void Add3DLine(Vector2 A, Vector2 B, float depth)
		{
			AddLine(new Vector3(A.x, A.y, 0f - depth), new Vector3(B.x, B.y, 0f - depth));
			AddLine(new Vector3(A.x, A.y, depth), new Vector3(B.x, B.y, depth));
			AddLine(new Vector3(A.x, A.y, 0f - depth), new Vector3(A.x, A.y, depth));
		}

		public void AddLine(Vector2 A, Vector2 B)
		{
			AddLine(new Vector3(A.x, A.y, 0f), new Vector3(B.x, B.y, 0f));
		}

		public void AddLine(Vector3 A, Vector3 B)
		{
			lines.Add(new Line(A, B, firstSprite + totalSprites));
			totalSprites++;
		}

		public virtual void Update()
		{
			lastRotation = rotation;
			lastOffset = offset;
			lastTransform = transform;
			lastPartFade = partFade;
			partFade = Custom.LerpAndTick(partFade, visible ? 1f : 0f, 0.01f, 0.05f);
			if ((partFade * hologram.fade == 0f || partFade * hologram.fade == 1f) && partFade * hologram.fade != lastPartFade * hologram.lastFade)
			{
				fadeExponent = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
			}
			lastColor = color;
			color = Color.Lerp(color, GetToColor, 0.5f);
		}

		public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				sLeaser.sprites[lines[i].sprite] = new FSprite("pixel");
				sLeaser.sprites[lines[i].sprite].anchorY = 0f;
			}
		}

		public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 partPos, Vector2 headPos, float useFade, float popOut, Color useColor)
		{
		}

		public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
    }

    public class Arrow : SensoryHoloPart
    {
        public Vector2 dir;

        public Arrow(SensoryHologram hologram, int firstSprite)
            : base(hologram, firstSprite)
        {
            AddClosed3DPolygon(new List<Vector2>
            {
                new Vector2(-3.5f, 20f),
                new Vector2(0f, 27f),
                new Vector2(3.5f, 20f)
            }, 3f);
        }

        public override void Update()
        {
            base.Update();
            rotation.z = Custom.VecToDeg(new Vector2(0f - dir.x, dir.y));
        }
    }

    public class Symbol : SensoryHoloPart
    {
        
    }
    
    
    
    public Player owner;
    
    public Vector2 pos;
    public Vector2 lastPos;

    public List<SensoryHoloPart> parts;

    public virtual Color color
    {
        get
        {
            if (owner != null) return owner.ShortCutColor();
            return Color.magenta;
        }
    }

    public SensoryHologram(Player owner)
    {
        this.owner = owner;
    }

    public void AddPart(SensoryHoloPart part)
    {
        parts.Add(part);
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            parts[i].ApplyPalette(sLeaser, rCam, palette);
        }
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null)
        {
            newContainer = rCam.ReturnFContainer("Bloom");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            newContainer.AddChild(sLeaser.sprites[i]);
        }
    }



}