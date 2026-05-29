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
    Furnace
}

public enum ResourceType
{
    Metal,
    Organic,
    Meat,
    Wood,
    Coal
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

    public Building(BuildingType type, Point position, int level = 1)
    {
        Type = type;
        Position = position;
        Level = level;
        ProductionProgress = 0;
        ProductionOutput = new Dictionary<ResourceType, int>();

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
        }

        return cost;
    }

    public bool CanUpgrade()
    {
        return Type != BuildingType.Base && Type != BuildingType.Bank && Type != BuildingType.Marketplace && (Type != BuildingType.Furnace || Level < 2) && Level < 5;
    }
}
