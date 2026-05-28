using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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

    public GameView()
    {
        InitializeComponent();

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

        Loaded += OnLoaded;
        MouseWheel += OnMouseWheel;
        MouseDown += OnMouseDown;
        MouseUp += OnMouseUp;
        MouseMove += OnMouseMove;
        KeyDown += OnKeyDown;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        GenerateMap();
        RenderMap();
        CenterCamera();
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
                        withinBuildingRadius = distanceToBuilding <= nearestBuilding.BuildRadius;
                    }

                    if (_selectedBuildingType == BuildingType.Mine)
                    {
                        var nearestResourcePoint = FindNearestResourcePoint(mapPos);
                        bool nearResource = false;

                        if (nearestResourcePoint != null)
                        {
                            double tilePixelX = nearestResourcePoint.X * TileSize + nearestResourcePoint.OffsetX;
                            double tilePixelY = nearestResourcePoint.Y * TileSize + nearestResourcePoint.OffsetY;

                            double distanceToResource = Math.Sqrt(
                                Math.Pow(mapPos.X - tilePixelX, 2) +
                                Math.Pow(mapPos.Y - tilePixelY, 2)
                            );

                            nearResource = distanceToResource <= 100;
                        }

                        canPlace = withinBuildingRadius && nearResource && !hasOverlap;
                    }
                    else
                    {
                        canPlace = withinBuildingRadius && !hasOverlap;
                    }

                    if (canPlace)
                    {
                        _map?.Buildings.Add(newBuilding);

                        RenderBuilding(newBuilding);

                        var radiusCircle = new Ellipse
                        {
                            Width = newBuilding.BuildRadius * 2,
                            Height = newBuilding.BuildRadius * 2,
                            Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                            StrokeThickness = 2,
                            Fill = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255))
                        };

                        Canvas.SetLeft(radiusCircle, newBuilding.Position.X - newBuilding.BuildRadius);
                        Canvas.SetTop(radiusCircle, newBuilding.Position.Y - newBuilding.BuildRadius);

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
        if (e.Key == Key.Escape && _isBuildMode)
        {
            ExitBuildMode();
            e.Handled = true;
        }
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
                withinBuildingRadius = distanceToBuilding <= nearestBuilding.BuildRadius;
            }
            else
            {
                errorMessage = "Занадто далеко від будівель";
            }

            bool isValid = false;

            if (_selectedBuildingType == BuildingType.Mine)
            {
                var nearestResourcePoint = FindNearestResourcePoint(mapPos);
                bool nearResource = false;

                if (nearestResourcePoint != null)
                {
                    double tilePixelX = nearestResourcePoint.X * TileSize + nearestResourcePoint.OffsetX;
                    double tilePixelY = nearestResourcePoint.Y * TileSize + nearestResourcePoint.OffsetY;

                    double distanceToResource = Math.Sqrt(
                        Math.Pow(mapPos.X - tilePixelX, 2) +
                        Math.Pow(mapPos.Y - tilePixelY, 2)
                    );

                    nearResource = distanceToResource <= 100;
                }

                if (!withinBuildingRadius && !nearResource)
                {
                    errorMessage = "Шахта: занадто далеко від будівель та ресурсів";
                }
                else if (!withinBuildingRadius)
                {
                    errorMessage = "Шахта: занадто далеко від будівель";
                }
                else if (!nearResource)
                {
                    errorMessage = "Шахта: має бути біля ресурсів";
                }
                else if (hasOverlap)
                {
                    errorMessage = "Перетинається з іншою будівлею";
                }

                isValid = withinBuildingRadius && nearResource && !hasOverlap;
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

    private Tile? FindNearestResourcePoint(Point position)
    {
        if (_map == null) return null;

        Tile? nearest = null;
        double minDistance = double.MaxValue;

        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                var tile = _map.GetTile(x, y);
                if (tile != null && tile.Type == TileType.ResourcePoint)
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
            _elapsedTime += 0.1 * _gameSpeed;
            int totalSeconds = (int)_elapsedTime;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            TimerText.Text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    private void SpeedButton_Click(object sender, RoutedEventArgs e)
    {
        _gameSpeed = (_gameSpeed + 1) % 4;
        SpeedButton.Content = _gameSpeed switch
        {
            0 => "⏸",
            1 => "▶",
            2 => "▶▶",
            3 => "▶▶▶",
            _ => "▶"
        };
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

    private UIElement CreateBuildPanelContent()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

        var factoryButton = new Button
        {
            Content = "ФАБРИКА",
            Height = 50,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        factoryButton.Click += (s, e) => StartBuildMode(BuildingType.Factory);

        var mineButton = new Button
        {
            Content = "ШАХТА",
            Height = 50,
            Margin = new Thickness(0, 0, 0, 10),
            Style = (Style)FindResource("GameButtonStyle")
        };
        mineButton.Click += (s, e) => StartBuildMode(BuildingType.Mine);

        panel.Children.Add(factoryButton);
        panel.Children.Add(mineButton);

        return panel;
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
                        if (tile != null && tile.Type == TileType.ResourcePoint)
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
            else
            {
                foreach (var building in _map.Buildings)
                {
                    var radiusCircle = new Ellipse
                    {
                        Width = building.BuildRadius * 2,
                        Height = building.BuildRadius * 2,
                        Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255))
                    };

                    Canvas.SetLeft(radiusCircle, building.Position.X - building.BuildRadius);
                    Canvas.SetTop(radiusCircle, building.Position.Y - building.BuildRadius);

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

    private void ToggleRightPanel(string panelType, string title)
    {
        if (_currentOpenPanel == panelType)
        {
            CloseRightPanel();
        }
        else
        {
            _currentOpenPanel = panelType;
            RightPanelTitle.Text = title;

            RightPanelContent.Content = panelType switch
            {
                "build" => CreateBuildPanelContent(),
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
        animation.Completed += (s, e) => _currentOpenPanel = "";
        RightPanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);
    }
}
