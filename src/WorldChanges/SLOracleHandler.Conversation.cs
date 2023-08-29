using UnityEngine;

namespace TheFriend.WorldChanges;

public partial class SLOracleHandler
{
    private static void MoonConversationOnAddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        if (!FriendWorldState.SolaceWorldstate)
        {
            orig(self);
            return;
        }

        #region Helpers
        void Say(string text)
        {
            self.events.Add(new Conversation.TextEvent(self, 0, text, 0));
        }
        void Say2(string text, int initialWait,  int textLinger)
        {
            self.events.Add(new Conversation.TextEvent(self, initialWait, text, textLinger));
        }
        void Wait(int initialWait)
        {
            self.events.Add(new Conversation.WaitEvent(self, initialWait));
        }
        #endregion

        if (self.currentSaveFile == Plugin.NoirName) //todo: Update dialogue to match Solace
        {
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 2:
                        Say2("Get... get away... black... thing.", 30, 10);
                        Say2("Please... thiss all I have left.", 0, 10);
                        return;
                    case 3:
                        Say2("YOU!", 30, 10);
                        Say2("...You ate... me. Please go away. I won't speak... to you.<LINE>" +
                             "I... CAN'T speak to you... because... you ate...me...", 60, 0);
                        return;
                    case 5:
                        Say("Hello little <PlayerName>.");
                        Say("What are you? If I had my memories I would know...");
                        if (self.State.playerEncounters > 0 && self.State.playerEncountersWithMark == 0)
                        {
                            Say("Perhaps... I saw you before?");
                        }

                        Say("You must be very brave to have made it all the way here.<LINE>" +
                            "You dont seem suited to the ocean around my remains.");
                        Say("I am sorry to say, that I have nothing for you. Not even my memories.");
                        Say2("Or did I say that already?", 0, 5);
                        Say("You have been given the gift of communication.<LINE>" +
                            "Did you meet my neighbour, Five Pebbles?");
                        Say("If so.. You must already understand how little of me remains functional.");
                        Wait(10);
                        Say("Your.. biology... feels familiar.");
                        Say2("Almost like a distant memory..<LINE>" +
                             "Be it a very fluffy memory!", 0, 5);
                        Say("Speaking of which, my previous visitor wasnt quite as fluffy as you.<LINE>" +
                            "More... Round. They also didnt seem to walk or crawl the same as you.");
                        Say2("Maybe you're distantly related..?", 0, 2);
                        Say("I must admit your kind's visits help keep what remains of me busy!");
                        Say("I can only hope to remember them all for as long as I can in my current state.");
                        Say("Maybe you know the round one? They left some time ago.<LINE>" +
                            "My last overseer watched them head far to the west, past the...");
                        Say2("...", 0, 2);
                        Wait(5);
                        Say("It appears I dont remember the locales name. I'm sorry little one.");
                        Say("You are welcome to stay as long as you desire. It is nice to have someone to talk to!");
                        Say("I apologize I don't fully understand the noises you make...<LINE>" +
                            "They are very cute though.");
                        return;
                    default:
                        orig(self);
                        return;
                }
            }

            if (self.id == Conversation.ID.MoonSecondPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 4:
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                        {
                            Say2("Hello there! You again!", 30, 0);
                            Say("I wonder what it is that you want?");
                            return;
                        }

                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                        {
                            Say2("Oh. You.", 30, 0);
                            Say("I wonder what it is that you want?");
                            Say("I have had scavengers come by before. Scavengers!<LINE>" +
                                "And they left me alive!<LINE>" +
                                "But... I have told you that already, haven't I?");
                            Say("You must excuse me if I repeat myself. My memory is bad.<LINE>" +
                                "I used to have a pathetic five neurons... And then you ate one.<LINE>" +
                                "Maybe I've told you that before as well.");
                            return;
                        }

                        Say2("Oh. You.", 30, 0);
                        return;

                    case 5:
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                        {
                            Say2("You again.", 0, 10);
                            return;
                        }

                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                        {
                            Say2("Oh, hello!", 0, 10);
                            Say("I wonder what it is that you want?");
                            Say("There is nothing here. Not even my memories remain.");
                            Say2("Even the scavengers that come here from time to time leave with nothing. But... I have told you that already, haven't I?", 30, 0);
                            if (ModManager.MSC && self.myBehavior.CheckSlugpupsInRoom())
                            {
                                Say2("I do enjoy the company though. You and your family are always welcome here.", 0, 5);
                                return;
                            }

                            if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                            {
                                Say2("I do enjoy the company of you and your friend though, <PlayerName>.", 0, 5);
                                Say2("You're welcome to stay a while, fluffy little thing.", 0, 5);
                                return;
                            }

                            Say2("I do enjoy the company though. You're welcome to stay a while, fluffy little thing.", 0, 5);
                            return;
                        }

                        Say2("Oh, hello.", 0, 10);
                        return;

                    default:
                        orig(self);
                        return;
                }
            }

            if (self.id == Conversation.ID.Moon_Misc_Item)
            {
                if (self.describeItem == SLOracleBehaviorHasMark.MiscItemType.Spear)
                {
                    Say2("It's a piece of sharpened rebar... Others of your kind seemed very proficient at using it,<LINE>" +
                         "but given your anatomy, I believe you use different ways to catch your meal!", 10, 0);
                    return;
                }
            }
        }

        if (self.currentSaveFile == Plugin.FriendName)
        {
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                Say2("...", 0, 2);
                Wait(5);
                Say2("Ah, yes.", 0, 2);
                Say2("The Slugcats.", 0, 999999);
                return;
            }
        }

        orig(self);
    }
}

