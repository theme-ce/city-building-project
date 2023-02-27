using System;

[Serializable]
public class Cell
{
    public CellType CellType;
}

public enum CellType : byte
{
    Water = 0,
    Corrupted = 1,
    Grass = 2,
}