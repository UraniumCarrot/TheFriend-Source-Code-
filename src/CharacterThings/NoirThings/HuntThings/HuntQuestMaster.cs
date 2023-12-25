using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    public static HuntQuestMaster Master;
    public class HuntQuestMaster
    {
        private const string HUNTQUEST_DATA = "HUNTQUEST_DATA";
        private readonly List<WeakReference> _huntedCreatures; //AbstractCreature
        
        public readonly ObservableCollection<HuntQuest> Quests;
        public StoryGameSession StorySession;
        public bool Completed;
        public RewardPhase NextRewardPhase;

        public HuntQuestMaster()
        {
            _huntedCreatures = new List<WeakReference>();
            Quests = new ObservableCollection<HuntQuest>();
        }

        public void LoadOrCreateQuests()
        {
            Quests.Clear();
            Completed = false;
            NextRewardPhase = RewardPhase.Normal;

            var newQuests = new List<HuntQuest>();

            if (SaveThings.SolaceCustom.LoadStorySpecific(HUNTQUEST_DATA, out List<HuntQuest> loadedQuests, StorySession))
                newQuests = loadedQuests;

            if (!newQuests.Any())
            {
                if (StorySession.saveState.cycleNumber == 0) return; //Do nothing on first cycle
                var karma = StorySession.saveState.deathPersistentSaveData.karmaCap;
                if (karma >= 9) return; //Max karma reached!
                newQuests = HuntQuestTemplates.FromKarma(0); //todo: use karma
            }

            foreach (var quest in newQuests)
                Quests.Add(quest);
        }

        public void SaveQuestProgress()
        {
            var questsToSave = new List<HuntQuest>();
            if (!Completed) questsToSave = Quests.ToList();
            SaveThings.SolaceCustom.SaveStorySpecific(HUNTQUEST_DATA, questsToSave, StorySession);
        }

        public void QuestComplete(HuntQuest quest)
        {
            //Quests.Clear();
            Completed = true;
            NextRewardPhase = RewardPhase.IncreaseKarmaCap;
        }
        
        private void TargetHunted(AbstractCreature target)
        {
            _huntedCreatures.Add(new WeakReference(target));
            foreach (var quest in Quests.ToArray())
            {
                var customType = HuntQuest.TypeTranslator(target.creatureTemplate.type);
                if (!quest.Targets.Remove(customType))
                    quest.Targets.Remove(target.creatureTemplate.type);
                if (quest.Completed) QuestComplete(quest);
            }
        }
        public void AddEat(PhysicalObject eatenobject)
        {
            if (eatenobject is not Creature crit) return;
            if (!crit.dead || crit.killTag != null && crit.killTag.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                if (!AlreadyHunted(crit.abstractCreature))
                    TargetHunted(crit.abstractCreature);
            }
        }

        #region Helper methods
        private bool AlreadyHunted(AbstractCreature abstractCreature)
        {
            var huntedCreatures = new List<AbstractCreature>();

            foreach (var weakref in _huntedCreatures)
            {
                if (weakref.Target is not AbstractCreature crit)
                    _huntedCreatures.Remove(weakref);
                else
                    huntedCreatures.Add(crit);
            }
            return huntedCreatures.Contains(abstractCreature);
        }
        #endregion
    }
}