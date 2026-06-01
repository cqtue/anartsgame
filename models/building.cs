using System.Windows;
using System.Collections.Generic;

namespace anartsgame.models;

public enum BuildingType
{
    Base,
    Factory,
    Mine,
    MeatFactory,
    Sawmill,
    Bank,
    Marketplace,
    Furnace,
    Altar,
    Crystallizer
}

public enum ResourceType
{
    Metal,
    Organic,
    Meat,
    Wood,
    Coal,
    Bones,
    Diamonds
}

public class Building
{
    public BuildingType Type { get; set; }
    public Point Position { get; set; }
    public double BuildRadius { get; set; }
    public double Size { get; set; }
    public int Level { get; set; }
    public double ProductionTime { get; set; }
    public double ProductionProgress { get; set; }
    public Dictionary<ResourceType, int> ProductionOutput { get; set; }
    public bool CanProduce { get; set; }

    public ResourceType? InvestmentResource { get; set; }
    public int InvestmentAmount { get; set; }
    public double InvestmentProgress { get; set; }
    public double InvestmentCooldown { get; set; }
    public bool IsInvesting { get; set; }

    // Batch production fields (for Altar and Crystallizer)
    public bool IsBatchProducing { get; set; }
    public int BatchProductionTarget { get; set; }
    public int BatchProductionRemaining { get; set; }
    public ResourceType BatchProductionOutput { get; set; }
    public Dictionary<ResourceType, int> BatchProductionInput { get; set; }

    public Building(BuildingType type, Point position, int level = 1)
    {
        Type = type;
        Position = position;
        Level = level;
        ProductionProgress = 0;
        ProductionOutput = new Dictionary<ResourceType, int>();
        BatchProductionInput = new Dictionary<ResourceType, int>();
        IsBatchProducing = false;
        BatchProductionTarget = 0;
        BatchProductionRemaining = 0;

        switch (type)
        {
            case BuildingType.Base:
                BuildRadius = 200;
                Size = 60;
                CanProduce = false;
                ProductionTime = 0;
                break;
            case BuildingType.Factory:
                BuildRadius = 150;
                Size = 45;
                CanProduce = true;
                InitializeFactoryProduction(level);
                break;
            case BuildingType.Mine:
                BuildRadius = 150;
                Size = 35;
                CanProduce = true;
                InitializeMineProduction(level);
                break;
            case BuildingType.MeatFactory:
                BuildRadius = 150;
                Size = 45;
                CanProduce = true;
                InitializeMeatFactoryProduction(level);
                break;
            case BuildingType.Sawmill:
                BuildRadius = 150;
                Size = 40;
                CanProduce = true;
                InitializeSawmillProduction(level);
                break;
            case BuildingType.Bank:
                BuildRadius = 150;
                Size = 45;
                CanProduce = false;
                ProductionTime = 0;
                break;
            case BuildingType.Marketplace:
                BuildRadius = 150;
                Size = 50;
                CanProduce = false;
                ProductionTime = 0;
                break;
            case BuildingType.Furnace:
                BuildRadius = 150;
                Size = 40;
                CanProduce = true;
                InitializeFurnaceProduction(level);
                break;
            case BuildingType.Altar:
                BuildRadius = 150;
                Size = 35;
                CanProduce = false;
                ProductionTime = 0;
                InitializeAltarProduction(level);
                break;
            case BuildingType.Crystallizer:
                BuildRadius = 150;
                Size = 45;
                CanProduce = false;
                ProductionTime = 0;
                InitializeCrystallizerProduction(level);
                break;
        }
    }

