using System.Collections.Generic;

namespace anartsgame.models;

public class GameMap
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,] Tiles { get; set; }
    public List<Building> Buildings { get; set; } = new();

    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tiles[x, y] = new Tile
                {
                    X = x,
                    Y = y,
                    Type = TileType.Empty,
                    IsWalkable = true
                };
            }
        }
    }

    public Tile? GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        return Tiles[x, y];
    }
}
