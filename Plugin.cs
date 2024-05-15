﻿
using System;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;

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
        public static int worldEditTool = 925;

        private NameManager nameManager = new NameManager();
        private BrushManager brushManager = new BrushManager();
        private PreviewManager previewManager = new PreviewManager();
        private SelectionManager selectionManager = new SelectionManager();

        public Plugin()
        {
        }

        private void Awake()
        {
            this.configKey = base.Config.Bind<KeyCode>("Options", "Combo Key", KeyCode.LeftShift, "Combo Key for use when doing advanced selections.");
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
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

            previewManager.Tick();

            int invIndex = Inventory.Instance.selectedSlot;
            int heldItemID = Inventory.Instance.invSlots[invIndex].itemNo;

            if (heldItemID == worldEditTool){    

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
                    
                    // if there is an existing preview get rid of that instead of doing any further action
                    if (previewManager.TimeSinceLastPreview() < previewManager.previewTime)
                    {
                        previewManager.RevertPreviewChanges();
                        return;
                    }

                    // change tile area size
                    if (Input.GetKey(this.configKey.Value))
                    {
                        brushManager.ChangeBrushShape();

                        // Get the tiles to operate on based on the new brush setting
                        List<TempTileData> previewTiles = selectionManager.GetSelectedTiles();

                        // Store the current tile information
                        previewManager.StorePreviewData(previewTiles);

                        // Update the selection with a tile preview
                        foreach (TempTileData tile in previewTiles)
                        {
                            int x = tile.xCoord;
                            int z = tile.zCoord;
                            if (sampledTileType != -1)
                            {
                                NetworkMapSharer.Instance.RpcUpdateTileType(sampledTileType, x, z);
                            }
                            else
                            {
                                NetworkMapSharer.Instance.RpcUpdateTileType(32, x, z);
                            }

                            NetworkMapSharer.Instance.RpcUpdateTileHeight(0, x, z);
                        
                            // Flag the chunk as needing an update
                            WorldManager.Instance.heightChunkHasChanged(x, z);
                            WorldManager.Instance.addToChunksToRefreshList(x, z);
                            
                        }

                        return;
                    }

                    // Get the tiles to operate on based on current brush setting
                    List<TempTileData> targetTiles = selectionManager.GetSelectedTiles();

                    // Actually apply the tile changes
                    foreach (TempTileData tile in targetTiles)
                    {
                        int x = tile.xCoord;
                        int z = tile.zCoord;
                        int currentTileType = tile.tileType;

                        // Update the tile visuals
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
