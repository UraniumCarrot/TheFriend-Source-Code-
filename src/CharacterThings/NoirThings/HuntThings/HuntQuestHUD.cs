using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HUD;
using UnityEngine;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    private static void HUDOnInitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);
        if (cam.room.game.IsStorySession && !cam.room.game.rainWorld.ExpeditionMode &&
            self.owner is Player && cam.room.game.StoryCharacter == Plugin.NoirName)
        {
            if (Master != null)
                self.AddPart(new HuntQuestHUD(self));
        }
    }

    public class HuntQuestHUD : HudPart
    {
        private readonly FContainer fContainer;
        private readonly Dictionary<HuntQuest, List<CreatureSymbol>> iconsContainer;
        private Vector2 containerPos;
        private Vector2 containerLastPos;
        private float fade;
        private float lastFade;
        private float minFade;
        private float maxFade;
        private int fadeDelay;
        private Queue<FadeRequest> fadeQueue;
        private float iconMargin;
        private float lastIconMargin;
        private float questMargin;
        private float lastQuestMargin;

        public readonly Player Owner;
        public bool Visible;

        public HuntQuestHUD(HUD.HUD hud) : base(hud)
        {
            fContainer = hud.fContainers[1];
            iconsContainer = new Dictionary<HuntQuest, List<CreatureSymbol>>();
            containerPos = new Vector2(20.2f, 725.2f); //todo: use screen size?
            containerLastPos = containerPos;
            Visible = true;

            fade = 1f;
            lastFade = 1;
            minFade = 0f;
            maxFade = 1f;
            fadeQueue = new Queue<FadeRequest>();
            iconMargin = 1f;
            lastIconMargin = 1f;
            questMargin = 1f;
            lastQuestMargin = 1f;

            Owner = (Player)hud.owner;

            Master.Quests.CollectionChanged += QuestsOnCollectionChanged;
            InitializeContainer();
            LoadMiscData();
        }

        public override void Update()
        {
            base.Update();
            MessageUpdate();
            FadeQueueUpdate();

            foreach (var icon in iconsContainer.Values.SelectMany(icons => icons))
                icon.Update();

            if (hud.map.visible)
            {
                iconMargin += 1f;
                questMargin += 1f;
            }
            else
            {
                iconMargin -= 1f;
                questMargin -= 1f;
            }
            iconMargin = Mathf.Clamp(iconMargin, 10f, 20f);
            questMargin = Mathf.Clamp(questMargin, 18f, 20f);

            if ((Visible || hud.map.visible) && fade < maxFade)
                fade += 0.025f;
            else if (fade > minFade)
                fade -= 0.025f;

            fade = Mathf.Clamp01(fade);
            lastFade = fade;

            lastIconMargin = iconMargin;
            lastQuestMargin = questMargin;
            containerLastPos = containerPos;
            fadeDelay.Tick();
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            var drawFade = Mathf.Lerp(lastFade, fade, timeStacker);
            var drawIconMargin = Mathf.Lerp(lastIconMargin, iconMargin, timeStacker);
            var drawQuestMargin = Mathf.Lerp(lastQuestMargin, questMargin, timeStacker);

            for (var i = 0; i < iconsContainer.Count; i++)
            {
                var container = iconsContainer.ElementAt(i).Value;
                var questPos = ContainerDrawPos(timeStacker);
                questPos.y -= i * drawQuestMargin;
                for (var j = 0; j < container.Count; j++)
                {
                    var icon = container[j];
                    var iconPos = questPos;
                    iconPos.x += j * drawIconMargin;
                    icon.Draw(timeStacker, iconPos);
                    icon.symbolSprite.alpha = drawFade;
                }
            }
        }

        private void AddIcon(HuntQuest quest, CreatureTemplate.Type type)
        {
            var icon = new CreatureSymbol(HuntQuest.SymbolDataFromType(type), fContainer);
            iconsContainer[quest].Add(icon);
            icon.Show(false);
        }
        private void RemoveIcon(HuntQuest quest, CreatureTemplate.Type type)
        {
            var icon = iconsContainer[quest].First(x => x.critType == type);
            icon.RemoveSprites();
            iconsContainer[quest].Remove(icon);
        }

        private void InitializeContainer()
        {
            foreach (var quest in Master.Quests) NewQuest(quest);
        }

        #region Targets
        private void TargetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var quest = ((ObservableTargetCollection<CreatureTemplate.Type>)sender).Owner;
            if (e.OldItems != null) //Targets have been removed
            {
                foreach (CreatureTemplate.Type oldTarget in e.OldItems)
                {
                    RemoveIcon(quest, oldTarget);
                }
            }
            if (e.NewItems != null) //Targets have been added
            {
                foreach (CreatureTemplate.Type newTarget in e.NewItems)
                {
                    AddIcon(quest, newTarget);
                }
            }
        }
        #endregion

        #region Quests
        private void NewQuest(HuntQuest quest)
        {
            iconsContainer.Add(quest, new List<CreatureSymbol>());
            foreach (var target in quest.Targets)
            {
                AddIcon(quest, target);
            }
            quest.Targets.CollectionChanged += TargetsOnCollectionChanged;
        }
        private void DisposeQuest(HuntQuest quest)
        {
            foreach (var creatureSymbol in iconsContainer[quest])
            {
                creatureSymbol.RemoveSprites();
            }
            quest.Targets.CollectionChanged -= TargetsOnCollectionChanged;
            iconsContainer.Remove(quest);
        }

        //-----

        private void QuestsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var quest in iconsContainer.Keys.ToArray())
                    DisposeQuest(quest);
                return;
            }
            if (e.OldItems != null) //Quests have been removed
            {
                foreach (HuntQuest oldQuest in e.OldItems)
                    DisposeQuest(oldQuest);
            }
            if (e.NewItems != null) //Quests have been added
            {
                foreach (HuntQuest newQuest in e.NewItems)
                    NewQuest(newQuest);
            }
        }
        #endregion

        private Vector2 ContainerDrawPos(float timeStacker)
        {
            return Vector2.Lerp(containerLastPos, containerPos, timeStacker);
        }

        #region Messages
        private const string HUNTQUEST_GREETMSG = "HUNTQUEST_GREETMSG";
        private bool TutorialMessageShown;
        private void MessageUpdate()
        {
            if (!TutorialMessageShown)
            {
                Visible = false;
                fade = 0f;
                if (Master.StorySession.saveState.cycleNumber != 0 && Owner.room != null && !Owner.room.abstractRoom.shelter)
                {
                    hud.textPrompt.AddMessage("You feel the urge to hunt...", 10, 300, true, true);
                    TutorialMessageShown = true;
                    new FadeRequest(true, 300, fadeQueue);
                    new FadeRequest(false, 100, fadeQueue, 0.5f);
                    SaveThings.SolaceCustom.SaveStorySpecific(HUNTQUEST_GREETMSG, false, Master.StorySession); //todo true
                }
            }
        }
        #endregion

        private class FadeRequest
        {
            public int Delay;
            public bool Visible;
            public float? MaxFade;
            public float? MinFade;
            public FadeRequest(bool visible, int delay, Queue<FadeRequest> queue, float? minFade = null, float? maxFade = null)
            {
                Visible = visible;
                Delay = delay;
                MaxFade = maxFade;
                MinFade = minFade;
                queue.Enqueue(this);
            }
        }
        private FadeRequest currentRequest;
        private void FadeQueueUpdate()
        {
            if (fadeDelay == 0 && currentRequest != null)
            {
                Visible = currentRequest.Visible;
                if (currentRequest.MaxFade != null) maxFade = currentRequest.MaxFade.Value;
                if (currentRequest.MinFade != null) minFade = currentRequest.MinFade.Value;
                currentRequest = null;
            }
            if (currentRequest == null && fadeQueue.Any())
            {
                currentRequest = fadeQueue.Dequeue();
                fadeDelay = currentRequest.Delay;
            }
        }

        private void LoadMiscData()
        {
            SaveThings.SolaceCustom.LoadStorySpecific(HUNTQUEST_GREETMSG, out TutorialMessageShown, Master.StorySession);
        }
    }
}