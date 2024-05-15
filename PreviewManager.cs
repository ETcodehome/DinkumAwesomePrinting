using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AwesomePrinting
{
    public class PreviewManager
    {
        public List<TempTileData> currentPreview = new List<TempTileData>();
        public DateTime lastPreview = DateTime.Now;
        public double previewTime = 1500;


        public double TimeSinceLastPreview()
        {
            return (DateTime.Now - lastPreview).TotalMilliseconds;
        }

        public bool ShouldPreviewBeDisplayed()
        {
            // guard against an empty list (do nothing)
            if (currentPreview.Count < 1){
                return false;
            }

            // guard against insufficient elapsed time (do nothing)
            if (TimeSinceLastPreview() > previewTime){
                return false;
            }

            return true;
        }

        public void Tick()
        {
            if (!ShouldPreviewBeDisplayed()){
                RevertPreviewChanges();
            }
        }


        public void RevertPreviewChanges(){

            foreach (TempTileData tile in currentPreview)
            {
                int x = tile.xCoord;
                int z = tile.zCoord;
                int ID = tile.tileType;
                int oldHeight = tile.tileHeight;
                int currentHeight = WorldManager.Instance.heightMap[x, z];
                int heightDiff = oldHeight - currentHeight;
                NetworkMapSharer.Instance.RpcUpdateTileType(ID, x, z);
                NetworkMapSharer.Instance.RpcUpdateTileHeight(heightDiff, x, z);
                        
                // Flag the chunk as needing an update
                WorldManager.Instance.heightChunkHasChanged(x, z);
                WorldManager.Instance.addToChunksToRefreshList(x, z);
            }

            currentPreview.Clear();
            lastPreview = new DateTime();
        }

        
        public void StorePreviewData(List<TempTileData> newPreview)
        {

            // clear any existing preview    
            RevertPreviewChanges();

            // assign the passed in temp tile data to the temp data array
            currentPreview = newPreview;

            // update the timestamp for the last preview
            lastPreview = DateTime.Now;
        }

    }
}