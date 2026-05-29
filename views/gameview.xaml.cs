using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using anartsgame.models;

namespace anartsgame.views;

public partial class GameView : UserControl
{
    private const int TileSize = 64;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 2.0;
    private const double ZoomSpeed = 0.1;

    private GameMap? _map;
    private Random _random = new();
    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
    private TransformGroup _transformGroup;

    private Point _lastMousePosition;
    private bool _isPanning;

    private int _gameSpeed = 1;
    private double _elapsedTime = 0;
    private System.Windows.Threading.DispatcherTimer _gameTimer;

    private string _currentOpenPanel = "";

    private bool _isBuildMode = false;
    private BuildingType? _selectedBuildingType = null;
    private Canvas? _buildModeCanvas;
    private Ellipse? _buildPrototype;
    private Line? _buildRoadLine;
    private List<Line> _buildRoadLines = new();
    private List<Ellipse> _buildRadiusCircles = new();
    private TextBlock? _buildErrorTooltip;
    private List<Line> _permanentRoads = new();

    private Building? _selectedBuilding = null;
    private bool _deleteConfirmationState = false;
    private List<System.Windows.Controls.ProgressBar> _currentProgressBars = new();

    private Dictionary<ResourceType, int> _resources = new();
    private List<Research> _availableResearch = new();
    private List<Research> _completedResearch = new();

    // Trading system state
    private int _tradeStage = 1;
    private ResourceType? _tradeFromResource = null;
    private int _tradeFromAmount = 0;
    private ResourceType? _tradeToResource = null;
    private Research? _currentResearch = null;
    private bool _isPaused = false;
    private bool _isLoadingFromSave = false;

