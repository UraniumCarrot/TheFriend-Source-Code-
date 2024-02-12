using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HUD;
using TheFriend.HudThings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    internal static void HUDOnInitSinglePlayerHud(HUD.HUD self, RoomCamera cam)
    {
        if (cam.room.game.IsStorySession && !cam.room.game.rainWorld.ExpeditionMode &&
            self.owner is Player && cam.room.game.StoryCharacter == Plugin.NoirName)
        {
            if (Master != null)
                self.AddPart(new HuntQuestHUD(self));
        }
    }

    public class HuntQuestHUD : HudPart //To any poor soul entering here, I apologize in advance. I must uphold the Rain World's poor and uncomprehensible coding standards.
    {
        public class HuntCreatureSprite : CreatureSprite
        {
            public bool? MarkedForDeletion;
            public HuntCreatureSprite(CreatureTemplate.Type type) : base(type) { }
        }
        
        private readonly FContainer fContainer;
        private readonly Dictionary<HuntQuest, List<HuntCreatureSprite>> iconsContainer;
        private readonly Queue<FadeRequest> fadeQueue;
        private readonly FSpriteEx glowSprite;
        private readonly List<FSprite> separators;
        private float separatorFade;
        private float separatorLastFade;
        private HuntCreatureSprite winIcon;
        private byte winScript;
        private Vector2 containerPos;
        private Vector2 containerLastPos;
        private float fade;
        private float lastFade;
        private float minFade;
        private float maxFade;
        private int fadeDelay;
        private float iconMargin;
        private float lastIconMargin;
        private float questMargin;
        private float lastQuestMargin;
        private bool visible;
        private bool scriptRunning;

        public readonly Player Owner;

        public HuntQuestHUD(HUD.HUD hud) : base(hud)
        {
            fContainer = hud.fContainers[1];
            iconsContainer = new Dictionary<HuntQuest, List<HuntCreatureSprite>>();
            containerPos = new Vector2(20.2f, Screen.height - 42.8f);
            containerLastPos = containerPos;
            visible = false;
            scriptRunning = false;
            glowSprite = new FSpriteEx("Futile_White");
            glowSprite.shader = hud.rainWorld.Shaders["FlatLight"];
            separators = new List<FSprite>();
            separatorFade = 1f;
            separatorLastFade = 1f;
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

        public bool Visible => visible || !scriptRunning && (hud.map.visible || iconsContainer.Values.Any(x => x.Any(y => y.MarkedForDeletion != null)));

        public override void Update()
        {
            if (Master == null) return;
            if (Master.StorySession.game.GamePaused) return;
            base.Update();
            WinUpdate();
            RoomUpdate();
            MessageUpdate();
            FadeQueueUpdate();

            foreach (var item in iconsContainer.ToArray())
            {
                foreach (var icon in item.Value.ToArray())
                {
                    icon.Update();
                    if (fade >= maxFade && icon.MarkedForDeletion != null)
                    {
                        if (icon.MarkedForDeletion.Value == false)
                        {
                            if (fadeDelay == 0) fadeDelay = 50;
                            icon.MarkedForDeletion = true;
                        }
                        if (fadeDelay == 0 && icon.MarkedForDeletion is true)
                        {
                            icon.targetAlpha -= 0.025f;
                            if (icon.alpha <= 0f)
                                RemoveIcon(item.Key, icon);
                        }
                    }
                }
            }
            glowSprite.Update();

            if (Visible)
            {
                iconMargin += 3f;
                questMargin += 1f;
            }
            else
            {
                iconMargin -= 3f;
                questMargin -= 1f;
            }
            iconMargin = Mathf.Clamp(iconMargin, 10f, 25f);
            questMargin = Mathf.Clamp(questMargin, 20f, 25f);

            if (Visible)
            {
                if (fade > maxFade) fade -= 0.025f;
                if (fade < maxFade) fade += 0.025f;
                if (separatorFade < fade * 0.5f) separatorFade += 0.025f;
            }
            else
            {
                if (fade < minFade) fade += 0.025f;
                if (fade > minFade) fade -= 0.025f;
                separatorFade -= 0.025f;
            }

            fade = Mathf.Clamp01(fade);
            lastFade = fade;
            separatorFade = Mathf.Clamp01(separatorFade);
            separatorLastFade = separatorFade;
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
            var containerDrawPos = Vector2.Lerp(containerLastPos, containerPos, timeStacker);

            for (var i = 0; i < iconsContainer.Count; i++)
            {
                var container = iconsContainer.ElementAt(i).Value;
                var questPos = containerDrawPos;
                questPos.y -= i * drawQuestMargin;

                for (var j = 0; j < container.Count; j++)
                {
                    var icon = container[j];
                    var iconPos = questPos;
                    iconPos.x += j * drawIconMargin;
                    icon.pos = Vector2.Lerp(icon.lastPos, iconPos, timeStacker);
                    icon.color = icon.LerpColor(timeStacker);
                    if (icon.MarkedForDeletion is not true && winScript < 3)
                        icon.alpha = drawFade;
                    else if (icon.MarkedForDeletion is true || winScript >= 3)
                        icon.alpha = icon.LerpAlpha(timeStacker);
                }

                var line = separators.ElementAtOrDefault(i);
                if (line == null) continue;
                var linePos = questPos;
                linePos.y -= 0.5f * drawQuestMargin;
                line.scaleX = drawIconMargin * (container.Count - 1) + 20f;
                linePos.x += line.scaleX * 0.5f - 10f;
                line.SetPosition(linePos);
                if (winScript < 3)
                    line.alpha = Mathf.Lerp(separatorLastFade, separatorFade, timeStacker);
                else
                {
                    var iconAlpha = iconsContainer.Values.SelectMany(icons => icons).First(icon => icon != winIcon).alpha;
                    if (iconAlpha < line.alpha) line.alpha = iconAlpha;
                }
            }

            glowSprite.LerpAll(timeStacker);
        }

        private void AddIcon(HuntQuest quest, CreatureTemplate.Type type)
        {
            var icon = HuntQuest.GetHuntSprite(type);
            fContainer.AddChild(icon);
            iconsContainer[quest].Add(icon);
        }
        private void RemoveIcon(HuntQuest quest, HuntCreatureSprite icon)
        {
            icon.RemoveFromContainer();
            iconsContainer[quest].Remove(icon);
            if (!iconsContainer[quest].Any())
            {
                RemoveSeparator();
                iconsContainer.Remove(quest);
                quest.Targets.CollectionChanged -= TargetsOnCollectionChanged;
            }
        }
        private void MarkIconForDeletion(HuntQuest quest, CreatureTemplate.Type type)
        {
            var icon = iconsContainer[quest].Last(x => x.CreatureType == type);
            if (quest.Completed && winIcon == null)
            {
                winIcon = icon;
                winScript = 1;
                return;
            }
            icon.MarkedForDeletion = false;
        }
        private void AddSeparator()
        {
            var sprite = new FSprite("pixel");
            separators.Add(sprite);
            fContainer.AddChild(sprite);
        }
        private void RemoveSeparator()
        {
            var sprite = separators.LastOrDefault();
            sprite?.RemoveFromContainer();
            separators.Remove(sprite);
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
                    MarkIconForDeletion(quest, oldTarget);
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
            if (iconsContainer.Any()) AddSeparator();
            iconsContainer.Add(quest, new List<HuntCreatureSprite>());
            foreach (var target in quest.Targets)
            {
                AddIcon(quest, target);
            }
            quest.Targets.CollectionChanged += TargetsOnCollectionChanged;
        }
        private void DisposeQuest(HuntQuest quest)
        {
            foreach (var icon in iconsContainer[quest])
                icon.MarkedForDeletion = true;

            quest.Targets.CollectionChanged -= TargetsOnCollectionChanged;
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

        private void WinUpdate()
        {
            if (winScript == 0) return;
            switch (winScript)
            {
                case 1:
                    visible = true;
                    if (fade >= maxFade)
                    {
                        foreach (var icon in iconsContainer.Values.SelectMany(icons => icons))
                        {
                            icon.MarkedForDeletion = null; //Prevent accidentally removing other icons
                        }
                        fadeDelay = 50;
                        winScript++;
                    }
                    break;
                case 2:
                    if (fadeDelay != 0) return;
                    glowSprite.SetPosition(winIcon.pos);
                    glowSprite.alpha = 0f;
                    glowSprite.targetAlpha = 0f;
                    glowSprite.scale = 3f;
                    fContainer.AddChild(glowSprite);
                    fadeDelay = 79;
                    hud.PlaySound(SoundID.HUD_Karma_Reinforce_Flicker);
                    winScript++;
                    break;
                case 3:
                    glowSprite.targetAlpha = 0.5f * Mathf.Pow(Mathf.InverseLerp(78f, 54f, fadeDelay), 3f);
                    glowSprite.targetAlpha += 0.06f * Mathf.Sin(fadeDelay * Mathf.Deg2Rad * Random.Range(0, 10));
                    foreach (var icon in iconsContainer.Values.SelectMany(x => x))
                    {
                        if (icon == winIcon) continue;
                        icon.targetColor = Color.Lerp(icon.CreatureColor, Color.white, Mathf.InverseLerp(78f, 54f, fadeDelay));
                        if (icon.alpha > 0.5f) icon.targetAlpha -= 0.025f;
                    }
                    if (fadeDelay == 0)
                    {
                        fadeDelay = 5;
                        winScript++;
                    }
                    break;
                case 4:
                    if (fadeDelay != 0) return;
                    hud.fadeCircles.Add(new FadeCircle(hud, winIcon.pixelSize.MaxValue(), 1f, 0.82f, 50f, 4f, winIcon.pos, fContainer));
                    hud.PlaySound(SoundID.HUD_Karma_Reinforce_Bump);
                    fadeDelay = 40;
                    winScript++;
                    break;
                case 5:
                    winIcon.MarkedForDeletion = true;
                    winScript++;
                    break;
                case 6:
                    foreach (var icon in iconsContainer.Values.SelectMany(x => x))
                    {
                        if (icon == winIcon) continue;
                        if (icon.alpha > 0f) icon.targetAlpha -= 0.025f;
                    }
                    glowSprite.targetAlpha = 0.6f * Mathf.Pow(winIcon.alpha, 2f);
                    if (glowSprite.alpha <= 0f)
                    {
                        fadeDelay = 10;
                        winScript++;
                    }
                    break;
                case 7:
                    if (fadeDelay != 0) return;
                    foreach (var quest in iconsContainer.Keys)
                        DisposeQuest(quest);
                    visible = false;
                    winIcon = null;
                    winScript = 0;
                    break;
            }
            hud.foodMeter.showCountDelay = 50;
        }

        private void RoomUpdate()
        {
            if (scriptRunning) return;
            if (Owner.room == null) return;
            if (Owner.room.abstractRoom.shelter)
            {
                if (minFade != 0f) new FadeRequest(null, 0, fadeQueue, 0f);
            }
            else
            {
                if (minFade != 0.5f) new FadeRequest(null, 0, fadeQueue, 0.5f);
            }
        }

        #region Messages
        private const string HUNTQUEST_GREETMSG = "HUNTQUEST_GREETMSG";
        private byte TutorialMessageShown;
        private void MessageUpdate()
        {
            if (TutorialMessageShown < 5)
            {
                switch (TutorialMessageShown)
                {
                    case 0:
                        scriptRunning = true;
                        visible = false;
                        fade = 0f;
                        TutorialMessageShown++;
                        break;
                    case 1:
                        if (Master.StorySession.saveState.cycleNumber != 0 && Owner.room != null && !Owner.room.abstractRoom.shelter)
                            TutorialMessageShown++;
                        break;
                    case 2:
                        hud.textPrompt.AddMessage("You feel the urge to hunt...", 10, 200, true, true);
                        new FadeRequest(true, 200, fadeQueue, null, 1f);
                        new FadeRequest(false, 100, fadeQueue, 0.5f);
                        TutorialMessageShown++;
                        break;
                    case 3:
                        if (fade >= maxFade)
                            TutorialMessageShown++;
                        break;
                    case 4:
                        if (fade <= minFade)
                        {
                            scriptRunning = false;
                            TutorialMessageShown++;
                            SaveThings.SolaceCustom.SaveStorySpecific(HUNTQUEST_GREETMSG, true, Master.StorySession);
                        }
                        break;
                }
            }
        }
        #endregion

        private class FadeRequest
        {
            public int Delay;
            public bool? Visible;
            public float? MaxFade;
            public float? MinFade;
            public FadeRequest(bool? visible, int delay, Queue<FadeRequest> queue, float? minFade = null, float? maxFade = null)
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
                if (currentRequest.Visible != null) visible = currentRequest.Visible.Value;
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
            SaveThings.SolaceCustom.LoadStorySpecific(HUNTQUEST_GREETMSG, out bool tutorialMessageShown, Master.StorySession);
            if (tutorialMessageShown) TutorialMessageShown = byte.MaxValue;
        }
    }
}