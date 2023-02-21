public class Cell
{
    public CellType CellType;
    public DualGridCell AOfDualGrids = null;
    public DualGridCell BOfDualGrids = null;
    public DualGridCell COfDualGrids = null;
    public DualGridCell DOfDualGrids = null;

    public Cell(CellType cellType)
    {
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
}