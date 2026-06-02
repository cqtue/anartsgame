using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace anartsgame.views;

public partial class AnimatedBackgroundControl : UserControl
{
    private readonly List<Particle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new();
    private const int MaxParticles = 50;
    private const double ParticleSpeed = 0.3;
    private const double ConnectionDistance = 150;
    private const double ParticleSize = 2;

    public AnimatedBackgroundControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTick;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeParticles();
        _animationTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
    }

    private void InitializeParticles()
    {
        _particles.Clear();
        ParticleCanvas.Children.Clear();

        var width = ActualWidth > 0 ? ActualWidth : 800;
        var height = ActualHeight > 0 ? ActualHeight : 600;

        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = new Particle
            {
                X = _random.NextDouble() * width,
                Y = _random.NextDouble() * height,
                VelocityX = (_random.NextDouble() - 0.5) * ParticleSpeed,
                VelocityY = (_random.NextDouble() - 0.5) * ParticleSpeed,
                Opacity = 0.3 + _random.NextDouble() * 0.4
            };

            var ellipse = new Ellipse
            {
                Width = ParticleSize,
                Height = ParticleSize,
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)(particle.Opacity * 255),
                    0x00, 0xff, 0x00))
            };

            particle.Visual = ellipse;
            Canvas.SetLeft(ellipse, particle.X);
            Canvas.SetTop(ellipse, particle.Y);
            ParticleCanvas.Children.Add(ellipse);

            _particles.Add(particle);
        }
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        foreach (var particle in _particles)
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;

            if (particle.X < 0) particle.X = ActualWidth;
            if (particle.X > ActualWidth) particle.X = 0;
            if (particle.Y < 0) particle.Y = ActualHeight;
            if (particle.Y > ActualHeight) particle.Y = 0;

            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
        }

        DrawConnections();
    }

    private void DrawConnections()
    {
        var linesToRemove = ParticleCanvas.Children
            .OfType<Line>()
            .ToList();

        foreach (var line in linesToRemove)
        {
            ParticleCanvas.Children.Remove(line);
        }

        for (int i = 0; i < _particles.Count; i++)
        {
            for (int j = i + 1; j < _particles.Count; j++)
            {
                var p1 = _particles[i];
                var p2 = _particles[j];

                var distance = Math.Sqrt(
                    Math.Pow(p2.X - p1.X, 2) +
                    Math.Pow(p2.Y - p1.Y, 2));

                if (distance < ConnectionDistance)
                {
                    var opacity = (1 - distance / ConnectionDistance) * 0.15;
                    var line = new Line
                    {
                        X1 = p1.X + ParticleSize / 2,
                        Y1 = p1.Y + ParticleSize / 2,
                        X2 = p2.X + ParticleSize / 2,
                        Y2 = p2.Y + ParticleSize / 2,
                        Stroke = new SolidColorBrush(Color.FromArgb(
                            (byte)(opacity * 255),
                            0x00, 0xff, 0x00)),
                        StrokeThickness = 1
                    };

                    ParticleCanvas.Children.Insert(0, line);
                }
            }
        }
    }

    private class Particle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Opacity { get; set; }
        public Ellipse Visual { get; set; } = null!;
    }
}
