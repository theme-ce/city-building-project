using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int BelongToChunk;
    public CellType CellType;
    public DualGridCell AOfDualGrids = null;
    public DualGridCell BOfDualGrids = null;
    public DualGridCell COfDualGrids = null;
    public DualGridCell DOfDualGrids = null;

    public Resource ResourceData;
    public List<Cell> ResourceChildCell = new List<Cell>();

    public Cell(Vector2Int chunk, CellType cellType)
    {
        BelongToChunk = chunk;
        CellType = cellType;
    }

    public void UpdateType(CellType cellType)
    {
        CellType = cellType;
        if (AOfDualGrids != null) AOfDualGrids.A = cellType;
        if (BOfDualGrids != null) BOfDualGrids.B = cellType;
        if (COfDualGrids != null) COfDualGrids.C = cellType;
        if (DOfDualGrids != null) DOfDualGrids.D = cellType;
    }
}

public enum CellType : byte
{
    Soil = 0,
    Grass = 1,
    Structure = 2,
    Resource = 3,
}