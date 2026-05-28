using anartsgame.models;
using System.Windows;
using System.Windows.Media;

namespace anartsgame.services;

public class MapGenerator
{
    private readonly Random _random = new();
    private const int TileSize = 64;

    public GameMap Generate(int widthInTiles, int heightInTiles)
    {
        var map = new GameMap(widthInTiles, heightInTiles);

        PlacePlayerBase(map);
        PlaceResourcePoints(map, 12);
        PlaceObstacles(map);
        AddRandomOffsets(map);

        return map;
    }

    private void AddRandomOffsets(GameMap map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(x, y);
                if (tile != null && tile.Type != TileType.Empty && tile.Type != TileType.Water)
                {
                    tile.OffsetX = (_random.NextDouble() - 0.5) * 12;
                    tile.OffsetY = (_random.NextDouble() - 0.5) * 12;
                }
            }
        }
    }

    private void PlacePlayerBase(GameMap map)
    {
        int centerX = map.Width / 2;
        int centerY = map.Height / 2;

        for (int x = centerX - 2; x <= centerX + 2; x++)
        {
            for (int y = centerY - 2; y <= centerY + 2; y++)
            {
                var tile = map.GetTile(x, y);
                if (tile != null)
                {
                    tile.Type = TileType.Empty;
                    tile.IsWalkable = true;
                }
            }
        }

        double pixelX = centerX * TileSize;
        double pixelY = centerY * TileSize;
        var playerBase = new Building(BuildingType.Base, new Point(pixelX, pixelY));
        map.Buildings.Add(playerBase);
    }

    private void PlaceResourcePoints(GameMap map, int count)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 10;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;
            int x = _random.Next(5, map.Width - 5);
            int y = _random.Next(5, map.Height - 5);

            var tile = map.GetTile(x, y);
            if (tile != null && tile.Type == TileType.Empty)
            {
                if (IsAreaClear(map, x, y, 4))
                {
                    tile.Type = TileType.ResourcePoint;
                    tile.IsWalkable = false;
                    placed++;
                }
            }
        }
    }

    private void PlaceObstacles(GameMap map)
    {
        PlaceObstacleType(map, TileType.Rock, 25);
        PlaceObstacleType(map, TileType.Tree, 35);
    }

    private void PlaceObstacleType(GameMap map, TileType type, int count)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 10;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;
            int x = _random.Next(0, map.Width);
            int y = _random.Next(0, map.Height);

            var tile = map.GetTile(x, y);
            if (tile != null && tile.Type == TileType.Empty)
            {
                if (IsAreaClear(map, x, y, 2))
                {
                    tile.Type = type;
                    tile.IsWalkable = false;
                    placed++;
                }
            }
        }
    }

    private bool IsAreaClear(GameMap map, int centerX, int centerY, int radius)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                var tile = map.GetTile(x, y);
                if (tile == null || tile.Type != TileType.Empty)
                    return false;
            }
        }
        return true;
    }
}
