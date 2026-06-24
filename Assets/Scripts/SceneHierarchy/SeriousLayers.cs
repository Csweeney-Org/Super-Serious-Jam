namespace Assets.Scripts.SceneHierarchy
{
    public static class SeriousLayers
    {
        public static int TerrainLayer => 20;
        public static int ImpassableWallLayer => 21;
        public static int GetLayerMaskFor(int[] selectedLayers)
        {
            int combinedMask = 0;
            for (int i = 0; i < selectedLayers.Length; i++)
            {
                combinedMask = combinedMask | GetLayerMaskFor(selectedLayers[i]);
            }
            return combinedMask;
        }
        public static int GetLayerMaskFor(int Layer)
        {
            return 1 << Layer;
        }
    }
}