    public void InitializeFactoryProduction(int level)
    {
        ProductionOutput.Clear();
        switch (level)
        {
            case 1:
                ProductionTime = 1.5;
                ProductionOutput[ResourceType.Organic] = 1;
                break;
            case 2:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Organic] = 1;
                break;
            case 3:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Organic] = 2;
                break;
            case 4:
                ProductionTime = 0.8;
                ProductionOutput[ResourceType.Organic] = 2;
                ProductionOutput[ResourceType.Meat] = 1;
                break;
            case 5:
                ProductionTime = 0.6;
                ProductionOutput[ResourceType.Organic] = 4;
                ProductionOutput[ResourceType.Meat] = 2;
                break;
        }
    }

    public void InitializeMineProduction(int level)
    {
        ProductionOutput.Clear();
        switch (level)
        {
            case 1:
                ProductionTime = 3.0;
                ProductionOutput[ResourceType.Metal] = 2;
                break;
            case 2:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Metal] = 2;
                break;
            case 3:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Metal] = 4;
                break;
            case 4:
                ProductionTime = 5.0;
                ProductionOutput[ResourceType.Metal] = 15;
                break;
            case 5:
                ProductionTime = 2.0;
                ProductionOutput[ResourceType.Metal] = 10;
                break;
        }
    }

    public void InitializeMeatFactoryProduction(int level)
    {
        ProductionOutput.Clear();
        switch (level)
        {
            case 1:
                ProductionTime = 3.0;
                ProductionOutput[ResourceType.Meat] = 2;
                break;
            case 2:
                ProductionTime = 3.0;
                ProductionOutput[ResourceType.Meat] = 4;
                break;
            case 3:
                ProductionTime = 3.0;
                ProductionOutput[ResourceType.Meat] = 8;
                break;
            case 4:
                ProductionTime = 2.0;
                ProductionOutput[ResourceType.Meat] = 10;
                break;
            case 5:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Meat] = 8;
                break;
        }
    }

    public void InitializeSawmillProduction(int level)
    {
        ProductionOutput.Clear();
        switch (level)
        {
            case 1:
                ProductionTime = 3.0;
                ProductionOutput[ResourceType.Wood] = 1;
                break;
            case 2:
                ProductionTime = 3.0;
                ProductionOutput[ResourceType.Wood] = 3;
                break;
            case 3:
                ProductionTime = 2.0;
                ProductionOutput[ResourceType.Wood] = 4;
                break;
            case 4:
                ProductionTime = 5.0;
                ProductionOutput[ResourceType.Wood] = 12;
                break;
            case 5:
                ProductionTime = 2.0;
                ProductionOutput[ResourceType.Wood] = 10;
                break;
        }
    }

    public void InitializeFurnaceProduction(int level)
    {
        ProductionOutput.Clear();
            switch (level)
        {
            case 1:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Coal] = 1;
                break;
            case 2:
                ProductionTime = 1.0;
                ProductionOutput[ResourceType.Coal] = 2;
                break;
        }
    }

    public void InitializeAltarProduction(int level)
    {
        BatchProductionInput.Clear();
        BatchProductionOutput = ResourceType.Bones;

        // Input: 10 coal + 5 organic + 1 meat → 1 bone
        BatchProductionInput[ResourceType.Coal] = 10;
        BatchProductionInput[ResourceType.Organic] = 5;
        BatchProductionInput[ResourceType.Meat] = 1;

        switch (level)
        {
            case 1:
                ProductionTime = 1.0; // 1 bone per 1 second
                break;
            case 2:
                ProductionTime = 0.5; // 1 bone per 0.5 seconds
                break;
            case 3:
                ProductionTime = 0.0; // Instant conversion
                break;
        }
    }

    public void InitializeCrystallizerProduction(int level)
    {
        BatchProductionInput.Clear();
        BatchProductionOutput = ResourceType.Diamonds;

        // Input: 400 coal + 400 organic + 600 metal → 1 diamond
        BatchProductionInput[ResourceType.Coal] = 400;
        BatchProductionInput[ResourceType.Organic] = 400;
        BatchProductionInput[ResourceType.Metal] = 600;

        switch (level)
        {
            case 1:
                ProductionTime = 2.0; // 1 diamond per 2 seconds
                break;
        }
    }

    public Dictionary<ResourceType, int> GetUpgradeCost()
    {
        var cost = new Dictionary<ResourceType, int>();

        switch (Type)
        {
            case BuildingType.Factory:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Metal] = 150;
                        cost[ResourceType.Organic] = 125;
                        break;
                    case 2:
                        cost[ResourceType.Metal] = 250;
                        cost[ResourceType.Organic] = 125;
                        break;
                    case 3:
                        cost[ResourceType.Metal] = 150;
                        cost[ResourceType.Organic] = 120;
                        cost[ResourceType.Meat] = 400;
                        break;
                    case 4:
                        cost[ResourceType.Metal] = 450;
                        cost[ResourceType.Organic] = 425;
                        break;
                }
                break;
            case BuildingType.Mine:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Metal] = 125;
                        cost[ResourceType.Meat] = 200;
                        break;
                    case 2:
                        cost[ResourceType.Metal] = 125;
                        cost[ResourceType.Meat] = 400;
                        break;
                    case 3:
                        cost[ResourceType.Metal] = 325;
                        cost[ResourceType.Meat] = 300;
                        cost[ResourceType.Wood] = 600;
                        break;
                    case 4:
                        cost[ResourceType.Metal] = 625;
                        cost[ResourceType.Meat] = 300;
                        break;
                }
                break;
            case BuildingType.MeatFactory:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Metal] = 135;
                        cost[ResourceType.Organic] = 200;
                        break;
                    case 2:
                        cost[ResourceType.Metal] = 135;
                        cost[ResourceType.Organic] = 170;
                        cost[ResourceType.Wood] = 150;
                        break;
                    case 3:
                        cost[ResourceType.Metal] = 350;
                        cost[ResourceType.Organic] = 400;
                        cost[ResourceType.Wood] = 700;
                        break;
                    case 4:
                        cost[ResourceType.Metal] = 635;
                        cost[ResourceType.Organic] = 200;
                        break;
                }
                break;
            case BuildingType.Sawmill:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Metal] = 155;
                        cost[ResourceType.Organic] = 120;
                        break;
                    case 2:
                        cost[ResourceType.Metal] = 95;
                        cost[ResourceType.Organic] = 300;
                        cost[ResourceType.Wood] = 200;
                        break;
                    case 3:
                        cost[ResourceType.Metal] = 205;
                        cost[ResourceType.Wood] = 400;
                        break;
                    case 4:
                        cost[ResourceType.Metal] = 135;
                        cost[ResourceType.Organic] = 130;
                        cost[ResourceType.Wood] = 125;
                        break;
                }
                break;
            case BuildingType.Bank:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Wood] = 1200;
                        break;
                }
                break;
            case BuildingType.Furnace:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Wood] = 600;
                        cost[ResourceType.Organic] = 300;
                        break;
                }
                break;
            case BuildingType.Altar:
                switch (Level)
                {
                    case 1:
                        cost[ResourceType.Organic] = 250;
                        cost[ResourceType.Bones] = 150;
                        break;
                    case 2:
                        cost[ResourceType.Bones] = 400;
                        cost[ResourceType.Diamonds] = 10;
                        break;
                }
                break;
        }

        return cost;
    }

    public static Dictionary<ResourceType, int> GetBuildCost(BuildingType type)
    {
        var cost = new Dictionary<ResourceType, int>();

        switch (type)
        {
            case BuildingType.Factory:
                cost[ResourceType.Metal] = 250;
                cost[ResourceType.Organic] = 125;
                break;
            case BuildingType.Mine:
                cost[ResourceType.Metal] = 225;
                cost[ResourceType.Meat] = 130;
                break;
            case BuildingType.MeatFactory:
                cost[ResourceType.Metal] = 135;
                cost[ResourceType.Organic] = 140;
                break;
            case BuildingType.Sawmill:
                cost[ResourceType.Metal] = 105;
                cost[ResourceType.Organic] = 165;
                break;
            case BuildingType.Bank:
                cost[ResourceType.Wood] = 600;
                cost[ResourceType.Organic] = 200;
                break;
            case BuildingType.Marketplace:
                cost[ResourceType.Wood] = 1200;
                cost[ResourceType.Metal] = 560;
                break;
            case BuildingType.Furnace:
                cost[ResourceType.Organic] = 500;
                cost[ResourceType.Meat] = 300;
                break;
            case BuildingType.Altar:
                cost[ResourceType.Organic] = 600;
                cost[ResourceType.Coal] = 200;
                break;
            case BuildingType.Crystallizer:
                cost[ResourceType.Metal] = 1000;
                break;
        }

        return cost;
    }

    public bool CanUpgrade()
    {
        // Buildings that cannot upgrade at all
        if (Type == BuildingType.Base || Type == BuildingType.Bank ||
            Type == BuildingType.Marketplace || Type == BuildingType.Crystallizer)
            return false;

        // Furnace can only upgrade to level 2
        if (Type == BuildingType.Furnace && Level >= 2)
            return false;

        // Altar can only upgrade to level 3
        if (Type == BuildingType.Altar && Level >= 3)
            return false;

        // All other buildings can upgrade up to level 5
        return Level < 5;
    }
}
