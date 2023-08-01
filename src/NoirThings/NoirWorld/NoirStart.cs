using System.Linq;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    public class NoirStart : UpdatableAndDeletable
    {
        private Player Cat;
        private NoirData NoirData;
        private byte Phase;
        private uint PhaseCounter;
        private Cicada Squiddy;

        public NoirStart() { }

        public Player GetNoirOrNull()
        {
            foreach (var player in room.game.Players)
            {
                if (player.realizedCreature is Player realPlayer)
                {
                    if (realPlayer.SlugCatClass == Plugin.NoirName)
                    {
                        return realPlayer;
                    }
                }
            }
            return null;
        }

        public override void Update(bool eu)
        {
            if (Cat == null)
            {
                Cat = room.game.Players.Count <= 0 ? null : GetNoirOrNull();
                if (Cat != null) NoirData = Cat.GetNoir();
                return;
            }

            PhaseCounter++;

            switch (Phase)
            {
                case 0:
                    Cat.controller = new NoirStartController(this);
                    Cat.bodyChunks[0].HardSetPosition(room.MiddleOfTile(10, 77));
                    Cat.bodyChunks[1].HardSetPosition(room.MiddleOfTile(11, 78));
                    Cat.bodyChunks[0].vel = Vector2.zero;
                    Cat.bodyChunks[1].vel = Vector2.zero;

                    var squiddy = new AbstractCreature(room.world,  StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA), null,
                        room.GetWorldCoordinate(new IntVector2(30, 68)), room.game.GetNewID());
                    room.abstractRoom.AddEntity(squiddy);
                    squiddy.RealizeInRoom();
                    Squiddy = (Cicada)squiddy.realizedCreature;
                    Squiddy.AI.modules = Squiddy.AI.modules.Where(x => x is CicadaPather).ToList();

                    NextPhase();
                    break;
                case 1:
                    Cat.sleepCurlUp = 1f;
                    Cat.sleepCounter = 99;
                    if (room.game.manager.fadeToBlack <= 0.1f) NextPhase();
                    break;
                case 2:
                    if (PhaseCounter < 40)
                    {
                        Cat.sleepCurlUp = 1f;
                        Cat.sleepCounter = 99;
                    }
                    else
                    {
                        Cat.sleepCurlUp = 0f;
                        Cat.sleepCounter = 10;
                        NextPhase();
                    }
                    break;
                case 3:
                    if (Cat.abstractCreature.pos.x <= 5) NextPhase();
                    break;
                case 4:
                    if (Cat.abstractCreature.pos.x >= 11)
                    {
                        Cat.room?.PlaySound(Meow2SND, Cat.firstChunk, false, 1f, NoirData.MeowPitch);
                        NextPhase();
                    };
                    break;
                case 5:
                    if (Custom.DistLess(Cat.firstChunk.pos, Squiddy.firstChunk.pos, 40f)) NextPhase(); //If close to Squidcada, hit it.
                    if (PhaseCounter >= 30)
                    {
                        Debug.Log("NoirStart: Failed to hunt Squidcada!");
                        NextPhase();
                    }
                    break;
                case 6:
                    if (Cat.bodyChunks[0].contactPoint != default || Cat.bodyChunks[1].contactPoint != default) NextPhase();
                    break;
                case 7:
                    if (PhaseCounter >= 5) NextPhase();
                    break;
                case 8:
                    if (NoirData.OnAnyBeam()) NextPhase();
                    break;
                case 9:
                    if (Cat.grabbedBy.Count > 0)
                    {
                        foreach (var villain in Cat.grabbedBy.ToArray()) //Centichildren like me too much
                        {
                            villain.Release();
                        }
                    }
                    if (Cat.animation == Player.AnimationIndex.BeamTip) this.Destroy();
                    break;
            }

            if (Phase >= 1)
            {
                if (Squiddy.abstractCreature.abstractAI != null)
                {
                    var goal = room.GetWorldCoordinate(new IntVector2(18, 62));
                    Squiddy.AI.behavior = CicadaAI.Behavior.Idle;
                    Squiddy.AI.SetDestination(goal);
                }
            }
        }

        public Player.InputPackage GetInput()
        {
            var inpt = new Player.InputPackage(false, global::Options.ControlSetup.Preset.KeyboardSinglePlayer, 0, 0, false, false, false, false, false);

            switch (Phase)
            {
                case 3:
                    if (PhaseCounter >= 60 && PhaseCounter < 110) inpt.y = 1;
                    if (PhaseCounter >= 80) inpt.x = 1;
                    if (PhaseCounter >= 85) inpt.x = -1;
                    break;
                case 4:
                case 7:
                    inpt.x = 1;
                    inpt.y = -1;
                    inpt.downDiagonal = 1;
                    break;
                case 5:
                    inpt.x = 1;
                    inpt.downDiagonal = 1;
                    inpt.jmp = true;
                    break;
                case 6:
                    inpt.x = 1;
                    inpt.downDiagonal = 1;
                    inpt.thrw = true;
                    break;
                case 8:
                    inpt.x = 1;
                    inpt.y = 1;
                    inpt.downDiagonal = 1;
                    inpt.jmp = true;
                    break;
                case 9:
                    inpt.y = 1;
                    break;

            }

            return inpt;
        }

        public override void Destroy()
        {
            if (Cat.controller is NoirStartController) Cat.controller = null;
            this.slatedForDeletetion = true;
        }

        private void NextPhase()
        {
            Phase++;
            PhaseCounter = 0;
        }
    }

    public class NoirStartController : Player.PlayerController
    {
        public NoirStart Owner;
        public NoirStartController(NoirStart owner)
        {
            Owner = owner;
        }

        public override Player.InputPackage GetInput()
        {
            return Owner.GetInput();
        }
    }
}