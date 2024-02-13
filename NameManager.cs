
using HarmonyLib;

namespace AwesomePrinting
{

    public class NameManager
    {

        // constructor
        public NameManager()
        {
        }

    }

    [HarmonyPatch(typeof(InventoryItem), "getInvItemName")]
    public class GetInvItemNamePatch
    {
        private static void Postfix(InventoryItem __instance, ref string __result)
        {
            var heldItemID = __instance.getItemId();
            if (heldItemID == Plugin.worldEditTool)
            {

                string result = string.Empty;

                // Display brush sizes
                switch (Plugin.mode)
                {
                    case 0:
                        result += "SQ 1x1";
                        break;

                    case 1:
                        result += "SQ 3x3";
                        break;

                    case 2:
                        result += "SQ 5x5";
                        break;

                    case 3:
                        result += "SQ 7x7";
                        break;

                    case 4:
                        result += "R 3x3";
                        break;

                    case 5:
                        result += "R 5x5";
                        break;

                    case 6:
                        result += "R 7x7";
                        break;

                    case 7:
                        result += "R 9x9";
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

}
