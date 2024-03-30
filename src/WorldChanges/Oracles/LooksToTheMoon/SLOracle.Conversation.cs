using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;

namespace TheFriend.WorldChanges.Oracles.LooksToTheMoon;

public partial class SLOracle
{
    internal static void MoonConversationOnAddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        if (!QuickWorldData.SolaceCampaign)
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

        if (self.currentSaveFile == Plugin.NoirName)
        {
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                /*
                Say("Hello little <PlayerName>.");
                Say("What are you? If I had my memories I would know...");
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
                    "They are very cute though.");*/
                
                Say2("Ah... Hello little creature.",0,2);
                Say2("Sorry, give me a moment. I'm still getting used to this...",0,5);
                Say2("...",0,2);
                Say("Thank you. Doing that takes a lot of energy. Can you understand me now?");
                /*var meowPos = self.myBehavior.oracle.firstChunk.pos;
                if (self.myBehavior.player.SlugCatClass == Plugin.NoirName)
                {
                    meowPos = self.myBehavior.player.firstChunk.pos;
                }
                else
                {
                    var noir = self.myBehavior.PlayersInRoom.FirstOrDefault(p => p.SlugCatClass == Plugin.NoirName);
                    if (noir != null) meowPos = noir.firstChunk.pos;
                }
                self.myBehavior.oracle.room.PlaySound(NoirCatto.Meow1SND, meowPos);*/
                Say("It seems so! I'm sorry if that caused any discomfort.");
                Say("Amazing... three visitors, each right after the other!");
                Say("Do you know them? There's a little one - I think it may be a child - and a fluffy blue one!");
                Say("Not as fluffy as you, though... your fur is so thick! It's perfectly suited to the blizzard.");
                Say2("...",0,5);
                Say("How curious! You're one of their kind, yet you're also so different.<LINE>" +
                    "You remind me of how felines looked, back when our creators were around.");
                Say("They were kept as pets, but also were quite capable on their own.");
                Say("I don't think you descended from them, they're surely extinct by now.");
                Say("Do your differences come from evolving in some far-away place?<LINE>" +
                    "Or are you purposed, to look like them?");
                Say("What a marvel you are, fluffy one!");
                Wait(10);
                
                Say("I apologize, I don't fully understand the noises you make...<LINE>" +
                    "They are very cute though.");
                return;
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

        if (self.currentSaveFile == Plugin.FriendName || self.currentSaveFile == Plugin.DragonName) //TODO: Give poacher proper unique dialogue
        {
            bool IsFriend = self.currentSaveFile == Plugin.FriendName;
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                Say2("...", 0, 2);
                Wait(5);
                Say2("Ah... h-hello.", 0, 2);
                Say2("Please... Give me a moment.", 0, 2);
                Say2("...", 0, 2);
                Wait(100);
                Say2("Thank you. Sorry, that took a lot of energy... But it seems you can understand me now.<LINE>" + 
                     "I hope that didn't hurt.", 0, 2);
                Say2("It is amazing to see more of your kind again. It's been such a long time!", 0, 2);
                Say2("My last regular visitor passed long ago. " + 
                     ((IsFriend) ? 
                         "They looked a lot like you! More aquatic than fluffy, though..." : 
                         "They were much older than you."), 0, 2);
                Say2("They helped me a lot. Actually, your kind seems to do that often.", 0, 2);
                Say2("Are you here to help me too? I don't see how you could." +
                    ((IsFriend)? 
                        "<LINE>But, I know someone else who really needs it." : 
                        "<LINE>You seem very young to be exploring alone. It's dangerous."), 0, 2);
                if (IsFriend)
                {
                    Say2("He's my neighbor, Five Pebbles. I... he-he's hurt.", 0, 2);
                    Say2("So badly... My last visitor reinstated my ability to communicate with him.", 0, 2);
                    Say2("And now I've lost him. Again.", 0, 2);
                    Say2("Please... if there's anything you can do, do it for him. He needs help.", 0, 2);
                    Say2("Even just keeping him company will do. He is suffering, rotting in his chamber.", 0, 2);
                    Say2("You... may need to make him less dangerous first. Somehow.<LINE>" + 
                         "I know he'd appreciate anything at all.", 0, 2);
                    Say2("You are welcome to stay with me as long as you like, but I urge you to seek him instead.<LINE>" + 
                         "He needs it much more than I.", 0, 2);
                }
                else
                {
                    Say2("You are welcome to stay as long as you like, but be careful, little creature.<LINE>" +
                        "It doesn't seem like the cold suits you well.", 0, 2);
                }
                
                return;
            }
        }

        orig(self);
    }
}

