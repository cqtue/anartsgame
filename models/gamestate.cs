using System.Collections.Generic;
using System.Windows;

namespace anartsgame.models;

public class GameState
{
    public Dictionary<ResourceType, int> Resources { get; set; } = new();
    public List<BuildingData> Buildings { get; set; } = new();
    public List<ResearchData> AvailableResearch { get; set; } = new();
    public List<ResearchData> CompletedResearch { get; set; } = new();
    public ResearchData? CurrentResearch { get; set; }
    public MapData Map { get; set; } = new();
    public CameraData Camera { get; set; } = new();
    public int GameSpeed { get; set; } = 1;
    public double ElapsedTime { get; set; } = 0;
}

public class BuildingData
{
    public BuildingType Type { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public int Level { get; set; }
    public double ProductionProgress { get; set; }
    public ResourceType? InvestmentResource { get; set; }
    public int InvestmentAmount { get; set; }
    public double InvestmentProgress { get; set; }
    public double InvestmentCooldown { get; set; }
    public bool IsInvesting { get; set; }

    // Batch production fields
    public bool IsBatchProducing { get; set; }
    public int BatchProductionTarget { get; set; }
    public int BatchProductionRemaining { get; set; }
    public ResourceType BatchProductionOutput { get; set; }
    public Dictionary<ResourceType, int> BatchProductionInput { get; set; } = new();

    public static BuildingData FromBuilding(Building building)
    {
        return new BuildingData
        {
            Type = building.Type,
            PositionX = building.Position.X,
            PositionY = building.Position.Y,
            Level = building.Level,
            ProductionProgress = building.ProductionProgress,
            InvestmentResource = building.InvestmentResource,
            InvestmentAmount = building.InvestmentAmount,
            InvestmentProgress = building.InvestmentProgress,
            InvestmentCooldown = building.InvestmentCooldown,
            IsInvesting = building.IsInvesting,
            IsBatchProducing = building.IsBatchProducing,
            BatchProductionTarget = building.BatchProductionTarget,
            BatchProductionRemaining = building.BatchProductionRemaining,
            BatchProductionOutput = building.BatchProductionOutput,
            BatchProductionInput = new Dictionary<ResourceType, int>(building.BatchProductionInput)
        };
    }

    public Building ToBuilding()
    {
        var building = new Building(Type, new Point(PositionX, PositionY), Level);
        building.ProductionProgress = ProductionProgress;
        building.InvestmentResource = InvestmentResource;
        building.InvestmentAmount = InvestmentAmount;
        building.InvestmentProgress = InvestmentProgress;
        building.InvestmentCooldown = InvestmentCooldown;
        building.IsInvesting = IsInvesting;
        building.IsBatchProducing = IsBatchProducing;
        building.BatchProductionTarget = BatchProductionTarget;
        building.BatchProductionRemaining = BatchProductionRemaining;
        building.BatchProductionOutput = BatchProductionOutput;
        building.BatchProductionInput = new Dictionary<ResourceType, int>(BatchProductionInput);
        return building;
    }
}

public class ResearchData
{
    public ResearchType Type { get; set; }
    public double Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsResearching { get; set; }

    public static ResearchData FromResearch(Research research)
    {
        return new ResearchData
        {
            Type = research.Type,
            Progress = research.Progress,
            IsCompleted = research.IsCompleted,
            IsResearching = research.IsResearching
        };
    }

    public Research ToResearch()
    {
        var research = new Research(Type);
        research.Progress = Progress;
        research.IsCompleted = IsCompleted;
        research.IsResearching = IsResearching;
        return research;
    }
}

public class MapData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<TileData> Tiles { get; set; } = new();

    public static MapData FromGameMap(GameMap map)
    {
        var mapData = new MapData
        {
            Width = map.Width,
            Height = map.Height
        };

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(x, y);
                if (tile != null)
                {
                    mapData.Tiles.Add(TileData.FromTile(tile));
                }
            }
        }

        return mapData;
    }

    public GameMap ToGameMap()
    {
        var map = new GameMap(Width, Height);

        foreach (var tileData in Tiles)
        {
            var tile = map.GetTile(tileData.X, tileData.Y);
            if (tile != null)
            {
                tile.Type = tileData.Type;
                tile.IsWalkable = tileData.IsWalkable;
                tile.OffsetX = tileData.OffsetX;
                tile.OffsetY = tileData.OffsetY;
            }
        }

        return map;
    }
}

public class TileData
{
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type { get; set; }
    public bool IsWalkable { get; set; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }

    public static TileData FromTile(Tile tile)
    {
        return new TileData
        {
            X = tile.X,
            Y = tile.Y,
            Type = tile.Type,
            IsWalkable = tile.IsWalkable,
            OffsetX = tile.OffsetX,
            OffsetY = tile.OffsetY
        };
    }
}

public class CameraData
{
    public double ScaleX { get; set; } = 1.0;
    public double ScaleY { get; set; } = 1.0;
    public double TranslateX { get; set; } = 0;
    public double TranslateY { get; set; } = 0;
}
