
using CreatureChat;
using HUD;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;

namespace CreatureChat
{
    public class HUDModuleManager
    {
        public static readonly ConditionalWeakTable<HUD.HUD, HUDModule> HUDModules = new ConditionalWeakTable<HUD.HUD, HUDModule>();


        public class HUDModule
        {
            WeakReference<HUD.HUD> playerRef;

            public HUDModule(HUD.HUD hud)
            {
                playerRef = new WeakReference<HUD.HUD>(hud);
            }

            public List<CreatureDialogBox> creatureDialogBoxes = new List<CreatureDialogBox>();
            public List<CreatureChatTx> creatureChatTxes = new List<CreatureChatTx>(); 


        }
    }
}