    public GameView(bool loadFromSave = false)
    {
        InitializeComponent();

        _isLoadingFromSave = loadFromSave;

        _scaleTransform = new ScaleTransform(1.0, 1.0);
        _translateTransform = new TranslateTransform(0, 0);
        _transformGroup = new TransformGroup();
        _transformGroup.Children.Add(_scaleTransform);
        _transformGroup.Children.Add(_translateTransform);

        MapCanvas.RenderTransform = _transformGroup;

        _gameTimer = new System.Windows.Threading.DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(100);
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();

        if (!_isLoadingFromSave)
        {
            _resources[ResourceType.Metal] = 200;
            _resources[ResourceType.Organic] = 200;
            _resources[ResourceType.Meat] = 200;
            _resources[ResourceType.Wood] = 100;
            _resources[ResourceType.Coal] = 0;

            _availableResearch.Add(new Research(ResearchType.ImprovedProduction));
            _availableResearch.Add(new Research(ResearchType.EfficientConstruction));
            _availableResearch.Add(new Research(ResearchType.FastLearning));
            _availableResearch.Add(new Research(ResearchType.ExtendedRadius));
            _availableResearch.Add(new Research(ResearchType.AdvancedMining));
            _availableResearch.Add(new Research(ResearchType.OrganicBoost));
        }

        Loaded += OnLoaded;
        MouseWheel += OnMouseWheel;
        MouseDown += OnMouseDown;
        MouseUp += OnMouseUp;
        MouseMove += OnMouseMove;
        KeyDown += OnKeyDown;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_isLoadingFromSave)
        {
            GenerateMap();
            RenderMap();
            CenterCamera();
        }
    }

    private void GenerateMap()
    {
        var generator = new services.MapGenerator();
        _map = generator.Generate(80, 60);
    }

    private void RenderMap()
    {
        if (_map == null) return;

        MapCanvas.Children.Clear();

        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                var tile = _map.Tiles[x, y];
                if (tile.Type != TileType.Empty && tile.Type != TileType.Water)
                {
                    var visual = CreateTileVisual(tile);
                    Canvas.SetLeft(visual, x * TileSize + tile.OffsetX);
                    Canvas.SetTop(visual, y * TileSize + tile.OffsetY);
                    MapCanvas.Children.Add(visual);
                }
            }
        }

        MapCanvas.Width = _map.Width * TileSize;
        MapCanvas.Height = _map.Height * TileSize;

        foreach (var building in _map.Buildings)
        {
            RenderBuilding(building);
        }

        CreateRoadsBetweenBuildings();
    }

    private void CreateRoadsBetweenBuildings()
    {
        if (_map == null) return;

        for (int i = 0; i < _map.Buildings.Count; i++)
        {
            var building = _map.Buildings[i];
            var buildingsInRange = FindBuildingsInRange(building.Position, 250);

            foreach (var nearbyBuilding in buildingsInRange)
            {
                if (nearbyBuilding != building)
                {
                    int buildingIndex = _map.Buildings.IndexOf(nearbyBuilding);
                    if (buildingIndex > i)
                    {
                        var road = new Line
                        {
                            X1 = building.Position.X,
                            Y1 = building.Position.Y,
                            X2 = nearbyBuilding.Position.X,
                            Y2 = nearbyBuilding.Position.Y,
                            Stroke = new SolidColorBrush(Color.FromArgb(150, 100, 100, 100)),
                            StrokeThickness = 2
                        };
                        MapCanvas.Children.Add(road);
                    }
                }
            }
        }
    }

    private UIElement CreateTileVisual(Tile tile)
    {
        return tile.Type switch
        {
            TileType.PlayerBase => CreatePlayerBaseTile(),
            TileType.ResourcePoint => CreateResourcePointTile(),
            TileType.Rock => CreateRockTile(),
            TileType.Tree => CreateTreeTile(),
            _ => new Canvas { Width = TileSize, Height = TileSize }
        };
    }

    private UIElement CreatePlayerBaseTile()
    {
        var canvas = new Canvas { Width = TileSize, Height = TileSize };

        var outerGlow = new Ellipse
        {
            Width = TileSize - 4,
            Height = TileSize - 4,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.45, 0.45),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, 40, 80, 40), 0.0),
                    new GradientStop(Color.FromArgb(30, 30, 60, 30), 0.6),
                    new GradientStop(Color.FromArgb(0, 20, 40, 20), 1.0)
                }
            }
        };
        Canvas.SetLeft(outerGlow, 2);
        Canvas.SetTop(outerGlow, 2);

        var mainBody = new Ellipse
        {
            Width = TileSize - 12,
            Height = TileSize - 12,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.4, 0.4),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(42, 78, 42), 0.0),
                    new GradientStop(Color.FromRgb(28, 58, 28), 0.5),
                    new GradientStop(Color.FromRgb(18, 42, 18), 0.8),
                    new GradientStop(Color.FromRgb(12, 30, 12), 1.0)
                }
            }
        };
        Canvas.SetLeft(mainBody, 6);
        Canvas.SetTop(mainBody, 6);

        var innerHighlight = new Ellipse
        {
            Width = TileSize - 28,
            Height = TileSize - 28,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.35, 0.35),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(80, 60, 100, 60), 0.0),
                    new GradientStop(Color.FromArgb(0, 40, 80, 40), 1.0)
                }
            }
        };
        Canvas.SetLeft(innerHighlight, 14);
        Canvas.SetTop(innerHighlight, 14);

        canvas.Children.Add(outerGlow);
        canvas.Children.Add(mainBody);
        canvas.Children.Add(innerHighlight);
        return canvas;
    }

    private UIElement CreateResourcePointTile()
    {
        var canvas = new Canvas { Width = TileSize, Height = TileSize };

        var outerRing = new Ellipse
        {
            Width = TileSize - 8,
            Height = TileSize - 8,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.4, 0.4),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(95, 82, 38), 0.0),
                    new GradientStop(Color.FromRgb(72, 62, 28), 0.5),
                    new GradientStop(Color.FromRgb(52, 45, 20), 0.8),
                    new GradientStop(Color.FromRgb(38, 32, 15), 1.0)
                }
            }
        };
        Canvas.SetLeft(outerRing, 4);
        Canvas.SetTop(outerRing, 4);

        var middleRing = new Ellipse
        {
            Width = TileSize - 20,
            Height = TileSize - 20,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.35, 0.35),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(108, 95, 45), 0.0),
                    new GradientStop(Color.FromRgb(85, 72, 32), 0.6),
                    new GradientStop(Color.FromRgb(62, 52, 22), 1.0)
                }
            }
        };
        Canvas.SetLeft(middleRing, 10);
        Canvas.SetTop(middleRing, 10);

        var innerCore = new Ellipse
        {
            Width = TileSize - 36,
            Height = TileSize - 36,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.3, 0.3),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(125, 110, 52), 0.0),
                    new GradientStop(Color.FromRgb(95, 82, 38), 0.5),
                    new GradientStop(Color.FromRgb(72, 62, 28), 1.0)
                }
            }
        };
        Canvas.SetLeft(innerCore, 18);
        Canvas.SetTop(innerCore, 18);

        var highlight = new Ellipse
        {
            Width = TileSize - 48,
            Height = TileSize - 48,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.25, 0.25),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, 140, 125, 60), 0.0),
                    new GradientStop(Color.FromArgb(0, 100, 85, 40), 1.0)
                }
            }
        };
        Canvas.SetLeft(highlight, 24);
        Canvas.SetTop(highlight, 24);

        canvas.Children.Add(outerRing);
        canvas.Children.Add(middleRing);
        canvas.Children.Add(innerCore);
        canvas.Children.Add(highlight);
        return canvas;
    }

    private UIElement CreateRockTile()
    {
        var canvas = new Canvas { Width = TileSize, Height = TileSize };
        var centerX = TileSize / 2.0;
        var centerY = TileSize / 2.0;

        int mainPoints = _random.Next(7, 11);
        var rockPoints = new List<Point>();

        for (int i = 0; i < mainPoints; i++)
        {
            double angle = (i / (double)mainPoints) * Math.PI * 2;
            double radiusVar = _random.NextDouble() * 0.6 + 0.4;
            double radius = (TileSize / 2.0 - 8) * radiusVar;

            angle += (_random.NextDouble() - 0.5) * 0.6;

            double x = centerX + Math.Cos(angle) * radius;
            double y = centerY + Math.Sin(angle) * radius;
            rockPoints.Add(new Point(x, y));
        }

        var mainFigure = new PathFigure { StartPoint = rockPoints[0], IsClosed = true };
        for (int i = 0; i < rockPoints.Count; i++)
        {
            var p1 = rockPoints[i];
            var p2 = rockPoints[(i + 1) % rockPoints.Count];
            var midX = (p1.X + p2.X) / 2 + (_random.NextDouble() - 0.5) * 6;
            var midY = (p1.Y + p2.Y) / 2 + (_random.NextDouble() - 0.5) * 6;
            mainFigure.Segments.Add(new QuadraticBezierSegment(new Point(midX, midY), p2, true));
        }

        var mainGeometry = new PathGeometry();
        mainGeometry.Figures.Add(mainFigure);

        var mainRock = new Path
        {
            Data = mainGeometry,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.35, 0.35),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(68, 68, 72), 0.0),
                    new GradientStop(Color.FromRgb(52, 52, 58), 0.4),
                    new GradientStop(Color.FromRgb(38, 38, 45), 0.7),
                    new GradientStop(Color.FromRgb(28, 28, 35), 1.0)
                }
            }
        };
        canvas.Children.Add(mainRock);

        int crackCount = _random.Next(3, 6);
        for (int i = 0; i < crackCount; i++)
        {
            var crackFigure = new PathFigure();
            int segments = _random.Next(2, 5);
            double startX = centerX + (_random.NextDouble() - 0.5) * (TileSize - 24);
            double startY = centerY + (_random.NextDouble() - 0.5) * (TileSize - 24);

            crackFigure.StartPoint = new Point(startX, startY);

            for (int j = 0; j < segments; j++)
            {
                startX += (_random.NextDouble() - 0.5) * 14;
                startY += (_random.NextDouble() - 0.5) * 14;
                crackFigure.Segments.Add(new LineSegment(new Point(startX, startY), true));
            }

            var crackGeometry = new PathGeometry();
            crackGeometry.Figures.Add(crackFigure);

            var crack = new Path
            {
                Data = crackGeometry,
                Stroke = new SolidColorBrush(Color.FromArgb(70, 20, 20, 25)),
                StrokeThickness = 1.8
            };
            canvas.Children.Add(crack);
        }

        int spotCount = _random.Next(4, 8);
        for (int i = 0; i < spotCount; i++)
        {
            double spotX = centerX + (_random.NextDouble() - 0.5) * (TileSize - 20);
            double spotY = centerY + (_random.NextDouble() - 0.5) * (TileSize - 20);
            double spotSize = _random.NextDouble() * 4 + 3;

            int spotPoints = _random.Next(4, 7);
            var spotPts = new List<Point>();

            for (int j = 0; j < spotPoints; j++)
            {
                double a = (j / (double)spotPoints) * Math.PI * 2;
                double r = spotSize * (_random.NextDouble() * 0.3 + 0.7);
                spotPts.Add(new Point(spotX + Math.Cos(a) * r, spotY + Math.Sin(a) * r));
            }

            var spotFig = new PathFigure { StartPoint = spotPts[0], IsClosed = true };
            for (int j = 0; j < spotPts.Count; j++)
            {
                spotFig.Segments.Add(new LineSegment(spotPts[(j + 1) % spotPts.Count], true));
            }

            var spotGeo = new PathGeometry();
            spotGeo.Figures.Add(spotFig);

            byte grayShade = (byte)(_random.Next(75, 85));
            var spotPath = new Path
            {
                Data = spotGeo,
                Fill = new SolidColorBrush(Color.FromArgb(60, grayShade, grayShade, (byte)(grayShade + 5)))
            };
            canvas.Children.Add(spotPath);
        }

        return canvas;
    }

    private UIElement CreateTreeTile()
    {
        var canvas = new Canvas { Width = TileSize, Height = TileSize };
        var centerX = TileSize / 2.0;
        var centerY = TileSize / 2.0;

        int mainBlobCount = _random.Next(5, 8);
        var mainPoints = new List<Point>();

        for (int i = 0; i < mainBlobCount; i++)
        {
            double angle = (i / (double)mainBlobCount) * Math.PI * 2;
            double radiusVar = _random.NextDouble() * 0.5 + 0.5;
            double radius = (TileSize / 2.0 - 6) * radiusVar;

            angle += (_random.NextDouble() - 0.5) * 0.8;

            double x = centerX + Math.Cos(angle) * radius;
            double y = centerY + Math.Sin(angle) * radius;
            mainPoints.Add(new Point(x, y));
        }

        var mainFigure = new PathFigure { StartPoint = mainPoints[0], IsClosed = true };
        for (int i = 0; i < mainPoints.Count; i++)
        {
            var p1 = mainPoints[i];
            var p2 = mainPoints[(i + 1) % mainPoints.Count];
            var midX = (p1.X + p2.X) / 2 + (_random.NextDouble() - 0.5) * 8;
            var midY = (p1.Y + p2.Y) / 2 + (_random.NextDouble() - 0.5) * 8;
            mainFigure.Segments.Add(new QuadraticBezierSegment(new Point(midX, midY), p2, true));
        }

        var mainGeometry = new PathGeometry();
        mainGeometry.Figures.Add(mainFigure);

        var mainPath = new Path
        {
            Data = mainGeometry,
            Fill = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.4, 0.4),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(35, 58, 35), 0.0),
                    new GradientStop(Color.FromRgb(26, 44, 26), 0.6),
                    new GradientStop(Color.FromRgb(18, 32, 18), 1.0)
                }
            }
        };
        canvas.Children.Add(mainPath);

        int detailCount = _random.Next(3, 6);
        for (int d = 0; d < detailCount; d++)
        {
            double detailAngle = _random.NextDouble() * Math.PI * 2;
            double detailDist = _random.NextDouble() * (TileSize / 3.0);
            double detailCenterX = centerX + Math.Cos(detailAngle) * detailDist;
            double detailCenterY = centerY + Math.Sin(detailAngle) * detailDist;

            int detailPoints = _random.Next(4, 7);
            var detailPts = new List<Point>();
            double detailSize = _random.NextDouble() * 8 + 6;

            for (int i = 0; i < detailPoints; i++)
            {
                double a = (i / (double)detailPoints) * Math.PI * 2;
                double r = detailSize * (_random.NextDouble() * 0.4 + 0.6);
                detailPts.Add(new Point(detailCenterX + Math.Cos(a) * r, detailCenterY + Math.Sin(a) * r));
            }

            var detailFig = new PathFigure { StartPoint = detailPts[0], IsClosed = true };
            for (int i = 0; i < detailPts.Count; i++)
            {
                var p1 = detailPts[i];
                var p2 = detailPts[(i + 1) % detailPts.Count];
                detailFig.Segments.Add(new LineSegment(p2, true));
            }

            var detailGeo = new PathGeometry();
            detailGeo.Figures.Add(detailFig);

            byte greenShade = (byte)(_random.Next(42, 52));
            var detailPath = new Path
            {
                Data = detailGeo,
                Fill = new SolidColorBrush(Color.FromRgb((byte)(greenShade - 10), greenShade, (byte)(greenShade - 10)))
            };
            canvas.Children.Add(detailPath);
        }

        return canvas;
    }

    private UIElement CreateWaterTile()
    {
        return new Canvas { Width = TileSize, Height = TileSize };
    }

    private void CenterCamera()
    {
        if (_map == null) return;

        double centerX = (_map.Width * TileSize) / 2.0;
        double centerY = (_map.Height * TileSize) / 2.0;

        _translateTransform.X = ActualWidth / 2.0 - centerX;
        _translateTransform.Y = ActualHeight / 2.0 - centerY;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        double zoomFactor = e.Delta > 0 ? (1.0 + ZoomSpeed) : (1.0 - ZoomSpeed);
        double newScale = _scaleTransform.ScaleX * zoomFactor;

        newScale = Math.Clamp(newScale, MinZoom, MaxZoom);

        if (Math.Abs(newScale - _scaleTransform.ScaleX) < 0.001)
        {
            e.Handled = true;
            return;
        }

        double centerX = ActualWidth / 2.0;
        double centerY = ActualHeight / 2.0;

        Point centerPointOnCanvas = new Point(
            (centerX - _translateTransform.X) / _scaleTransform.ScaleX,
            (centerY - _translateTransform.Y) / _scaleTransform.ScaleY
        );

        _scaleTransform.ScaleX = newScale;
        _scaleTransform.ScaleY = newScale;

        _translateTransform.X = centerX - centerPointOnCanvas.X * newScale;
        _translateTransform.Y = centerY - centerPointOnCanvas.Y * newScale;

        e.Handled = true;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_isBuildMode)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point screenPos = e.GetPosition(ViewportGrid);
                Point mapPos = _transformGroup.Inverse.Transform(screenPos);

                if (_selectedBuildingType != null)
                {
                    var newBuilding = new Building(_selectedBuildingType.Value, mapPos);
                    bool hasOverlap = CheckBuildingOverlap(mapPos, newBuilding.Size);
                    bool canPlace = false;

                    var nearestBuilding = FindNearestBuilding(mapPos);
                    bool withinBuildingRadius = false;

                    if (nearestBuilding != null)
                    {
                        double distanceToBuilding = Math.Sqrt(
                            Math.Pow(mapPos.X - nearestBuilding.Position.X, 2) +
                            Math.Pow(mapPos.Y - nearestBuilding.Position.Y, 2)
                        );
                        withinBuildingRadius = distanceToBuilding <= GetEffectiveBuildRadius(nearestBuilding);
                    }

                    if (_selectedBuildingType == BuildingType.Mine)
                    {
                        var nearestRock = FindNearestTileOfType(mapPos, TileType.Rock);
                        bool nearResource = false;

                        if (nearestRock != null)
                        {
                            double tilePixelX = nearestRock.X * TileSize + nearestRock.OffsetX;
                            double tilePixelY = nearestRock.Y * TileSize + nearestRock.OffsetY;

                            double distanceToResource = Math.Sqrt(
                                Math.Pow(mapPos.X - tilePixelX, 2) +
                                Math.Pow(mapPos.Y - tilePixelY, 2)
                            );

                            nearResource = distanceToResource <= 100;
                        }

                        canPlace = withinBuildingRadius && nearResource && !hasOverlap;
                    }
                    else if (_selectedBuildingType == BuildingType.Sawmill)
                    {
                        var nearestTree = FindNearestTileOfType(mapPos, TileType.Tree);
                        bool nearTree = false;

                        if (nearestTree != null)
                        {
                            double tilePixelX = nearestTree.X * TileSize + nearestTree.OffsetX;
                            double tilePixelY = nearestTree.Y * TileSize + nearestTree.OffsetY;

                            double distanceToTree = Math.Sqrt(
                                Math.Pow(mapPos.X - tilePixelX, 2) +
                                Math.Pow(mapPos.Y - tilePixelY, 2)
                            );

                            nearTree = distanceToTree <= 100;
                        }

                        canPlace = withinBuildingRadius && nearTree && !hasOverlap;
                    }
                    else if (_selectedBuildingType == BuildingType.Market || _selectedBuildingType == BuildingType.Marketplace)
                    {
                        bool marketExists = _map.Buildings.Any(b => b.Type == BuildingType.Market || b.Type == BuildingType.Marketplace);

                        if (marketExists)
                        {
                            canPlace = false;
                        }
                        else
                        {
                            canPlace = withinBuildingRadius && !hasOverlap;
                        }
                    }
                    else
                    {
                        canPlace = withinBuildingRadius && !hasOverlap;
                    }

                    if (canPlace)
                    {
                        var buildCost = Building.GetBuildCost(_selectedBuildingType.Value);
                        double costMultiplier = GetBuildCostMultiplier();

                        bool canAfford = true;
                        foreach (var cost in buildCost)
                        {
                            int adjustedCost = (int)(cost.Value * costMultiplier);
                            if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < adjustedCost)
                            {
                                canAfford = false;
                                break;
                            }
                        }

                        if (!canAfford)
                        {
                            return;
                        }

                        foreach (var cost in buildCost)
                        {
                            int adjustedCost = (int)(cost.Value * costMultiplier);
                            _resources[cost.Key] -= adjustedCost;
                        }

                        UpdateResourceDisplay();

                        _map?.Buildings.Add(newBuilding);

                        RenderBuilding(newBuilding);

                        double effectiveRadius = GetEffectiveBuildRadius(newBuilding);
                        var radiusCircle = new Ellipse
                        {
                            Width = effectiveRadius * 2,
                            Height = effectiveRadius * 2,
                            Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                            StrokeThickness = 2,
                            Fill = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255))
                        };

                        Canvas.SetLeft(radiusCircle, newBuilding.Position.X - effectiveRadius);
                        Canvas.SetTop(radiusCircle, newBuilding.Position.Y - effectiveRadius);

                        _buildModeCanvas?.Children.Add(radiusCircle);
                        _buildRadiusCircles.Add(radiusCircle);

                        var buildingsInRange = FindBuildingsInRange(newBuilding.Position, 250);
                        foreach (var building in buildingsInRange)
                        {
                            if (building != newBuilding)
                            {
                                var road = new Line
                                {
                                    X1 = building.Position.X,
                                    Y1 = building.Position.Y,
                                    X2 = newBuilding.Position.X,
                                    Y2 = newBuilding.Position.Y,
                                    Stroke = new SolidColorBrush(Color.FromArgb(150, 100, 100, 100)),
                                    StrokeThickness = 2
                                };
                                MapCanvas.Children.Add(road);
                                _permanentRoads.Add(road);
                            }
                        }

                        ExitBuildMode();
                    }
                }

                e.Handled = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                ExitBuildMode();
                e.Handled = true;
            }
        }
        else if (e.ChangedButton == MouseButton.Left && !_isBuildMode)
        {
            Point screenPos = e.GetPosition(ViewportGrid);
            Point mapPos = _transformGroup.Inverse.Transform(screenPos);

            var clickedBuilding = GetBuildingAtPosition(mapPos);
            if (clickedBuilding != null)
            {
                ShowBuildingInfoPanel(clickedBuilding);
                e.Handled = true;
            }
        }

        if (e.MiddleButton == MouseButtonState.Pressed && !_isBuildMode)
        {
            _isPanning = true;
            _lastMousePosition = e.GetPosition(this);
            CaptureMouse();
            Cursor = Cursors.SizeAll;
            e.Handled = true;
        }
    }

    private void RenderBuilding(Building building)
    {
        var visual = CreateBuildingVisual(building);
        Canvas.SetLeft(visual, building.Position.X - building.Size / 2);
        Canvas.SetTop(visual, building.Position.Y - building.Size / 2);
        MapCanvas.Children.Add(visual);
    }

    private UIElement CreateBuildingVisual(Building building)
    {
        var color = building.Type switch
        {
            BuildingType.Base => Color.FromRgb(100, 150, 255),
            BuildingType.Factory => Color.FromRgb(200, 100, 50),
            BuildingType.Mine => Color.FromRgb(150, 150, 150),
            BuildingType.MeatFactory => Color.FromRgb(180, 80, 80),
            BuildingType.Sawmill => Color.FromRgb(139, 90, 43),
            BuildingType.Market => Color.FromRgb(200, 180, 50),
            BuildingType.Marketplace => Color.FromRgb(220, 200, 80),
            BuildingType.Furnace => Color.FromRgb(200, 100, 30),
            _ => Color.FromRgb(128, 128, 128)
        };

        var ellipse = new Ellipse
        {
            Width = building.Size,
            Height = building.Size,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb((byte)(color.R * 1.3), (byte)(color.G * 1.3), (byte)(color.B * 1.3)), 0.0),
                    new GradientStop(color, 0.7),
                    new GradientStop(Color.FromRgb((byte)(color.R * 0.5), (byte)(color.G * 0.5), (byte)(color.B * 0.5)), 1.0)
                }
            }
        };

        return ellipse;
    }

    private void ExitBuildMode()
    {
        _isBuildMode = false;
        _selectedBuildingType = null;

        if (_buildModeCanvas != null)
        {
            _buildModeCanvas.Children.Clear();
            _buildModeCanvas.Visibility = Visibility.Collapsed;
        }

        _buildRadiusCircles.Clear();
        _buildPrototype = null;
        _buildRoadLine = null;
        _buildRoadLines.Clear();
        _buildErrorTooltip = null;

        RightPanel.IsHitTestVisible = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.MiddleButton == MouseButtonState.Released && _isPanning)
        {
            _isPanning = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_currentOpenPanel != "")
            {
                CloseRightPanel();
                e.Handled = true;
            }
            else if (_isBuildMode)
            {
                ExitBuildMode();
                e.Handled = true;
            }
            else
            {
                TogglePause();
                e.Handled = true;
            }
        }
    }

    private void TogglePause()
    {
        if (_isPaused)
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        _isPaused = true;
        _gameTimer.Stop();
        PauseOverlay.Visibility = Visibility.Visible;
    }

    private void HidePauseMenu()
    {
        _isPaused = false;
        _gameTimer.Start();
        PauseOverlay.Visibility = Visibility.Collapsed;
    }

    public void ShowPauseMenuAfterLoad()
    {
        ShowPauseMenu();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_isPanning && e.MiddleButton == MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(this);
            Vector delta = currentPosition - _lastMousePosition;

            _translateTransform.X += delta.X;
            _translateTransform.Y += delta.Y;

            _lastMousePosition = currentPosition;
            e.Handled = true;
        }

        if (_isBuildMode && _buildPrototype != null && _buildRoadLine != null && _map != null)
        {
            Point screenPos = e.GetPosition(ViewportGrid);
            Point mapPos = _transformGroup.Inverse.Transform(screenPos);

            double size = _buildPrototype.Width;
            Canvas.SetLeft(_buildPrototype, mapPos.X - size / 2);
            Canvas.SetTop(_buildPrototype, mapPos.Y - size / 2);
            _buildPrototype.Visibility = Visibility.Visible;

            foreach (var line in _buildRoadLines)
            {
                line.Visibility = Visibility.Collapsed;
            }

            if (_buildErrorTooltip != null)
            {
                _buildErrorTooltip.Visibility = Visibility.Collapsed;
            }

            bool hasOverlap = CheckBuildingOverlap(mapPos, size);
            var nearestBuilding = FindNearestBuilding(mapPos);
            bool withinBuildingRadius = false;
            string errorMessage = "";

            if (nearestBuilding != null)
            {
                double distanceToBuilding = Math.Sqrt(
                    Math.Pow(mapPos.X - nearestBuilding.Position.X, 2) +
                    Math.Pow(mapPos.Y - nearestBuilding.Position.Y, 2)
                );
                withinBuildingRadius = distanceToBuilding <= GetEffectiveBuildRadius(nearestBuilding);
            }
            else
            {
                errorMessage = "Занадто далеко від будівель";
            }

            bool isValid = false;

            if (_selectedBuildingType == BuildingType.Mine)
            {
                var nearestRock = FindNearestTileOfType(mapPos, TileType.Rock);
                bool nearResource = false;

                if (nearestRock != null)
                {
                    double tilePixelX = nearestRock.X * TileSize + nearestRock.OffsetX;
                    double tilePixelY = nearestRock.Y * TileSize + nearestRock.OffsetY;

                    double distanceToResource = Math.Sqrt(
                        Math.Pow(mapPos.X - tilePixelX, 2) +
                        Math.Pow(mapPos.Y - tilePixelY, 2)
                    );

                    nearResource = distanceToResource <= 100;
                }

                if (!withinBuildingRadius && !nearResource)
                {
                    errorMessage = "Шахта: занадто далеко від будівель та каменів";
                }
                else if (!withinBuildingRadius)
                {
                    errorMessage = "Шахта: занадто далеко від будівель";
                }
                else if (!nearResource)
                {
                    errorMessage = "Шахта: має бути біля каменів";
                }
                else if (hasOverlap)
                {
                    errorMessage = "Перетинається з іншою будівлею";
                }

                isValid = withinBuildingRadius && nearResource && !hasOverlap;
            }
            else if (_selectedBuildingType == BuildingType.Sawmill)
            {
                var nearestTree = FindNearestTileOfType(mapPos, TileType.Tree);
                bool nearTree = false;

                if (nearestTree != null)
                {
                    double tilePixelX = nearestTree.X * TileSize + nearestTree.OffsetX;
                    double tilePixelY = nearestTree.Y * TileSize + nearestTree.OffsetY;

                    double distanceToTree = Math.Sqrt(
                        Math.Pow(mapPos.X - tilePixelX, 2) +
                        Math.Pow(mapPos.Y - tilePixelY, 2)
                    );

                    nearTree = distanceToTree <= 100;
                }

                if (!withinBuildingRadius && !nearTree)
                {
                    errorMessage = "Лісопилка: занадто далеко від будівель та дерев";
                }
                else if (!withinBuildingRadius)
                {
                    errorMessage = "Лісопилка: занадто далеко від будівель";
                }
                else if (!nearTree)
                {
                    errorMessage = "Лісопилка: має бути біля дерев";
                }
                else if (hasOverlap)
                {
                    errorMessage = "Перетинається з іншою будівлею";
                }

                isValid = withinBuildingRadius && nearTree && !hasOverlap;
            }
            else if (_selectedBuildingType == BuildingType.Market || _selectedBuildingType == BuildingType.Marketplace)
            {
                bool marketExists = _map.Buildings.Any(b => b.Type == BuildingType.Market || b.Type == BuildingType.Marketplace);

                if (marketExists)
                {
                    errorMessage = "Маркет може бути лише один";
                    isValid = false;
                }
                else if (!withinBuildingRadius)
                {
                    errorMessage = "Занадто далеко від будівель";
                }
                else if (hasOverlap)
                {
                    errorMessage = "Перетинається з іншою будівлею";
                }
                else
                {
                    isValid = withinBuildingRadius && !hasOverlap;
                }
            }
            else
            {
                if (!withinBuildingRadius)
                {
                    errorMessage = "Занадто далеко від будівель";
                }
                else if (hasOverlap)
                {
                    errorMessage = "Перетинається з іншою будівлею";
                }

                isValid = withinBuildingRadius && !hasOverlap;
            }

            if (isValid)
            {
                _buildPrototype.Fill = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255));

                var buildingsInRange = FindBuildingsInRange(mapPos, 250);
                int roadIndex = 0;

                foreach (var building in buildingsInRange)
                {
                    if (roadIndex < _buildRoadLines.Count)
                    {
                        var roadLine = _buildRoadLines[roadIndex];
                        roadLine.X1 = building.Position.X;
                        roadLine.Y1 = building.Position.Y;
                        roadLine.X2 = mapPos.X;
                        roadLine.Y2 = mapPos.Y;
                        roadLine.Visibility = Visibility.Visible;
                        roadIndex++;
                    }
                }
            }
            else
            {
                _buildPrototype.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 100, 100));

                if (_buildErrorTooltip != null && !string.IsNullOrEmpty(errorMessage))
                {
                    _buildErrorTooltip.Text = errorMessage;
                    Canvas.SetLeft(_buildErrorTooltip, mapPos.X - 50);
                    Canvas.SetTop(_buildErrorTooltip, mapPos.Y - 40);
                    _buildErrorTooltip.Visibility = Visibility.Visible;
                }
            }

            e.Handled = true;
        }
    }

    private Building? FindNearestBuilding(Point position)
    {
        if (_map == null || _map.Buildings.Count == 0) return null;

        Building? nearest = null;
        double minDistance = double.MaxValue;

        foreach (var building in _map.Buildings)
        {
            double distance = Math.Sqrt(
                Math.Pow(position.X - building.Position.X, 2) +
                Math.Pow(position.Y - building.Position.Y, 2)
            );

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = building;
            }
        }

        return nearest;
    }

    private List<Building> FindBuildingsInRange(Point position, double maxDistance)
    {
        var result = new List<Building>();
        if (_map == null) return result;

        foreach (var building in _map.Buildings)
        {
            double distance = Math.Sqrt(
                Math.Pow(position.X - building.Position.X, 2) +
                Math.Pow(position.Y - building.Position.Y, 2)
            );

            if (distance <= maxDistance)
            {
                result.Add(building);
            }
        }

        return result;
    }

    private bool CheckBuildingOverlap(Point position, double size)
    {
        if (_map == null) return false;

        foreach (var building in _map.Buildings)
        {
            double distance = Math.Sqrt(
                Math.Pow(position.X - building.Position.X, 2) +
                Math.Pow(position.Y - building.Position.Y, 2)
            );

            double minDistance = (size + building.Size) / 2;
            if (distance < minDistance)
                return true;
        }

        return false;
    }

    private Building? GetBuildingAtPosition(Point position)
    {
        if (_map == null) return null;

        foreach (var building in _map.Buildings)
        {
            double distance = Math.Sqrt(
                Math.Pow(position.X - building.Position.X, 2) +
                Math.Pow(position.Y - building.Position.Y, 2)
            );

            if (distance <= building.Size / 2)
            {
                return building;
            }
        }

        return null;
    }

    private Tile? FindNearestTileOfType(Point position, TileType tileType)
    {
        if (_map == null) return null;

        Tile? nearest = null;
        double minDistance = double.MaxValue;

        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                var tile = _map.GetTile(x, y);
                if (tile != null && tile.Type == tileType)
                {
                    double tilePixelX = x * TileSize + tile.OffsetX;
                    double tilePixelY = y * TileSize + tile.OffsetY;

                    double distance = Math.Sqrt(
                        Math.Pow(position.X - tilePixelX, 2) +
                        Math.Pow(position.Y - tilePixelY, 2)
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

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (_gameSpeed > 0)
        {
            double deltaTime = 0.1 * _gameSpeed;
            _elapsedTime += deltaTime;
            int totalSeconds = (int)_elapsedTime;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            TimerText.Text = $"{minutes:D2}:{seconds:D2}";

            UpdateProduction(deltaTime);
            UpdateResourceDisplay();
            UpdateResearch(deltaTime);
            UpdateInvestment(deltaTime);
        }
    }

    private void UpdateResearch(double deltaTime)
    {
        if (_currentResearch != null && _currentResearch.IsResearching)
        {
            double researchSpeed = GetResearchSpeedMultiplier();
            _currentResearch.Progress += (deltaTime / _currentResearch.Duration) * researchSpeed;

            if (_currentResearch.Progress >= 1.0)
            {
                _currentResearch.Progress = 1.0;
                _currentResearch.IsResearching = false;
                _currentResearch.IsCompleted = true;
                _completedResearch.Add(_currentResearch);
                _currentResearch = null;

                if (_currentOpenPanel == "research")
                {
                    RightPanelContent.Content = CreateResearchPanelContent();
                }
            }
            else if (_currentOpenPanel == "research")
            {
                RightPanelContent.Content = CreateResearchPanelContent();
            }
        }
    }

    private void UpdateInvestment(double deltaTime)
    {
        if (_map == null) return;

        foreach (var building in _map.Buildings)
        {
            if (building.Type == BuildingType.Market)
            {
                if (building.IsInvesting)
                {
                    double investmentDuration = building.Level == 1 ? 200.0 : 180.0;
                    building.InvestmentProgress += deltaTime / investmentDuration;

                    if (building.InvestmentProgress >= 1.0)
                    {
                        building.InvestmentProgress = 1.0;
                        building.IsInvesting = false;

                        int returnAmount = (int)(building.InvestmentAmount * 1.25);
                        if (!_resources.ContainsKey(building.InvestmentResource!.Value))
                        {
                            _resources[building.InvestmentResource!.Value] = 0;
                        }
                        _resources[building.InvestmentResource!.Value] += returnAmount;

                        building.InvestmentCooldown = investmentDuration;
                        building.InvestmentResource = null;
                        building.InvestmentAmount = 0;

                        UpdateResourceDisplay();

                        if (_selectedBuilding == building)
                        {
                            RightPanelContent.Content = CreateBuildingInfoPanelContent(building);
                        }
                    }
                    else if (_selectedBuilding == building)
                    {
                        RightPanelContent.Content = CreateBuildingInfoPanelContent(building);
                    }
                }
                else if (building.InvestmentCooldown > 0)
                {
                    building.InvestmentCooldown -= deltaTime;
                    if (building.InvestmentCooldown < 0)
                    {
                        building.InvestmentCooldown = 0;
                    }

                    if (_selectedBuilding == building)
                    {
                        RightPanelContent.Content = CreateBuildingInfoPanelContent(building);
                    }
                }
            }
        }
    }

    private bool IsResearchCompleted(ResearchType type)
    {
        return _completedResearch.Any(r => r.Type == type);
    }

    private double GetProductionSpeedMultiplier()
    {
        return IsResearchCompleted(ResearchType.ImprovedProduction) ? 1.15 : 1.0;
    }

    private double GetMineOutputMultiplier()
    {
        return IsResearchCompleted(ResearchType.AdvancedMining) ? 1.5 : 1.0;
    }

    private double GetFactoryOutputMultiplier()
    {
        return IsResearchCompleted(ResearchType.OrganicBoost) ? 1.4 : 1.0;
    }

    private double GetBuildCostMultiplier()
    {
        return IsResearchCompleted(ResearchType.EfficientConstruction) ? 0.8 : 1.0;
    }

    private double GetResearchSpeedMultiplier()
    {
        return IsResearchCompleted(ResearchType.FastLearning) ? 1.25 : 1.0;
    }

    private double GetBuildRadiusMultiplier()
    {
        return IsResearchCompleted(ResearchType.ExtendedRadius) ? 1.3 : 1.0;
    }

    private double GetEffectiveBuildRadius(Building building)
    {
        return building.BuildRadius * GetBuildRadiusMultiplier();
    }

    private void UpdateProduction(double deltaTime)
    {
        if (_map == null) return;

        foreach (var building in _map.Buildings)
        {
            if (building.CanProduce && building.ProductionTime > 0)
            {
                double productionSpeed = GetProductionSpeedMultiplier();
                building.ProductionProgress += (deltaTime / building.ProductionTime) * productionSpeed;

                if (building.ProductionProgress >= 1.0)
                {
                    building.ProductionProgress -= 1.0;

                    bool canProduce = true;

                    if (building.Type == BuildingType.Furnace)
                    {
                        int woodNeeded = building.ProductionOutput.ContainsKey(ResourceType.Coal)
                            ? building.ProductionOutput[ResourceType.Coal]
                            : 1;

                        if (!_resources.ContainsKey(ResourceType.Wood) || _resources[ResourceType.Wood] < woodNeeded)
                        {
                            canProduce = false;
                            building.ProductionProgress = 0;
                        }
                        else
                        {
                            _resources[ResourceType.Wood] -= woodNeeded;
                        }
                    }

                    if (canProduce)
                    {
                        foreach (var output in building.ProductionOutput)
                        {
                            if (!_resources.ContainsKey(output.Key))
                            {
                                _resources[output.Key] = 0;
                            }

                            int outputAmount = output.Value;

                            if (building.Type == BuildingType.Mine && output.Key == ResourceType.Metal)
                            {
                                outputAmount = (int)(output.Value * GetMineOutputMultiplier());
                            }
                            else if (building.Type == BuildingType.Factory && output.Key == ResourceType.Organic)
                            {
                                outputAmount = (int)(output.Value * GetFactoryOutputMultiplier());
                            }

                            _resources[output.Key] += outputAmount;
                        }

                        ShowProductionEffect(building);
                    }
                }

                if (_selectedBuilding == building && _currentProgressBars.Count > 0)
                {
                    foreach (var progressBar in _currentProgressBars)
                    {
                        progressBar.Value = building.ProductionProgress * 100;
                    }
                }
            }
        }
    }

    private void UpdateResourceDisplay()
    {
        MetalText.Text = _resources.ContainsKey(ResourceType.Metal) ? _resources[ResourceType.Metal].ToString() : "0";
        OrganicText.Text = _resources.ContainsKey(ResourceType.Organic) ? _resources[ResourceType.Organic].ToString() : "0";
        MeatText.Text = _resources.ContainsKey(ResourceType.Meat) ? _resources[ResourceType.Meat].ToString() : "0";
        WoodText.Text = _resources.ContainsKey(ResourceType.Wood) ? _resources[ResourceType.Wood].ToString() : "0";
        CoalText.Text = _resources.ContainsKey(ResourceType.Coal) ? _resources[ResourceType.Coal].ToString() : "0";
    }

    private void ShowProductionEffect(Building building)
    {
        var effectText = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
            Text = "+",
            Opacity = 1.0
        };

        Canvas.SetLeft(effectText, building.Position.X - 10);
        Canvas.SetTop(effectText, building.Position.Y - 30);
        MapCanvas.Children.Add(effectText);

        var fadeAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromSeconds(1.0)
        };

        var moveAnimation = new DoubleAnimation
        {
            From = building.Position.Y - 30,
            To = building.Position.Y - 60,
            Duration = TimeSpan.FromSeconds(1.0)
        };

        fadeAnimation.Completed += (s, e) => MapCanvas.Children.Remove(effectText);

        effectText.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
        effectText.BeginAnimation(Canvas.TopProperty, moveAnimation);
    }

    private string GetResourceName(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Metal => "Метал",
            ResourceType.Organic => "Органіка",
            ResourceType.Meat => "М'ясо",
            ResourceType.Wood => "Дерево",
            ResourceType.Coal => "Вуголь",
            _ => "Ресурс"
        };
    }

    private void UpgradeBuilding(Building building)
    {
        if (!building.CanUpgrade())
        {
            return;
        }

        var upgradeCost = building.GetUpgradeCost();

        foreach (var cost in upgradeCost)
        {
            if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < cost.Value)
            {
                return;
            }
        }

        foreach (var cost in upgradeCost)
        {
            _resources[cost.Key] -= cost.Value;
        }

        building.Level++;

        switch (building.Type)
        {
            case BuildingType.Factory:
                building.InitializeFactoryProduction(building.Level);
                break;
            case BuildingType.Mine:
                building.InitializeMineProduction(building.Level);
                break;
            case BuildingType.MeatFactory:
                building.InitializeMeatFactoryProduction(building.Level);
                break;
            case BuildingType.Sawmill:
                building.InitializeSawmillProduction(building.Level);
                break;
        }

        building.ProductionProgress = 0;

        ShowBuildingInfoPanel(building);
    }

    private void StartResearch(Research research)
    {
        if (_currentResearch != null && _currentResearch.IsResearching)
        {
            return;
        }

        foreach (var cost in research.Cost)
        {
            if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < cost.Value)
            {
                return;
            }
        }

        foreach (var cost in research.Cost)
        {
            _resources[cost.Key] -= cost.Value;
        }

        research.IsResearching = true;
        research.Progress = 0;
        _currentResearch = research;

        if (_currentOpenPanel == "research")
        {
            RightPanelContent.Content = CreateResearchPanelContent();
        }
    }

    private void StartInvestment(Building building, ResourceType resourceType, int amount)
    {
        if (building.Type != BuildingType.Market || building.IsInvesting || building.InvestmentCooldown > 0)
        {
            return;
        }

        if (!_resources.ContainsKey(resourceType) || _resources[resourceType] < amount)
        {
            return;
        }

        _resources[resourceType] -= amount;
        UpdateResourceDisplay();

        building.InvestmentResource = resourceType;
        building.InvestmentAmount = amount;
        building.IsInvesting = true;
        building.InvestmentProgress = 0;

        if (_selectedBuilding == building)
        {
            RightPanelContent.Content = CreateBuildingInfoPanelContent(building);
        }
    }

    private void SpeedButton_Click(object sender, RoutedEventArgs e)
    {
        _gameSpeed = (_gameSpeed + 1) % 4;
        UpdateSpeedButton();
    }

    private void UpdateSpeedButton()
    {
        SpeedButton.Content = _gameSpeed switch
        {
            0 => "⏸",
            1 => "▶",
            2 => "▶▶",
            3 => "▶▶▶",
            _ => "▶"
        };
    }

    private void UpdateBuildingsCount()
    {
        if (_map != null)
        {
            BuildingsText.Text = $" {_map.Buildings.Count}";
        }
    }

    private void BuildButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleRightPanel("build", "БУДІВНИЦТВО");
    }

    private void ResearchButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleRightPanel("research", "ДОСЛІДЖЕННЯ");
    }

    private void TradeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleRightPanel("trade", "ТРЕЙДИНГ");
    }

    private void ResumeButton_Click(object sender, RoutedEventArgs e)
    {
        HidePauseMenu();
    }

    private void PauseSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SaveGameState();
        services.NavigationService.Instance.SetReturnToGameWithPause(true);
        services.NavigationService.Instance.NavigateToSettings();
    }

    private void SaveAndExitButton_Click(object sender, RoutedEventArgs e)
    {
        SaveGameState();
        services.NavigationService.Instance.NavigateToMainMenu();
    }

    private void SaveGameState()
    {
        if (_map == null) return;

        var gameState = new GameState
        {
            Resources = new Dictionary<ResourceType, int>(_resources),
            GameSpeed = _gameSpeed,
            ElapsedTime = _elapsedTime
        };

        // Save buildings
        foreach (var building in _map.Buildings)
        {
            gameState.Buildings.Add(BuildingData.FromBuilding(building));
        }

        // Save research
        foreach (var research in _availableResearch)
        {
            gameState.AvailableResearch.Add(ResearchData.FromResearch(research));
        }

        foreach (var research in _completedResearch)
        {
            gameState.CompletedResearch.Add(ResearchData.FromResearch(research));
        }

        if (_currentResearch != null)
        {
            gameState.CurrentResearch = ResearchData.FromResearch(_currentResearch);
        }

        // Save map
        gameState.Map = MapData.FromGameMap(_map);

        // Save camera
        gameState.Camera = new CameraData
        {
            ScaleX = _scaleTransform.ScaleX,
            ScaleY = _scaleTransform.ScaleY,
            TranslateX = _translateTransform.X,
            TranslateY = _translateTransform.Y
        };

        services.SaveLoadService.Instance.SaveGame(gameState);
    }

    public void LoadGameState(GameState gameState)
    {
        // Restore resources
        _resources = new Dictionary<ResourceType, int>(gameState.Resources);
        UpdateResourceDisplay();

        // Restore game speed and time
        _gameSpeed = gameState.GameSpeed;
        _elapsedTime = gameState.ElapsedTime;
        UpdateSpeedButton();

        // Restore map
        _map = gameState.Map.ToGameMap();

        // Restore buildings
        foreach (var buildingData in gameState.Buildings)
        {
            _map.Buildings.Add(buildingData.ToBuilding());
        }

        // Restore research
        _availableResearch.Clear();
        foreach (var researchData in gameState.AvailableResearch)
        {
            _availableResearch.Add(researchData.ToResearch());
        }

        _completedResearch.Clear();
        foreach (var researchData in gameState.CompletedResearch)
        {
            _completedResearch.Add(researchData.ToResearch());
        }

        if (gameState.CurrentResearch != null)
        {
            _currentResearch = gameState.CurrentResearch.ToResearch();
        }

        // Restore camera
        _scaleTransform.ScaleX = gameState.Camera.ScaleX;
        _scaleTransform.ScaleY = gameState.Camera.ScaleY;
        _translateTransform.X = gameState.Camera.TranslateX;
        _translateTransform.Y = gameState.Camera.TranslateY;

        // Re-render everything
        RenderMap();
        UpdateBuildingsCount();
    }

    private UIElement CreateBuildPanelContent()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

        var factoryCost = Building.GetBuildCost(BuildingType.Factory);
        var factoryButton = new Button
        {
            Content = $"ФАБРИКА\n{FormatCost(factoryCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        factoryButton.Click += (s, e) => StartBuildMode(BuildingType.Factory);

        var mineCost = Building.GetBuildCost(BuildingType.Mine);
        var mineButton = new Button
        {
            Content = $"ШАХТА\n{FormatCost(mineCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        mineButton.Click += (s, e) => StartBuildMode(BuildingType.Mine);

        var meatFactoryCost = Building.GetBuildCost(BuildingType.MeatFactory);
        var meatFactoryButton = new Button
        {
            Content = $"М'ЯСОФАБРИКА\n{FormatCost(meatFactoryCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        meatFactoryButton.Click += (s, e) => StartBuildMode(BuildingType.MeatFactory);

        var sawmillCost = Building.GetBuildCost(BuildingType.Sawmill);
        var sawmillButton = new Button
        {
            Content = $"ЛІСОПИЛКА\n{FormatCost(sawmillCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        sawmillButton.Click += (s, e) => StartBuildMode(BuildingType.Sawmill);

        var marketCost = Building.GetBuildCost(BuildingType.Market);
        var marketButton = new Button
        {
            Content = $"РИНОК\n{FormatCost(marketCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        marketButton.Click += (s, e) => StartBuildMode(BuildingType.Market);

        var marketplaceCost = Building.GetBuildCost(BuildingType.Marketplace);
        var marketplaceButton = new Button
        {
            Content = $"МАРКЕТ\n{FormatCost(marketplaceCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        marketplaceButton.Click += (s, e) => StartBuildMode(BuildingType.Marketplace);

        var furnaceCost = Building.GetBuildCost(BuildingType.Furnace);
        var furnaceButton = new Button
        {
            Content = $"ПЕЧКА\n{FormatCost(furnaceCost)}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        furnaceButton.Click += (s, e) => StartBuildMode(BuildingType.Furnace);

        panel.Children.Add(factoryButton);
        panel.Children.Add(mineButton);
        panel.Children.Add(meatFactoryButton);
        panel.Children.Add(sawmillButton);
        panel.Children.Add(marketButton);
        panel.Children.Add(marketplaceButton);
        panel.Children.Add(furnaceButton);

        return panel;
    }

    private string FormatCost(Dictionary<ResourceType, int> cost)
    {
        var parts = new List<string>();
        foreach (var item in cost)
        {
            string resourceName = item.Key switch
            {
                ResourceType.Metal => "Метал",
                ResourceType.Organic => "Органіка",
                ResourceType.Meat => "М'ясо",
                ResourceType.Wood => "Дерево",
                ResourceType.Coal => "Вугілля",
                _ => item.Key.ToString()
            };
            parts.Add($"{item.Value} {resourceName}");
        }
        return string.Join(", ", parts);
    }

    private UIElement CreateResearchPanelContent()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

        if (_currentResearch != null && _currentResearch.IsResearching)
        {
            var currentResearchPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };

            var nameText = new TextBlock
            {
                Text = _currentResearch.Name,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 200, 255)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            currentResearchPanel.Children.Add(nameText);

            var progressBar = new System.Windows.Controls.ProgressBar
            {
                Height = 12,
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 200, 255)),
                Value = _currentResearch.Progress * 100,
                Maximum = 100,
                Margin = new Thickness(0, 0, 0, 5)
            };
            currentResearchPanel.Children.Add(progressBar);

            var timeText = new TextBlock
            {
                Text = $"Залишилось: {(_currentResearch.Duration * (1 - _currentResearch.Progress)):F1}с",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 11
            };
            currentResearchPanel.Children.Add(timeText);

            panel.Children.Add(currentResearchPanel);
        }

        var availableLabel = new TextBlock
        {
            Text = "ДОСТУПНІ ДОСЛІДЖЕННЯ:",
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(availableLabel);

        foreach (var research in _availableResearch)
        {
            if (research.IsCompleted) continue;

            var researchPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255))
            };

            var researchName = new TextBlock
            {
                Text = research.Name,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 5, 5, 3)
            };
            researchPanel.Children.Add(researchName);

            var researchDesc = new TextBlock
            {
                Text = research.Description,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 0, 5, 5)
            };
            researchPanel.Children.Add(researchDesc);

            var costText = "";
            foreach (var cost in research.Cost)
            {
                if (costText.Length > 0) costText += ", ";
                costText += $"{GetResourceName(cost.Key)}: {cost.Value}";
            }

            var costLabel = new TextBlock
            {
                Text = $"Вартість: {costText}",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 10,
                Margin = new Thickness(5, 0, 5, 5)
            };
            researchPanel.Children.Add(costLabel);

            var researchButton = new Button
            {
                Content = $"ДОСЛІДИТИ ({research.Duration}с)",
                Height = 35,
                Margin = new Thickness(5, 0, 5, 5),
                Style = (Style)FindResource("GameButtonStyle"),
                FontSize = 11
            };

            bool canAfford = true;
            foreach (var cost in research.Cost)
            {
                if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < cost.Value)
                {
                    canAfford = false;
                    break;
                }
            }

            if (!canAfford || (_currentResearch != null && _currentResearch.IsResearching))
            {
                researchButton.Opacity = 0.5;
                researchButton.IsEnabled = false;
            }

            researchButton.Click += (s, e) => StartResearch(research);
            researchPanel.Children.Add(researchButton);

            panel.Children.Add(researchPanel);
        }

        return panel;
    }

    private UIElement CreateTradePanelContent()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

        bool hasMarketplace = _map?.Buildings.Any(b => b.Type == BuildingType.Marketplace) ?? false;

        if (!hasMarketplace)
        {
            var messageText = new TextBlock
            {
                Text = "Потрібно побудувати Маркет для торгівлі",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 100, 100)),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            panel.Children.Add(messageText);
            return panel;
        }

        // Stage 1: Choose resource to give
        if (_tradeStage == 1)
        {
            var infoText = new TextBlock
            {
                Text = "Крок 1: Оберіть ресурс для обміну",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(infoText);

            var rateText = new TextBlock
            {
                Text = "Курс обміну: 100% → 60%",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(rateText);

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                if (!_resources.ContainsKey(resource) || _resources[resource] <= 100)
                    continue;

                var resourceButton = new Button
                {
                    Content = $"{GetResourceName(resource)}\n(є: {_resources[resource]})",
                    Height = 60,
                    Margin = new Thickness(0, 0, 0, 10),
                    Style = (Style)FindResource("GameButtonStyle"),
                    FontSize = 12
                };
                resourceButton.Click += (s, e) =>
                {
                    _tradeFromResource = resource;
                    _tradeFromAmount = 100;
                    _tradeStage = 2;
                    RightPanelContent.Content = CreateTradePanelContent();
                };
                panel.Children.Add(resourceButton);
            }
        }
        // Stage 2: Choose amount to give
        else if (_tradeStage == 2 && _tradeFromResource.HasValue)
        {
            var backButton = new Button
            {
                Content = "← Назад",
                Width = 100,
                Height = 35,
                Margin = new Thickness(0, 0, 0, 15),
                Style = (Style)FindResource("GameButtonStyle"),
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            backButton.Click += (s, e) =>
            {
                _tradeStage = 1;
                _tradeFromResource = null;
                _tradeFromAmount = 0;
                RightPanelContent.Content = CreateTradePanelContent();
            };
            panel.Children.Add(backButton);

            var infoText = new TextBlock
            {
                Text = $"Крок 2: Оберіть кількість {GetResourceName(_tradeFromResource.Value)}",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(infoText);

            var availableText = new TextBlock
            {
                Text = $"Доступно: {_resources[_tradeFromResource.Value]}",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(availableText);

            var amountText = new TextBlock
            {
                Text = $"Кількість: {_tradeFromAmount}",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 5)
            };
            panel.Children.Add(amountText);

            var slider = new Slider
            {
                Minimum = 1,
                Maximum = _resources[_tradeFromResource.Value],
                Value = _tradeFromAmount,
                TickFrequency = 10,
                IsSnapToTickEnabled = true,
                Margin = new Thickness(0, 0, 0, 15)
            };
            slider.ValueChanged += (s, e) =>
            {
                _tradeFromAmount = (int)slider.Value;
                amountText.Text = $"Кількість: {_tradeFromAmount}";
            };
            panel.Children.Add(slider);

            var nextButton = new Button
            {
                Content = "Далі →",
                Height = 45,
                Margin = new Thickness(0, 10, 0, 0),
                Style = (Style)FindResource("GameButtonStyle"),
                FontSize = 12
            };
            nextButton.Click += (s, e) =>
            {
                _tradeStage = 3;
                RightPanelContent.Content = CreateTradePanelContent();
            };
            panel.Children.Add(nextButton);
        }
        // Stage 3: Choose resource to receive and confirm
        else if (_tradeStage == 3 && _tradeFromResource.HasValue)
        {
            var backButton = new Button
            {
                Content = "← Назад",
                Width = 100,
                Height = 35,
                Margin = new Thickness(0, 0, 0, 15),
                Style = (Style)FindResource("GameButtonStyle"),
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            backButton.Click += (s, e) =>
            {
                _tradeStage = 2;
                _tradeToResource = null;
                RightPanelContent.Content = CreateTradePanelContent();
            };
            panel.Children.Add(backButton);

            var infoText = new TextBlock
            {
                Text = "Крок 3: Оберіть ресурс для отримання",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(infoText);

            var givingText = new TextBlock
            {
                Text = $"Віддаєте: {_tradeFromAmount} {GetResourceName(_tradeFromResource.Value)}",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 150, 150)),
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(givingText);

            if (_tradeToResource.HasValue)
            {
                int receiveAmount = (int)(_tradeFromAmount * 0.6);

                var calculationBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(40, 100, 200, 100)),
                    Margin = new Thickness(0, 0, 0, 15),
                    Padding = new Thickness(10)
                };

                var calculationPanel = new StackPanel();

                var calcText = new TextBlock
                {
                    Text = $"Отримаєте: {receiveAmount} {GetResourceName(_tradeToResource.Value)}",
                    Foreground = new SolidColorBrush(Color.FromRgb(150, 250, 150)),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                calculationPanel.Children.Add(calcText);

                var rateText = new TextBlock
                {
                    Text = $"Курс: {_tradeFromAmount} → {receiveAmount} ({(int)(0.6 * 100)}%)",
                    Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                    FontSize = 11
                };
                calculationPanel.Children.Add(rateText);

                calculationBorder.Child = calculationPanel;
                panel.Children.Add(calculationBorder);

                var confirmButton = new Button
                {
                    Content = "✓ Підтвердити обмін",
                    Height = 50,
                    Margin = new Thickness(0, 10, 0, 0),
                    Style = (Style)FindResource("GameButtonStyle"),
                    FontSize = 13,
                    Background = new SolidColorBrush(Color.FromRgb(50, 150, 50))
                };
                confirmButton.Click += (s, e) =>
                {
                    ExecuteTrade(_tradeFromResource.Value, _tradeFromAmount, _tradeToResource.Value);
                };
                panel.Children.Add(confirmButton);
            }
            else
            {
                foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
                {
                    if (resource == _tradeFromResource.Value)
                        continue;

                    int receiveAmount = (int)(_tradeFromAmount * 0.6);

                    var resourceButton = new Button
                    {
                        Content = $"{GetResourceName(resource)}\n(отримаєте: {receiveAmount})",
                        Height = 60,
                        Margin = new Thickness(0, 0, 0, 10),
                        Style = (Style)FindResource("GameButtonStyle"),
                        FontSize = 12
                    };
                    resourceButton.Click += (s, e) =>
                    {
                        _tradeToResource = resource;
                        RightPanelContent.Content = CreateTradePanelContent();
                    };
                    panel.Children.Add(resourceButton);
                }
            }
        }

        return panel;
    }

    private void ExecuteTrade(ResourceType fromResource, int fromAmount, ResourceType toResource)
    {
        if (!_resources.ContainsKey(fromResource) || _resources[fromResource] < fromAmount)
            return;

        int toAmount = (int)(fromAmount * 0.6);

        _resources[fromResource] -= fromAmount;
        if (!_resources.ContainsKey(toResource))
        {
            _resources[toResource] = 0;
        }
        _resources[toResource] += toAmount;

        UpdateResourceDisplay();

        // Reset trading state to stage 1
        _tradeStage = 1;
        _tradeFromResource = null;
        _tradeFromAmount = 0;
        _tradeToResource = null;

        if (_currentOpenPanel == "trade")
        {
            RightPanelContent.Content = CreateTradePanelContent();
        }
    }

    private void StartBuildMode(BuildingType buildingType)
    {
        _isBuildMode = true;
        _selectedBuildingType = buildingType;

        RightPanel.IsHitTestVisible = false;
        CloseRightPanel();

        if (_buildModeCanvas == null)
        {
            _buildModeCanvas = new Canvas
            {
                Width = MapCanvas.Width,
                Height = MapCanvas.Height,
                Background = Brushes.Transparent,
                IsHitTestVisible = false
            };
            _buildModeCanvas.RenderTransform = _transformGroup;
            ViewportGrid.Children.Add(_buildModeCanvas);
        }

        _buildModeCanvas.Visibility = Visibility.Visible;
        _buildModeCanvas.Children.Clear();
        _buildRadiusCircles.Clear();

        if (_map != null)
        {
            if (buildingType == BuildingType.Mine)
            {
                for (int x = 0; x < _map.Width; x++)
                {
                    for (int y = 0; y < _map.Height; y++)
                    {
                        var tile = _map.GetTile(x, y);
                        if (tile != null && tile.Type == TileType.Rock)
                        {
                            double tilePixelX = x * TileSize + tile.OffsetX;
                            double tilePixelY = y * TileSize + tile.OffsetY;

                            var radiusCircle = new Ellipse
                            {
                                Width = 200,
                                Height = 200,
                                Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 200, 100)),
                                StrokeThickness = 2,
                                Fill = new SolidColorBrush(Color.FromArgb(20, 255, 200, 100))
                            };

                            Canvas.SetLeft(radiusCircle, tilePixelX - 100);
                            Canvas.SetTop(radiusCircle, tilePixelY - 100);

                            _buildModeCanvas.Children.Add(radiusCircle);
                            _buildRadiusCircles.Add(radiusCircle);
                        }
                    }
                }
            }
            else if (buildingType == BuildingType.Sawmill)
            {
                for (int x = 0; x < _map.Width; x++)
                {
                    for (int y = 0; y < _map.Height; y++)
                    {
                        var tile = _map.GetTile(x, y);
                        if (tile != null && tile.Type == TileType.Tree)
                        {
                            double tilePixelX = x * TileSize + tile.OffsetX;
                            double tilePixelY = y * TileSize + tile.OffsetY;

                            var radiusCircle = new Ellipse
                            {
                                Width = 200,
                                Height = 200,
                                Stroke = new SolidColorBrush(Color.FromArgb(60, 139, 90, 43)),
                                StrokeThickness = 2,
                                Fill = new SolidColorBrush(Color.FromArgb(20, 139, 90, 43))
                            };

                            Canvas.SetLeft(radiusCircle, tilePixelX - 100);
                            Canvas.SetTop(radiusCircle, tilePixelY - 100);

                            _buildModeCanvas.Children.Add(radiusCircle);
                            _buildRadiusCircles.Add(radiusCircle);
                        }
                    }
                }
            }
            else
            {
                foreach (var building in _map.Buildings)
                {
                    double effectiveRadius = GetEffectiveBuildRadius(building);
                    var radiusCircle = new Ellipse
                    {
                        Width = effectiveRadius * 2,
                        Height = effectiveRadius * 2,
                        Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255))
                    };

                    Canvas.SetLeft(radiusCircle, building.Position.X - effectiveRadius);
                    Canvas.SetTop(radiusCircle, building.Position.Y - effectiveRadius);

                    _buildModeCanvas.Children.Add(radiusCircle);
                    _buildRadiusCircles.Add(radiusCircle);
                }
            }
        }

        _buildPrototype = new Ellipse
        {
            Width = GetBuildingSize(buildingType),
            Height = GetBuildingSize(buildingType),
            Fill = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255)),
            Visibility = Visibility.Collapsed
        };
        _buildModeCanvas.Children.Add(_buildPrototype);

        _buildRoadLine = new Line
        {
            Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
            StrokeThickness = 3,
            Visibility = Visibility.Collapsed
        };
        _buildModeCanvas.Children.Add(_buildRoadLine);

        _buildRoadLines.Clear();
        for (int i = 0; i < 10; i++)
        {
            var roadLine = new Line
            {
                Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
                StrokeThickness = 3,
                Visibility = Visibility.Collapsed
            };
            _buildModeCanvas.Children.Add(roadLine);
            _buildRoadLines.Add(roadLine);
        }

        _buildErrorTooltip = new TextBlock
        {
            Text = "",
            Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            Background = new SolidColorBrush(Color.FromArgb(200, 20, 20, 20)),
            Padding = new Thickness(8, 4, 8, 4),
            FontSize = 12,
            Visibility = Visibility.Collapsed
        };
        _buildModeCanvas.Children.Add(_buildErrorTooltip);
    }

    private double GetBuildingSize(BuildingType type)
    {
        return type switch
        {
            BuildingType.Base => 80,
            BuildingType.Factory => 60,
            BuildingType.Mine => 50,
            _ => 50
        };
    }

    private void ShowBuildingInfoPanel(Building building)
    {
        _selectedBuilding = building;
        _deleteConfirmationState = false;

        string buildingName = building.Type switch
        {
            BuildingType.Base => "БАЗА",
            BuildingType.Factory => "ФАБРИКА",
            BuildingType.Mine => "ШАХТА",
            BuildingType.MeatFactory => "М'ЯСОФАБРИКА",
            BuildingType.Sawmill => "ЛІСОПИЛКА",
            BuildingType.Market => "РИНОК",
            BuildingType.Marketplace => "МАРКЕТ",
            BuildingType.Furnace => "ПЕЧКА",
            _ => "БУДІВЛЯ"
        };

        _currentOpenPanel = "buildingInfo";
        RightPanelTitle.Text = buildingName;
        RightPanelContent.Content = CreateBuildingInfoPanelContent(building);
        OpenRightPanel();
    }

    private UIElement CreateBuildingInfoPanelContent(Building building)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

        string description = building.Type switch
        {
            BuildingType.Base => "Центр управління колонією",
            BuildingType.Factory => "Виробляє органіку",
            BuildingType.Mine => "Видобуває метал",
            BuildingType.MeatFactory => "Виробляє м'ясо",
            BuildingType.Sawmill => "Видобуває дерево",
            BuildingType.Market => "Інвестує ресурси для прибутку",
            BuildingType.Marketplace => "Дозволяє торгувати ресурсами",
            BuildingType.Furnace => "Переплавляє дерево у вугілля",
            _ => "Будівля"
        };

        var descriptionText = new TextBlock
        {
            Text = description,
            Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        };
        panel.Children.Add(descriptionText);

        if (building.Type != BuildingType.Base)
        {
            var levelText = new TextBlock
            {
                Text = $"Рівень: {building.Level}",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(levelText);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            if (building.CanUpgrade())
            {
                var upgradeCost = building.GetUpgradeCost();

                var upgradeContainer = new StackPanel
                {
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var upgradeButton = new Button
                {
                    Content = "АПГРЕЙД",
                    Width = 120,
                    Height = 45,
                    Style = (Style)FindResource("GameButtonStyle"),
                    FontSize = 11
                };

                bool canAfford = true;
                foreach (var cost in upgradeCost)
                {
                    if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < cost.Value)
                    {
                        canAfford = false;
                        break;
                    }
                }

                if (!canAfford)
                {
                    upgradeButton.Opacity = 0.5;
                    upgradeButton.IsEnabled = false;
                }

                upgradeButton.Click += (s, e) => UpgradeBuilding(building);
                upgradeContainer.Children.Add(upgradeButton);

                var costText = new TextBlock
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    FontSize = 9,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 120,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                var costLines = new List<string>();
                foreach (var cost in upgradeCost)
                {
                    costLines.Add($"{GetResourceName(cost.Key)}: {cost.Value}");
                }
                costText.Text = string.Join("\n", costLines);

                upgradeContainer.Children.Add(costText);
                buttonsPanel.Children.Add(upgradeContainer);
            }

            var deleteButton = new Button
            {
                Content = "ВИДАЛИТИ",
                Width = 120,
                Height = 45,
                Style = (Style)FindResource("GameButtonStyle")
            };
            deleteButton.Click += (s, e) =>
            {
                if (!_deleteConfirmationState)
                {
                    _deleteConfirmationState = true;
                    deleteButton.Content = "впевнені?";
                    deleteButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                }
                else
                {
                    DeleteBuilding(building);
                }
            };

            buttonsPanel.Children.Add(deleteButton);
            panel.Children.Add(buttonsPanel);

            var progressLabel = new TextBlock
            {
                Text = "ВИРОБНИЦТВО:",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 12,
                Margin = new Thickness(0, 10, 0, 10)
            };
            panel.Children.Add(progressLabel);

            _currentProgressBars.Clear();

            foreach (var output in building.ProductionOutput)
            {
                var progressContainer = new StackPanel
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };

                string resourceName = GetResourceName(output.Key);
                var resourceLabel = new TextBlock
                {
                    Text = $"{resourceName} +{output.Value}",
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                progressContainer.Children.Add(resourceLabel);

                var progressBar = new System.Windows.Controls.ProgressBar
                {
                    Height = 8,
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
                    Value = building.ProductionProgress * 100,
                    Maximum = 100
                };
                progressContainer.Children.Add(progressBar);
                _currentProgressBars.Add(progressBar);

                panel.Children.Add(progressContainer);
            }

            if (building.Type == BuildingType.Market)
            {
                var investmentLabel = new TextBlock
                {
                    Text = "ІНВЕСТИЦІЯ:",
                    Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                    FontSize = 12,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                panel.Children.Add(investmentLabel);

                if (building.IsInvesting)
                {
                    var investingText = new TextBlock
                    {
                        Text = $"Інвестовано: {building.InvestmentAmount} {GetResourceName(building.InvestmentResource!.Value)}",
                        Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                        FontSize = 11,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    panel.Children.Add(investingText);

                    var investmentProgressBar = new System.Windows.Controls.ProgressBar
                    {
                        Height = 8,
                        Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                        Foreground = new SolidColorBrush(Color.FromRgb(200, 180, 50)),
                        Value = building.InvestmentProgress * 100,
                        Maximum = 100,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    panel.Children.Add(investmentProgressBar);

                    double investmentDuration = building.Level == 1 ? 200.0 : 180.0;
                    double timeRemaining = investmentDuration * (1 - building.InvestmentProgress);
                    var timeText = new TextBlock
                    {
                        Text = $"Залишилось: {timeRemaining:F1}с",
                        Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                        FontSize = 10
                    };
                    panel.Children.Add(timeText);
                }
                else if (building.InvestmentCooldown > 0)
                {
                    var cooldownText = new TextBlock
                    {
                        Text = $"Кулдаун: {building.InvestmentCooldown:F1}с",
                        Foreground = new SolidColorBrush(Color.FromRgb(180, 100, 100)),
                        FontSize = 11
                    };
                    panel.Children.Add(cooldownText);
                }
                else
                {
                    var investButtonsPanel = new WrapPanel
                    {
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
                    {
                        if (_resources.ContainsKey(resourceType) && _resources[resourceType] > 100)
                        {
                            var investButton = new Button
                            {
                                Content = $"Інвестувати 100\n{GetResourceName(resourceType)}",
                                Width = 130,
                                Height = 50,
                                Margin = new Thickness(0, 0, 5, 5),
                                Style = (Style)FindResource("GameButtonStyle"),
                                FontSize = 10
                            };
                            investButton.Click += (s, e) => StartInvestment(building, resourceType, 100);
                            investButtonsPanel.Children.Add(investButton);
                        }
                    }

                    panel.Children.Add(investButtonsPanel);
                }
            }
        }

        return panel;
    }

    private void DeleteBuilding(Building building)
    {
        if (_map == null) return;

        _map.Buildings.Remove(building);

        var childrenToRemove = new List<UIElement>();
        foreach (UIElement child in MapCanvas.Children)
        {
            if (child is Ellipse ellipse)
            {
                double left = Canvas.GetLeft(ellipse);
                double top = Canvas.GetTop(ellipse);
                double centerX = left + ellipse.Width / 2;
                double centerY = top + ellipse.Height / 2;

                double distance = Math.Sqrt(
                    Math.Pow(centerX - building.Position.X, 2) +
                    Math.Pow(centerY - building.Position.Y, 2)
                );

                if (distance < 1)
                {
                    childrenToRemove.Add(child);
                }
            }
        }

        foreach (var child in childrenToRemove)
        {
            MapCanvas.Children.Remove(child);
        }

        var roadsToRemove = _permanentRoads.Where(road =>
            (Math.Abs(road.X1 - building.Position.X) < 1 && Math.Abs(road.Y1 - building.Position.Y) < 1) ||
            (Math.Abs(road.X2 - building.Position.X) < 1 && Math.Abs(road.Y2 - building.Position.Y) < 1)
        ).ToList();

        foreach (var road in roadsToRemove)
        {
            MapCanvas.Children.Remove(road);
            _permanentRoads.Remove(road);
        }

        _selectedBuilding = null;
        _deleteConfirmationState = false;
        CloseRightPanel();
    }

    private void ToggleRightPanel(string panelType, string title)
    {
        if (_currentOpenPanel == panelType)
        {
            CloseRightPanel();
        }
        else
        {
            // Clear selected building when opening menu panels
            _selectedBuilding = null;

            _currentOpenPanel = panelType;
            RightPanelTitle.Text = title;

            RightPanelContent.Content = panelType switch
            {
                "build" => CreateBuildPanelContent(),
                "research" => CreateResearchPanelContent(),
                "trade" => CreateTradePanelContent(),
                _ => null
            };

            OpenRightPanel();
        }
    }

    private void OpenRightPanel()
    {
        var animation = new DoubleAnimation
        {
            From = 300,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        RightPanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    private void CloseRightPanel()
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 300,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        animation.Completed += (s, e) =>
        {
            _currentOpenPanel = "";
            // Reset trading state when panel closes
            _tradeStage = 1;
            _tradeFromResource = null;
            _tradeFromAmount = 0;
            _tradeToResource = null;
        };
        RightPanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);
    }
}
