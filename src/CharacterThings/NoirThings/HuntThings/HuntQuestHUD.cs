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
            Master ??= new HuntQuestMaster(cam.room.game.GetStorySession);
            self.AddPart(new HuntQuestHUD(self));
        }
    }

    public class HuntQuestHUD : HudPart
    {
        private readonly FContainer fContainer;
        private readonly Dictionary<HuntQuest, List<CreatureSymbol>> IconsContainer;
        private Vector2 ContainerPos;
        private Vector2 ContainerLastPos;

        public Player Owner;

        public HuntQuestHUD(HUD.HUD hud) : base(hud)
        {
            fContainer = hud.fContainers[1];
            IconsContainer = new Dictionary<HuntQuest, List<CreatureSymbol>>();
            ContainerPos = new Vector2(20.2f, 725.2f); //todo: use screen size?
            ContainerLastPos = ContainerPos;

            Owner = (Player)hud.owner;

            Master.Quests.CollectionChanged += QuestsOnCollectionChanged;
            InitializeContainer();
        }

        public override void Update()
        {
            base.Update();
            foreach (var icon in IconsContainer.Values.SelectMany(icons => icons))
                icon.Update();

            ContainerLastPos = ContainerPos;
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            for (var i = 0; i < IconsContainer.Count; i++)
            {
                var container = IconsContainer.ElementAt(i).Value;
                var questPos = ContainerDrawPos(timeStacker);
                questPos.y -= i * 20;
                for (var j = 0; j < container.Count; j++)
                {
                    var icon = container[j];
                    var iconPos = questPos;
                    iconPos.x += j * 30;
                    icon.Draw(timeStacker, iconPos);
                }
            }
        }

        private void AddIcon(HuntQuest quest, CreatureTemplate.Type type)
        {
            var icon = new CreatureSymbol(HuntQuest.SymbolDataFromType(type), fContainer);
            IconsContainer[quest].Add(icon);
            icon.Show(false);
        }
        private void RemoveIcon(HuntQuest quest, CreatureTemplate.Type type)
        {
            var icon = IconsContainer[quest].First(x => x.critType == type);
            icon.RemoveSprites();
            IconsContainer[quest].Remove(icon);
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
            IconsContainer.Add(quest, new List<CreatureSymbol>());
            foreach (var target in quest.Targets)
            {
                AddIcon(quest, target);
            }
            quest.Targets.CollectionChanged += TargetsOnCollectionChanged;
        }
        private void DisposeQuest(HuntQuest quest)
        {
            foreach (var creatureSymbol in IconsContainer[quest])
            {
                creatureSymbol.RemoveSprites();
            }
            quest.Targets.CollectionChanged -= TargetsOnCollectionChanged;
            IconsContainer.Remove(quest);
        }

        //-----

        private void QuestsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var quest in IconsContainer.Keys.ToArray())
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
            return Vector2.Lerp(ContainerLastPos, ContainerPos, timeStacker);
        }
    }
}