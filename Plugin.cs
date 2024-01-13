﻿using BepInEx;
using UnityEngine;

namespace AwesomePrinting;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{

    public const string CONFIG_GENERAL              = "General";
    public const int UNITY_LEFT_CLICK               = 0;
    public const int UNITY_RIGHT_CLICK               = 1;
    public const int UNITY_MIDDLE_CLICK               = 2;

    public int printHeight = 0;
    public Vector2Int lastPos = new Vector2Int();


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

        // NotificationManager.manage.createChatNotification("holding " + heldItemID.ToString())

        // Get the target tile location above the highlighter tile



        int x = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x / 2f);
        int z = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z / 2f);

        int dirtPrinterItemID = 925;
        if (heldItemID == dirtPrinterItemID){
            
            if (Input.GetMouseButtonDown(UNITY_MIDDLE_CLICK)){
                
                //NetworkMapSharer.share.RpcUpdateTileHeight(1, x, z);
                printHeight = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.transform.position.y);
                NotificationManager.manage.createChatNotification("Locked print height to " + printHeight);
            }

            if (Input.GetMouseButtonDown(UNITY_RIGHT_CLICK)){

                Vector3 highlighterPosRaw = NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position;
                string highlighterRaw = highlighterPosRaw.ToString();
                
                Vector3 playerPosRaw = NetworkMapSharer.Instance.localChar.transform.position;
                string playerRaw = playerPosRaw.ToString();

                Vector2Int currentTarget = new Vector2Int(x,z);
                if (currentTarget == lastPos){
                    NetworkMapSharer.Instance.localChar.CmdSendEmote(1);// tileHighlighter.position
                    return;
                }

                // Good
                // int tileChange = Mathf.RoundToInt(playerPosRaw.y-(highlighterPosRaw.y + 0.5f));
                int tileChange = Mathf.RoundToInt(printHeight-(highlighterPosRaw.y + 0.5f));

                NotificationManager.manage.createChatNotification("highlighterRaw: " + highlighterRaw);
                NotificationManager.manage.createChatNotification("playerRaw: " + playerRaw);
                NotificationManager.manage.createChatNotification("tile Y Delta: " + tileChange.ToString());

                NetworkMapSharer.Instance.RpcUpdateTileHeight(tileChange, x, z);
                lastPos = currentTarget;
                // NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position = new Vector3(highlighterPosRaw.x, printHeight, highlighterPosRaw.z);//.changeTileHeight(1);
                // NetworkMapSharer.share.localChar.CmdSendEmote(5);// tileHighlighter.position

            }

        }
        

    }

}
