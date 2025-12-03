using CreatureChat;
using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureChat
{
    public class CreatureChatTx: UpdatableAndDeletable
    {
        public class TextEvent
        {
            public CreatureChatTx owner;

            public string text;

            public CreatureDialogBox dialogBox;

            public int age;

            public int initWait;

            public int extraLinger;

            public bool activated = false;

            public SoundID partWithSound;

            private bool loop;

            private float vol;

            private float pitch;

            public bool IsOver
            {
                get
                {
                    if (age < initWait || !activated)
                    {
                        return false;
                    }

                    return dialogBox.CurrentMessage == null;
                }
            }

            public TextEvent(CreatureChatTx owner, string text, CreatureDialogBox dialogBox, int initWait, int extraLinger = 40, SoundID partWithSound = null, bool loop = false, float vol = 1f, float pitch = 1f)
            {
                this.owner = owner;
                this.text = text;
                this.dialogBox = dialogBox;
                this.initWait = initWait;
                this.partWithSound = partWithSound;
                this.extraLinger = extraLinger;
                this.loop = loop;
                this.pitch = pitch;
                this.vol = vol;
            }

            public void Activate()
            {
                activated = true;
                if (text != "")
                {
                    dialogBox.NewMessage(text, extraLinger);
                }

                if (partWithSound != null)
                {
                    owner.room.PlaySound(partWithSound, owner.chatter.firstChunk, loop, vol, pitch);
                }

            }

            public void Update()
            {
                if (!activated && age >= initWait)
                {
                    Activate();
                }

                age++;
            }
        }

        public class PauseEvent : TextEvent
        {
            public PauseEvent(CreatureChatTx owner, CreatureDialogBox dialogBox, int initWait, int extraLinger = 40)
                : base(owner, "", dialogBox, initWait, extraLinger)
            {
                this.dialogBox = dialogBox;
            }
        }

        public CreatureDialogBox dialogBox;

        public PhysicalObject chatter;

        public int extraLingerFactor = 0;

        public int age;

        public int preWaitCounter;

        public List<TextEvent> events = new List<TextEvent>();

        public bool textPrompted = false;

        public bool inited = false;

        public bool canInterruptedByStun;

        public bool canInterruptedByDead;

        public string textPrompt;

        public int textPromptWait;

        public int textPromptTime;

        public string talkText;


        public CreatureChatTx(Room room, int preWaitCounter, Player chatter, string talkText, bool canInterruptedByStun = false, bool canInterruptedByDead = false, string textprompt = "", int textPromptWait = 0, int textPromptTime = 320)
        {
            base.room = room;
            this.preWaitCounter = preWaitCounter;
            this.canInterruptedByStun = canInterruptedByStun;
            this.canInterruptedByDead = canInterruptedByDead;
            this.chatter = chatter;
            textPrompt = textprompt;
            this.textPromptWait = textPromptWait;
            this.textPromptTime = textPromptTime;
            this.talkText = talkText;

            extraLingerFactor = ((room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese) ? 1 : 0);
            if (textPrompt == string.Empty)
            {
                textPrompted = true;
            }
            try
            {
                HUDModuleManager.HUDModules.TryGetValue(room.game.cameras[0].hud, out var HUDModule);
                if (HUDModule != null)
                {
                    dialogBox = new CreatureDialogBox(room.game.cameras[0].hud,chatter);
                    HUDModule.creatureDialogBoxes.Add(dialogBox);
                    HUDModule.creatureChatTxes.Add(this);
                }

            }
            catch { }
        }
        
        public string Translate(string orig)
        {
            string text = room.game.rainWorld.inGameTranslator.Translate(orig);
            Plugin.Log($"Original:{orig}\nTranslate:{text}");
            return text;
        }

        private void PromptText()
        {
            if (!textPrompted && room.game.cameras != null && room.game.cameras[0].hud != null)
            {
                if (room.game.cameras[0].hud.textPrompt == null)
                {
                    room.game.cameras[0].hud.AddPart(new TextPrompt(room.game.cameras[0].hud));
                }

                room.game.cameras[0].hud.textPrompt.AddMessage(Translate(textPrompt), textPromptWait, textPromptTime, darken: true, hideHud: true);
                textPrompted = true;
            }
        }

        private void SetUp()
        {
            if (room.game.cameras == null || room.game.cameras[0].hud == null)
            {
                Plugin.Log($"Set up failure : {room.game.cameras},{room.game.cameras[0].hud}");
                return;
            }

            for (int i = 0; i < room.game.cameras.Length; i++)
            {
                if (room.game.cameras[i].hud != null && room.game.cameras[i].followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && room.game.cameras[i].followAbstractCreature.Room == room.abstractRoom && room.game.cameras[i].hud.dialogBox == null)
                {
                    room.game.cameras[i].hud.InitDialogBox();
                }
            }
            if (dialogBox != null)
            {
                AddTextEvents(dialogBox);
            }
        }

        public virtual void AddTextEvents(CreatureDialogBox dialogBox)
        {
            string raw = Translate(talkText);
            if (string.IsNullOrEmpty(raw)) return;

            string[] segments = raw.Split(new[] { "<NEXT>" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < segments.Length; i++)
            {
                string seg = segments[i].Trim();
                if (seg.Length == 0) continue;

                // 每段都是一个独立事件
                events.Add(new CreatureChatTx.TextEvent(this, seg, dialogBox, 0, 60));
            }
            inited = true;
        }

        public override void Update(bool eu)
        {
            if (base.slatedForDeletetion)
            {
                return;
            }

            base.Update(eu);
            if (!textPrompted)
            {
                PromptText();
                return;
            }

            if (!inited)
            {
                if (room.game.cameras[0].hud.textPrompt.currentlyShowing == TextPrompt.InfoID.Nothing)
                {
                    SetUp();
                }

                return;
            }

            AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
            if (firstAlivePlayer == null)
            {
                return;
            }

            if (chatter == null && room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
            {
                chatter = firstAlivePlayer.realizedCreature as Player;
            }

            age++;
            if (age <= preWaitCounter)
            {
                return;
            }

            if (events.Count == 0 || !ChatterCanTalk())
            {
                Interrupted();
                return;
            }

            TextEvent textEvent = events[0];
            textEvent.Update();
            if (textEvent.IsOver)
            {
                events.RemoveAt(0);
            }
        }

        public void Interrupted()
        {
            HUDModuleManager.HUDModules.TryGetValue(room.game.cameras[0].hud, out var HUDModule);
            if (HUDModule != null)
            {
                dialogBox.EndCurrentMessageNow();
                HUDModule.creatureChatTxes.Remove(this);
            }
            Destroy();

        }

        public bool ChatterCanTalk()
        {
            if (!canInterruptedByStun && !canInterruptedByDead) return true;

            if (chatter == null) return true;

            if (chatter is Creature c)
            {
                if (canInterruptedByDead && c.dead) return false;

                if (canInterruptedByStun && c.Stunned) return false;
            }

            return true;
        }


        public override void Destroy()
        {
            base.Destroy();
        }
    }
}   
