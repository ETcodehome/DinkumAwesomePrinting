
namespace AwesomePrinting
{
    public class TempTileData
    {
        public int xCoord { get; set; }
        public int zCoord { get; set; }
        public int tileHeight { get; set; }
        public int tileType { get; set; }

        // constructor
        public TempTileData(int x, int z, int height, int ID)
        {
            xCoord = x;
            zCoord = z;
            tileHeight = height;
            tileType = ID;
        }

    }
}