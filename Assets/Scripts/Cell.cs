using UnityEngine;

public class Cell
{
    public CellType CellType;
    public Color Color;

    public Cell(TerrainInfo info)
    {
        CellType = info.CellType;
        Color = info.Color;
    }
}

public enum CellType : byte
{
    Water = 0,
    Sand = 1,
    Grass = 2,
    Grass2 = 3,
}