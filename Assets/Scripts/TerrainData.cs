using System;

[Serializable]
public class TerrainData
{
    public int TerrainOffsetX;
    public int TerrainOffsetY;

    public TerrainData()
    {

    }

    public TerrainData(int terrainOffsetX, int terrainOffsetY)
    {
        TerrainOffsetX = terrainOffsetX;
        TerrainOffsetY = terrainOffsetY;
    }
}
