
using HarmonyLib;

namespace AwesomePrinting
{

    public class BrushManager
    {

        // constructor
        public BrushManager()
        {
        }

        public void ChangeBrushShape()
        {
            
            switch (Plugin.mode)
            {
                case 0:
                    Plugin.mode = 1;
                    break;

                case 1:
                    Plugin.mode = 2;
                    break;

                case 2:
                    Plugin.mode = 3;
                    break;
                
                case 3:
                    Plugin.mode = 4;
                    break;

                case 4:
                    Plugin.mode = 5;
                    break;

                case 5:
                    Plugin.mode = 6;
                    break;

                case 6:
                    Plugin.mode = 7;
                    break;

                case 7:
                    Plugin.mode = 0;
                    break;
            }
            Inventory.Instance.equipNewSelectedSlot();

        }

    }

}
