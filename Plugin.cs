﻿using BepInEx;
using UnityEngine;
using HarmonyLib;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AwesomeReminders;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public const string CONFIG_GENERAL              = "General";
    public const int UNITY_LEFT_CLICK               = 0;
    public const int UNITY_RIGHT_CLICK               = 1;
    public const int UNITY_MIDDLE_CLICK               = 2;

    public static int sm_lastReminder = -1;

    /// <summary>
    /// Triggered when nextDay logic is run
    /// </summary>
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.loadDateFromSave))]
    class PatchNextDayLogic
    {
        private static void Postfix()
        {
            System.Console.WriteLine("test");
            sm_lastReminder = -1;
        }
    }

    /// <summary>
    /// Triggered when the plugin is activated (not constructed)
    /// </summary>
    private void Awake()
    {
        ApplyHarmonyPatches();
    }

    /// <summary>
    /// Patches native game routines with all harmony patches declared.
    /// </summary>
    private void ApplyHarmonyPatches()
    {
        _harmony.PatchAll();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} finished merging harmony patches!");
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

        if (Input.GetMouseButtonDown(UNITY_LEFT_CLICK) || Input.GetMouseButtonDown(UNITY_RIGHT_CLICK) || Input.GetMouseButtonDown(UNITY_MIDDLE_CLICK)){

            var todaysDate = WorldManager.Instance.day + (WorldManager.Instance.week - 1) * 7;
            
            // guard against spamming multiple times on the same day
            if (sm_lastReminder == todaysDate){
                return;
            }
            sm_lastReminder = todaysDate;

            // Birthday reminders
            for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
            {
                if (NPCManager.manage.npcStatus[i].hasMet && NPCManager.manage.NPCDetails[i].birthSeason == WorldManager.Instance.month && NPCManager.manage.NPCDetails[i].birthday == todaysDate)
                {
                    var reminder = "I've just remembered that it is " + NPCManager.manage.NPCDetails[i].NPCName + "'s birthday today!";
                    NotificationManager.manage.createChatNotification(reminder);
                    var advice = "I seem to recall that they like " + NPCManager.manage.NPCDetails[i].favouriteFood.itemName;
                    NotificationManager.manage.createChatNotification(advice);
                    break;
                }
            }
            
            // Event reminder - Generic
            TownEvent townEvent = TownEventManager.manage.checkEventForToday(todaysDate);
            if (townEvent != null)
            {
                var reminder = "I've jsut remembered that " + townEvent.getEventName() + " is on today!";
                NotificationManager.manage.createChatNotification(reminder);
            }

            // Event reminder - Bug Comp
            else if (CatchingCompetitionManager.manage.isBugCompDay(todaysDate))
            {
                var reminder = "I've just remembered that the Bug Catching Competition is on today!";
                NotificationManager.manage.createChatNotification(reminder);
                var advice = "I'll need to sign up with Julia, then see how many bugs I can catch starting from 9am.";
                NotificationManager.manage.createChatNotification(advice);
                advice = "A competition bug net would be handy, and all bugs caught must be logged in the book by 4pm.";
                NotificationManager.manage.createChatNotification(advice);
            }

            // Event reminder - Fishing Comp
            else if (CatchingCompetitionManager.manage.isFishCompDay(todaysDate))
            {
                var reminder = "I've just remembered that the Fishing Competition is on today!";
                NotificationManager.manage.createChatNotification(reminder);
                var advice = "I'll need to sign up with Max at the visitor's site, then see how many Barracuda I can catch starting from 9am.";
                NotificationManager.manage.createChatNotification(advice);
                advice = "A competition fishing rod would be handy, and my catch must be logged in the book by 4pm.";
                NotificationManager.manage.createChatNotification(advice);
            }

            // Event reminder - John's Goods Anniversary
            else if (TownEventManager.manage.IsJohnsAnniversary(todaysDate))
            {
                var reminder = "I've just remembered that its John's Goods Anniversary week!";
                NotificationManager.manage.createChatNotification(reminder);
                var advice = "I can earn some prize tokens by selling fish to John.";
                NotificationManager.manage.createChatNotification(advice);
            }
        }
    }
}
