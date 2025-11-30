
using BepInEx;
using RWCustom;


namespace CreatureChat
{

    [BepInPlugin(MOD_ID, "CreatureChat", "0.2.0")]
    class Plugin : BaseUnityPlugin
    {

        private const string MOD_ID = "CreatureChat";

        public bool hasClock;
        private static FAtlas atlas;

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            // Put your custom hooks here!
            //On.Player.Update += Player_Update;
            On.HUD.HUD.ctor += HUD_ctor;
            On.HUD.HUD.Update += HUD_Update;
            On.HUD.HUD.Draw += HUD_Draw;
        }
        private void HUD_ctor(On.HUD.HUD.orig_ctor orig, HUD.HUD self, FContainer[] fContainers, RainWorld rainWorld, HUD.IOwnAHUD owner)
        {
            orig.Invoke(self, fContainers, rainWorld, owner);
            HUDModuleManager.HUDModules.Add(self, new HUDModuleManager.HUDModule(self));
        }


        private void HUD_Draw(On.HUD.HUD.orig_Draw orig, HUD.HUD self, float timeStacker)
        {
            orig.Invoke(self, timeStacker);
            HUDModuleManager.HUDModules.TryGetValue(self, out var HUDModule);
            if (HUDModule != null)
            {
                foreach (var item in HUDModule.creatureDialogBoxes)
                {
                    item.Draw(timeStacker);
                }
            }

        }

        private void HUD_Update(On.HUD.HUD.orig_Update orig, HUD.HUD self)
        {
            orig.Invoke(self);
            HUDModuleManager.HUDModules.TryGetValue(self,out var HUDModule);
            if (HUDModule != null)
            {
                foreach (var item in HUDModule.creatureDialogBoxes)
                {
                    item.Update();
                }
                foreach (var item in HUDModule.creatureChatTxes)
                {
                    item.Update(true);
                }
            }
        }





        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }

        // Implement MeanLizards

        public static void Log(string text)
        {
            Custom.LogWarning("[CreatureChat]" + text);
        }
    }
}