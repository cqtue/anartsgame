using System.Windows;

namespace anartsgame.models;

public enum BuildingType
{
    Base,
    Factory,
    Mine
}

public class Building
{
    public BuildingType Type { get; set; }
    public Point Position { get; set; }
    public double BuildRadius { get; set; }
    public double Size { get; set; }

    public Building(BuildingType type, Point position)
    {
        Type = type;
        Position = position;

        switch (type)
        {
            case BuildingType.Base:
                BuildRadius = 200;
                Size = 60;
                break;
            case BuildingType.Factory:
                BuildRadius = 150;
                Size = 45;
                break;
            case BuildingType.Mine:
                BuildRadius = 150;
                Size = 35;
                break;
        }
    }
}
