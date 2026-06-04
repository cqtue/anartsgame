using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace anartsgame.helpers;

public static class VisualEffects
{
    public static Canvas CreateBuildingGlow(double size, Color baseColor, bool animated = true)
    {
        var glowCanvas = new Canvas
        {
            Width = size * 2,
            Height = size * 2
        };

        var outerGlow = new Ellipse
        {
            Width = size * 1.8,
            Height = size * 1.8,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, baseColor.R, baseColor.G, baseColor.B), 0.0),
                    new GradientStop(Color.FromArgb(20, baseColor.R, baseColor.G, baseColor.B), 0.5),
                    new GradientStop(Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B), 1.0)
                }
            },
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        Canvas.SetLeft(outerGlow, size * 0.1);
        Canvas.SetTop(outerGlow, size * 0.1);

        var innerGlow = new Ellipse
        {
            Width = size * 1.4,
            Height = size * 1.4,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, baseColor.R, baseColor.G, baseColor.B), 0.0),
                    new GradientStop(Color.FromArgb(30, baseColor.R, baseColor.G, baseColor.B), 0.6),
                    new GradientStop(Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B), 1.0)
                }
            },
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        Canvas.SetLeft(innerGlow, size * 0.3);
        Canvas.SetTop(innerGlow, size * 0.3);

        glowCanvas.Children.Add(outerGlow);
        glowCanvas.Children.Add(innerGlow);

        if (animated)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.6,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            outerGlow.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);

            var innerPulseAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            innerGlow.BeginAnimation(UIElement.OpacityProperty, innerPulseAnimation);
        }

        return glowCanvas;
    }

    public static void AddBreathingAnimation(UIElement element, double duration = 3.0)
    {
        var scaleTransform = new ScaleTransform(1.0, 1.0);
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromSeconds(duration),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    public static Canvas CreateProductionParticles(double centerX, double centerY, Color particleColor, int particleCount = 8)
    {
        var particlesCanvas = new Canvas
        {
            Width = 200,
            Height = 200,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(particlesCanvas, centerX - 100);
        Canvas.SetTop(particlesCanvas, centerY - 100);

        var random = new Random();

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Color.FromArgb(150, particleColor.R, particleColor.G, particleColor.B)),
                Opacity = 0
            };

            var angle = (360.0 / particleCount) * i + random.Next(-15, 15);
            var distance = 40 + random.Next(0, 20);

            var endX = 100 + Math.Cos(angle * Math.PI / 180) * distance;
            var endY = 100 + Math.Sin(angle * Math.PI / 180) * distance;

            Canvas.SetLeft(particle, 100);
            Canvas.SetTop(particle, 100);

            particlesCanvas.Children.Add(particle);

            var storyboard = new Storyboard
            {
                Duration = TimeSpan.FromSeconds(1.5),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var xAnimation = new DoubleAnimation
            {
                From = 100,
                To = endX,
                Duration = TimeSpan.FromSeconds(1.5),
                BeginTime = TimeSpan.FromSeconds(i * 0.1)
            };
            Storyboard.SetTarget(xAnimation, particle);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(Canvas.Left)"));

            var yAnimation = new DoubleAnimation
            {
                From = 100,
                To = endY,
                Duration = TimeSpan.FromSeconds(1.5),
                BeginTime = TimeSpan.FromSeconds(i * 0.1)
            };
            Storyboard.SetTarget(yAnimation, particle);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(Canvas.Top)"));

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = true,
                BeginTime = TimeSpan.FromSeconds(i * 0.1)
            };
            Storyboard.SetTarget(opacityAnimation, particle);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            storyboard.Children.Add(xAnimation);
            storyboard.Children.Add(yAnimation);
            storyboard.Children.Add(opacityAnimation);

            storyboard.Begin();
        }

        return particlesCanvas;
    }

    public static Canvas CreateBackgroundGrid(double width, double height, double spacing = 64)
    {
        var gridCanvas = new Canvas
        {
            Width = width,
            Height = height,
            IsHitTestVisible = false,
            Opacity = 0.03
        };

        for (double x = 0; x < width; x += spacing)
        {
            var line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = height,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                StrokeThickness = 0.5
            };
            gridCanvas.Children.Add(line);
        }

        for (double y = 0; y < height; y += spacing)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = width,
                Y2 = y,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                StrokeThickness = 0.5
            };
            gridCanvas.Children.Add(line);
        }

        return gridCanvas;
    }

    public static Canvas CreateAmbientParticles(double width, double height, int particleCount = 20)
    {
        var particlesCanvas = new Canvas
        {
            Width = width,
            Height = height,
            IsHitTestVisible = false,
            Opacity = 0.15
        };

        var random = new Random();

        for (int i = 0; i < particleCount; i++)
        {
            var size = random.Next(1, 4);
            var particle = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            };

            var startX = random.NextDouble() * width;
            var startY = random.NextDouble() * height;
            var endX = startX + random.Next(-100, 100);
            var endY = startY + random.Next(-100, 100);

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);

            particlesCanvas.Children.Add(particle);

            var duration = 10 + random.Next(0, 10);

            var xAnimation = new DoubleAnimation
            {
                From = startX,
                To = endX,
                Duration = TimeSpan.FromSeconds(duration),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var yAnimation = new DoubleAnimation
            {
                From = startY,
                To = endY,
                Duration = TimeSpan.FromSeconds(duration),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.2,
                To = 0.8,
                Duration = TimeSpan.FromSeconds(duration / 2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            particle.BeginAnimation(Canvas.LeftProperty, xAnimation);
            particle.BeginAnimation(Canvas.TopProperty, yAnimation);
            particle.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        return particlesCanvas;
    }

    public static Canvas CreateScanLines(double width, double height, double lineSpacing = 4)
    {
        var scanLinesCanvas = new Canvas
        {
            Width = width,
            Height = height,
            IsHitTestVisible = false,
            Opacity = 0.02
        };

        for (double y = 0; y < height; y += lineSpacing)
        {
            var line = new Rectangle
            {
                Width = width,
                Height = 1,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255))
            };
            Canvas.SetTop(line, y);
            scanLinesCanvas.Children.Add(line);
        }

        return scanLinesCanvas;
    }
}
