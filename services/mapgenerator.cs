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
        PlaceStartingBuildings(map);

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

    private void PlaceStartingBuildings(GameMap map)
    {
        if (map.Buildings.Count == 0) return;

        var playerBase = map.Buildings[0];
        int centerX = map.Width / 2;
        int centerY = map.Height / 2;

        Tile? nearestRock = FindNearestTileOfType(map, centerX, centerY, TileType.Rock);
        if (nearestRock != null)
        {
            double rockPixelX = nearestRock.X * TileSize + nearestRock.OffsetX;
            double rockPixelY = nearestRock.Y * TileSize + nearestRock.OffsetY;

            double directionX = rockPixelX - playerBase.Position.X;
            double directionY = rockPixelY - playerBase.Position.Y;
            double distanceToRock = Math.Sqrt(directionX * directionX + directionY * directionY);

            if (distanceToRock > 0)
            {
                directionX /= distanceToRock;
                directionY /= distanceToRock;

                double placementDistance = Math.Min(distanceToRock - 90, playerBase.BuildRadius - 50);
                placementDistance = Math.Max(placementDistance, 80);

                double mineX = playerBase.Position.X + directionX * placementDistance;
                double mineY = playerBase.Position.Y + directionY * placementDistance;

                if (IsPositionValidForBuilding(map, mineX, mineY))
                {
                    var mine = new Building(BuildingType.Mine, new Point(mineX, mineY));
                    map.Buildings.Add(mine);
                    PlaceRocksNearBuilding(map, mineX, mineY, 3);
                }
            }
        }

        Tile? nearestTree = FindNearestTileOfType(map, centerX, centerY, TileType.Tree);
        if (nearestTree != null)
        {
            double treePixelX = nearestTree.X * TileSize + nearestTree.OffsetX;
            double treePixelY = nearestTree.Y * TileSize + nearestTree.OffsetY;

            double directionX = treePixelX - playerBase.Position.X;
            double directionY = treePixelY - playerBase.Position.Y;
            double distanceToTree = Math.Sqrt(directionX * directionX + directionY * directionY);

            if (distanceToTree > 0)
            {
                directionX /= distanceToTree;
                directionY /= distanceToTree;

                double placementDistance = Math.Min(distanceToTree - 90, playerBase.BuildRadius - 50);
                placementDistance = Math.Max(placementDistance, 80);

                double sawmillX = playerBase.Position.X + directionX * placementDistance;
                double sawmillY = playerBase.Position.Y + directionY * placementDistance;

                if (IsPositionValidForBuilding(map, sawmillX, sawmillY))
                {
                    var sawmill = new Building(BuildingType.Sawmill, new Point(sawmillX, sawmillY));
                    map.Buildings.Add(sawmill);
                }
            }
        }

        int factoryAttempts = 0;
        int maxFactoryAttempts = 20;
        bool factoryPlaced = false;

        while (!factoryPlaced && factoryAttempts < maxFactoryAttempts)
        {
            factoryAttempts++;
            double factoryAngle = _random.NextDouble() * Math.PI * 2;
            double factoryDistance = 100 + _random.NextDouble() * 50;
            double factoryX = playerBase.Position.X + Math.Cos(factoryAngle) * factoryDistance;
            double factoryY = playerBase.Position.Y + Math.Sin(factoryAngle) * factoryDistance;

            if (IsPositionValidForBuilding(map, factoryX, factoryY))
            {
                var factory = new Building(BuildingType.Factory, new Point(factoryX, factoryY));
                map.Buildings.Add(factory);
                factoryPlaced = true;
            }
        }
    }

    private bool IsPositionValidForBuilding(GameMap map, double x, double y)
    {
        const double minDistance = 80;

        foreach (var building in map.Buildings)
        {
            double dx = x - building.Position.X;
            double dy = y - building.Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    private Tile? FindNearestTileOfType(GameMap map, int centerX, int centerY, TileType type)
    {
        Tile? nearest = null;
        double minDistance = double.MaxValue;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(x, y);
                if (tile != null && tile.Type == type)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(x - centerX, 2) +
                        Math.Pow(y - centerY, 2)
                    );

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = tile;
                    }
                }
            }
        }

        return nearest;
    }

    private void PlaceRocksNearBuilding(GameMap map, double buildingX, double buildingY, int count)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 10;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;

            // Generate random position around the building (within 3-5 tiles)
            double angle = _random.NextDouble() * Math.PI * 2;
            double distance = (3 + _random.NextDouble() * 2) * TileSize;

            double rockPixelX = buildingX + Math.Cos(angle) * distance;
            double rockPixelY = buildingY + Math.Sin(angle) * distance;

            // Convert to tile coordinates
            int tileX = (int)(rockPixelX / TileSize);
            int tileY = (int)(rockPixelY / TileSize);

            var tile = map.GetTile(tileX, tileY);
            if (tile != null && tile.Type == TileType.Empty)
            {
                tile.Type = TileType.Rock;
                tile.IsWalkable = false;
                placed++;
            }
        }
    }
}
