using System.Collections.Generic;
using HUD;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CreatureChat
{
    public static class CreatureChatUtils
    {
        /// <summary>
        /// 结束指定生物的所有对话
        /// </summary>
        /// <param name="chatter">要结束对话的生物</param>
        public static void EndAllDialoguesForCreature(PhysicalObject chatter)
        {
            if (chatter == null) return;
            
            // 方法1：通过游戏相机遍历（推荐）
            EndAllDialoguesViaCameras(chatter);
            
            // 方法2：使用反射遍历ConditionalWeakTable（备用方案）
            // EndAllDialoguesViaReflection(chatter);
        }
        
        /// <summary>
        /// 通过游戏相机遍历HUD模块
        /// </summary>
        private static void EndAllDialoguesViaCameras(PhysicalObject chatter)
        {
            if (chatter == null || chatter.room?.game?.cameras == null) return;
            
            foreach (var camera in chatter.room.game.cameras)
            {
                if (camera?.hud == null) continue;
                
                EndDialoguesInHUD(camera.hud, chatter);
            }
        }
        
        /// <summary>
        /// 结束指定HUD中特定生物的所有对话
        /// </summary>
        private static void EndDialoguesInHUD(HUD.HUD hud, PhysicalObject chatter)
        {
            if (hud == null || chatter == null) return;
            
            if (HUDModuleManager.HUDModules.TryGetValue(hud, out var module))
            {
                EndDialoguesInModule(module, chatter);
            }
        }
        
        /// <summary>
        /// 结束指定模块中特定生物的所有对话
        /// </summary>
        private static void EndDialoguesInModule(HUDModuleManager.HUDModule module, PhysicalObject chatter)
        {
            if (module == null) return;
            
            // 结束对话气泡
            var boxesToRemove = new List<CreatureDialogBox>();
            foreach (var dialogBox in module.creatureDialogBoxes)
            {
                if (dialogBox != null && dialogBox.chatter == chatter)
                {
                    dialogBox.EndCurrentMessageNow();
                    boxesToRemove.Add(dialogBox);
                }
            }
            
            // 从列表中移除
            foreach (var box in boxesToRemove)
            {
                module.creatureDialogBoxes.Remove(box);
            }
            
            // 结束对话事务
            var txesToRemove = new List<CreatureChatTx>();
            foreach (var chatTx in module.creatureChatTxes)
            {
                if (chatTx != null && chatTx.chatter == chatter)
                {
                    chatTx.Interrupted();
                    txesToRemove.Add(chatTx);
                }
            }
            
            // 从列表中移除
            foreach (var tx in txesToRemove)
            {
                module.creatureChatTxes.Remove(tx);
            }
        }
        
        /// <summary>
        /// 结束指定房间内某个生物的所有对话
        /// </summary>
        /// <param name="room">房间</param>
        /// <param name="chatter">要结束对话的生物</param>
        public static void EndAllDialoguesInRoomForCreature(Room room, PhysicalObject chatter)
        {
            if (room == null || chatter == null) return;
            
            // 只处理指定房间中的HUD
            if (room.game?.cameras == null) return;
            
            foreach (var camera in room.game.cameras)
            {
                if (camera?.hud == null) continue;
                
                EndDialoguesInHUD(camera.hud, chatter);
            }
        }
        
        /// <summary>
        /// 检查指定生物是否有活跃的对话
        /// </summary>
        /// <param name="chatter">要检查的生物</param>
        /// <returns>是否有活跃对话</returns>
        public static bool HasActiveDialogue(PhysicalObject chatter)
        {
            if (chatter == null) return false;
            
            // 检查所有相机对应的HUD
            if (chatter.room?.game?.cameras == null) return false;
            
            foreach (var camera in chatter.room.game.cameras)
            {
                if (camera?.hud == null) continue;
                
                if (HUDModuleManager.HUDModules.TryGetValue(camera.hud, out var module))
                {
                    // 检查对话气泡
                    foreach (var dialogBox in module.creatureDialogBoxes)
                    {
                        if (dialogBox != null && dialogBox.chatter == chatter && dialogBox.ShowingAMessage)
                        {
                            return true;
                        }
                    }
                    
                    // 检查对话事务
                    foreach (var chatTx in module.creatureChatTxes)
                    {
                        if (chatTx != null && chatTx.chatter == chatter && chatTx.events.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取指定生物的所有对话气泡
        /// </summary>
        /// <param name="chatter">生物</param>
        /// <returns>对话气泡列表</returns>
        public static List<CreatureDialogBox> GetDialogBoxesForCreature(PhysicalObject chatter)
        {
            var result = new List<CreatureDialogBox>();
            
            if (chatter == null) return result;
            
            // 遍历所有相机
            if (chatter.room?.game?.cameras == null) return result;
            
            foreach (var camera in chatter.room.game.cameras)
            {
                if (camera?.hud == null) continue;
                
                if (HUDModuleManager.HUDModules.TryGetValue(camera.hud, out var module))
                {
                    foreach (var dialogBox in module.creatureDialogBoxes)
                    {
                        if (dialogBox != null && dialogBox.chatter == chatter)
                        {
                            result.Add(dialogBox);
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取指定生物的所有对话事务
        /// </summary>
        /// <param name="chatter">生物</param>
        /// <returns>对话事务列表</returns>
        public static List<CreatureChatTx> GetChatTxesForCreature(PhysicalObject chatter)
        {
            var result = new List<CreatureChatTx>();
            
            if (chatter == null) return result;
            
            // 遍历所有相机
            if (chatter.room?.game?.cameras == null) return result;
            
            foreach (var camera in chatter.room.game.cameras)
            {
                if (camera?.hud == null) continue;
                
                if (HUDModuleManager.HUDModules.TryGetValue(camera.hud, out var module))
                {
                    foreach (var chatTx in module.creatureChatTxes)
                    {
                        if (chatTx != null && chatTx.chatter == chatter)
                        {
                            result.Add(chatTx);
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 结束当前房间中所有生物的对话（清场）
        /// </summary>
        /// <param name="room">房间</param>
        public static void EndAllDialoguesInRoom(Room room)
        {
            if (room == null || room.game?.cameras == null) return;
            
            foreach (var camera in room.game.cameras)
            {
                if (camera?.hud == null) continue;
                
                if (HUDModuleManager.HUDModules.TryGetValue(camera.hud, out var module))
                {
                    // 结束所有对话气泡
                    foreach (var dialogBox in module.creatureDialogBoxes)
                    {
                        if (dialogBox != null)
                        {
                            dialogBox.EndCurrentMessageNow();
                        }
                    }
                    module.creatureDialogBoxes.Clear();
                    
                    // 结束所有对话事务
                    foreach (var chatTx in module.creatureChatTxes)
                    {
                        if (chatTx != null)
                        {
                            chatTx.Interrupted();
                        }
                    }
                    module.creatureChatTxes.Clear();
                }
            }
        }
    }
}