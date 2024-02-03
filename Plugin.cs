using System;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;

namespace AwesomePrinting
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const int UNITY_LEFT_CLICK               = 0;
        public const int UNITY_RIGHT_CLICK               = 1;
        public const int UNITY_MIDDLE_CLICK               = 2;
        public static int printHeight = 0;
        public static int sampledTileType = -1;
        private ConfigEntry<KeyCode> configKey;
        public static int mode = 0;
        public static int dirtPrinterItemID = 925;

        public Plugin()
        {
        }

        private void Awake()
        {
            this.configKey = base.Config.Bind<KeyCode>("Options", "Combo Key", KeyCode.LeftShift, "Combo Key for use when doing advanced selections.");
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
        }

        [HarmonyPatch(typeof(InventoryItem), "getInvItemName")]
        public class GetInvItemNamePatch
        {
            private static void Postfix(InventoryItem __instance, ref string __result)
            {
                var heldItemID = __instance.getItemId();
                if (heldItemID == Plugin.dirtPrinterItemID)
                {

                    string result = string.Empty;

                    // Display brush sizes
                    switch (Plugin.mode)
                    {
                        case 0:
                            result += "1x1";
                            break;

                        case 1:
                            result += "3x3";
                            break;

                        case 2:
                            result += "5x5";
                            break;

                        case 3:
                            result += "7x7";
                            break;
                    }
                    
                    // Display editing mode
                    result += ", Height=" + Plugin.printHeight;

                    // Display editing mode
                    switch (Plugin.sampledTileType)
                    {
                        case -1:
                            result += ", Tile=Existing";
                            break;

                        default:
                            result += ", Tile=" + WorldManager.Instance.tileTypes[Plugin.sampledTileType].name;
                            break;
                    }

                    __result = result;

                }
            }
        }

        /// <summary>
        /// Update method called regularly during normal gameplay
        /// </summary>
        private void Update()
        {

            // Guard clause - Ensure player isn't in a menu
            if (Inventory.Instance.isMenuOpen()){
                return;
            }

            int invIndex = Inventory.Instance.selectedSlot;
            int heldItemID = Inventory.Instance.invSlots[invIndex].itemNo;
            int dirtPrinterItemID = 925;

            if (heldItemID == dirtPrinterItemID){    

                int xHighLight = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x / 2f);
                int zHighLight = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z / 2f);

                // provide feedback
                if (Input.GetKeyDown(this.configKey.Value)){
                    string comboKey = this.configKey.BoxedValue.ToString();
                    NotificationManager.manage.createChatNotification("[RightClick] = Apply at tile highlighter");
                    NotificationManager.manage.createChatNotification("[MiddleClick] = Set height to level at your feet");
                    NotificationManager.manage.createChatNotification($"[{comboKey}] + [MiddleClick] = Sample selected tile");
                    NotificationManager.manage.createChatNotification($"[{comboKey}] + [RightClick] = Change brush size");
                }
                
                if (Input.GetMouseButtonDown(UNITY_MIDDLE_CLICK)){
                    
                    var sampling = false;
                    if (Input.GetKey(this.configKey.Value)){
                        sampling = true;
                    }

                    if (sampling) 
                    {
                        var newType = WorldManager.Instance.tileTypeMap[xHighLight, zHighLight];
                        if (newType == sampledTileType){
                            sampledTileType = -1;
                        }
                        else {
                            sampledTileType = newType;
                        }
                        Inventory.Instance.equipNewSelectedSlot();
                        return;
                    }

                    // no modifiers
                    printHeight = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.transform.position.y);
                    Inventory.Instance.equipNewSelectedSlot();

                }

                if (Input.GetMouseButtonDown(UNITY_RIGHT_CLICK))
                {

                    // change tile area size
                    if (Input.GetKey(this.configKey.Value))
                    {
                    
                        switch (mode)
                        {
                            case 0:
                                mode = 1;
                                break;

                            case 1:
                                mode = 2;
                                break;

                            case 2:
                                mode = 3;
                                break;

                            case 3:
                                mode = 0;
                                break;
                        }
                        Inventory.Instance.equipNewSelectedSlot();
                        return;
                    }

                    // update area based on mode
                    int size = 1;
                    switch (mode)
                    {
                        case 0:
                            size = 1;
                            break;

                        case 1:
                            size = 3;
                            break;

                        case 2:
                            size = 5;
                            break;

                        case 3:
                            size = 7;
                            break;
                    }

                    // calculate actual target positions

                    var forwardTransform = NetworkMapSharer.Instance.localChar.transform.forward;
                    var sizeOffset = (size - 1) / 2;
                    var zOffset = sizeOffset;
                    var xOffset = sizeOffset;
                    var xCentre = xHighLight;
                    var zCentre = zHighLight;

                    if (forwardTransform.z < 0) { zOffset *= -1; }
                    if (forwardTransform.x < 0) { xOffset *= -1; }
                    if (Mathf.Abs(forwardTransform.x) > 0.5) { xCentre += xOffset; }
                    if (Mathf.Abs(forwardTransform.z) > 0.5) { zCentre += zOffset; }

                    var xMin = xCentre - sizeOffset;
                    var xMax = xCentre + sizeOffset;
                    var zMin = zCentre - sizeOffset;
                    var zMax = zCentre + sizeOffset;

                    // actually update the tiles
                    for (int x = xMin; x <= xMax; x++) 
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            // Update the tile visuals
                            int currentTileType = WorldManager.Instance.tileTypeMap[x, z];
                            if (sampledTileType != -1 && sampledTileType != currentTileType)
                            {
                                NetworkMapSharer.Instance.RpcUpdateTileType(sampledTileType, x, z);
                            }

                            // Update the tile height
                            int currentHeight = WorldManager.Instance.heightMap[x, z];
                            int heightDifference = printHeight - currentHeight;
                            NetworkMapSharer.Instance.RpcUpdateTileHeight(heightDifference, x, z);
                            
                            // Flag the chunk as needing an update
                            WorldManager.Instance.heightChunkHasChanged(x, z);
                            WorldManager.Instance.addToChunksToRefreshList(x, z);
                        }
                    }
                }
            } 
        }
    }
}
