namespace anartsgame.models;

public enum TileType
{
    Empty, // костиль
    PlayerBase,
    ResourcePoint,
    Rock,
    Tree,
    Water
}

public class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type { get; set; }
    public bool IsWalkable { get; set; }
    public int WaterBodyId { get; set; } = -1; // тоже костиль
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
}
