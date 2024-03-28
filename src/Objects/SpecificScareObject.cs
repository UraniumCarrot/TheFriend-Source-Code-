using System;
using UnityEngine;
using System.Collections.Generic;
using RWCustom;
using MoreSlugcats;

namespace TheFriend.Objects;

public class SpecificScareObject : UpdatableAndDeletable
{ // Allows scaring away a specific kind of creature instead of all creatures
	public int lifeTime;
	public int maxLifeTime;
	public Vector2 pos;
	public List<ThreatTracker.ThreatPoint> threatPoints;
	public bool init;
	public float fearRange;
	public CreatureTemplate.Type species;
	public List<AbstractCreature> exclusionList;
	
	public SpecificScareObject(Vector2 pos, CreatureTemplate.Type species, float fearRange = 2000f, int lifeTime = 700)
	{
		this.species = species;
		this.pos = pos;
		this.fearRange = fearRange;
		this.lifeTime = lifeTime;
		maxLifeTime = lifeTime;
		threatPoints = new List<ThreatTracker.ThreatPoint>();
	}
	public override void Update(bool eu)
	{
		base.Update(eu);
		WorldCoordinate worldCoordinate = room.GetWorldCoordinate(pos);
		if (lifeTime == maxLifeTime) for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (!CheckCreatureType(room.abstractRoom.creatures[i])) continue;
			
			if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.GarbageWorm)
			{
				(room.abstractRoom.creatures[i].realizedCreature as GarbageWorm).AI.stress = 1f;
				(room.abstractRoom.creatures[i].realizedCreature as GarbageWorm).Retract();
			}
			
			else if (room.abstractRoom.creatures[i].realizedCreature != null && 
			    !room.abstractRoom.creatures[i].realizedCreature.dead && 
			    Custom.DistLess(room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, pos, fearRange))
				if (room.abstractRoom.creatures[i].abstractAI != null && 
				    room.abstractRoom.creatures[i].abstractAI.RealAI != null && 
				    room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker != null)
				{
					threatPoints.Add(room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker.AddThreatPoint(null, worldCoordinate, 1f));
					MakeCreatureLeaveRoom(room.abstractRoom.creatures[i].abstractAI.RealAI);
				}
		}
		for (int j = 0; j < threatPoints.Count; j++)
		{
			threatPoints[j].severity = Mathf.InverseLerp(700f, 500f, (float)lifeTime / maxLifeTime);
			threatPoints[j].pos = worldCoordinate;
		}
		if (lifeTime <= 0)
			Destroy();
		if (lifeTime > 0) lifeTime--;
	}
	public void MakeCreatureLeaveRoom(ArtificialIntelligence AI)
	{
		if (AI.creature.abstractAI.destination.room != room.abstractRoom.index)
			return;
		int desiredExit = AI.threatTracker.FindMostAttractiveExit();
		if (desiredExit > -1 && desiredExit < room.abstractRoom.nodes.Length && room.abstractRoom.nodes[desiredExit].type == AbstractRoomNode.Type.Exit)
		{
			int exit = room.world.GetAbstractRoom(room.abstractRoom.connections[desiredExit]).ExitIndex(room.abstractRoom.index);
			if (exit > -1)
				AI.creature.abstractAI.MigrateTo(new WorldCoordinate(room.abstractRoom.connections[desiredExit], -1, -1, exit));
		}
	}
	public bool CheckCreatureType(AbstractCreature crit)
	{
		if (exclusionList.Contains(crit)) return false;
		switch (crit.creatureTemplate.type.value)
		{
			case nameof(CreatureTemplateType.MotherLizard): return false;
			case nameof(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing): return false;
			case nameof(species): return true;
			default: if (crit.creatureTemplate.ancestor?.type == species || 
			             crit.creatureTemplate.ancestor?.ancestor?.type == species) 
					return true;
				return false;
		}
	}

	public void ResetLifetime()
	{
		lifeTime = maxLifeTime;
	}
	public override void Destroy()
	{
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (!CheckCreatureType(room.abstractRoom.creatures[i])) continue;
			if (room.abstractRoom.creatures[i].realizedCreature != null && 
		     !room.abstractRoom.creatures[i].realizedCreature.dead && 
		     room.abstractRoom.creatures[i].abstractAI != null && 
		     room.abstractRoom.creatures[i].abstractAI.RealAI != null && 
		     room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker != null)
				for (int j = 0; j < threatPoints.Count; j++)
					room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker.RemoveThreatPoint(threatPoints[j]);
		}
		base.Destroy();
	}
}