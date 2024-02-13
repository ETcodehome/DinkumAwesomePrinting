
using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AwesomePrinting
{
    public class SelectionManager
    {

        // constructor
        public SelectionManager()
        {
        }

        public List<TempTileData> GetSelectedTiles()
        {
            var selectedTiles = new List<TempTileData>();

            // decide area based on mode
            int size = 1;
            float radius = -1;
            switch (Plugin.mode)
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

                case 4:
                    size = 3;
                    radius = 1.0f;
                    break;

                case 5:
                    size = 5;
                    radius = 2.25f;
                    break;

                case 6:
                    size = 7;
                    radius = 3.25f;
                    break;

                case 7:
                    size = 9;
                    radius = 4.25f;
                    break;
            }

            var forwardTransform = NetworkMapSharer.Instance.localChar.transform.forward;
            var sizeOffset = (size - 1) / 2;
            var zOffset = sizeOffset;
            var xOffset = sizeOffset;

            int xHighLight = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x / 2f);
            int zHighLight = Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z / 2f);
            
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

            // actually accumulate the tile data
            for (int x = xMin; x <= xMax; x++) 
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    // guard against points outside the radius
                    if (radius != -1){
                        double xDiff = Math.Abs(x - xCentre);
                        double zDiff = Math.Abs(z - zCentre);
                        double distance = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(zDiff, 2));
                        double tolerance = 0.01d;
                        double maxDistance = radius + tolerance;
                        if (distance > maxDistance){
                            continue;
                        }
                    }
                    
                    // current target tile details
                    int targetType = WorldManager.Instance.tileTypeMap[x, z];
                    int targetHeight = WorldManager.Instance.heightMap[x, z];

                    // add the resulting tile to the selection state
                    selectedTiles.Add(new TempTileData(x, z, targetHeight, targetType));
                }
            }

            return selectedTiles;
        }
    }
}
