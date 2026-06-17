using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

namespace GDIplus.Views
{
    public partial class MainWindow : Window
    {
        private class MyBall
        {
            public required Ellipse Shape { get; init; }
            public required TextBlock Label { get; init; }
            public double Vx { get; set; }
            public double Vy { get; set; }
            public int Kills { get; set; }
        }
        private readonly List<MyBall> _balls = new();
        private readonly Random _rnd = new();
        public MainWindow()
        {
            InitializeComponent();
            var first = new MyBall { Shape = Ball, Label = BallLabel, Vx = 180, Vy = 120 };
            PositionLabel(first);
            _balls.Add(first);

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            timer.Tick += OnTick;
            timer.Start();
        }
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                SpawnBall();
        }
        private void SpawnBall()
        {
            double size = 20 + _rnd.NextDouble() * 40;
            var shape = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromRgb(
                    (byte)_rnd.Next(80, 256),
                    (byte)_rnd.Next(80, 256),
                    (byte)_rnd.Next(80, 256)
                    ))
            };
            var label = new TextBlock
            {
                Text = "0",
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                FontSize = 14,
                IsHitTestVisible = false,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            };

            double x = _rnd.NextDouble() * (Field.Width - size);
            double y = _rnd.NextDouble() * (Field.Height - size);

            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);

            Field.Children.Add(shape);
            Field.Children.Add(label);
            var ball = new MyBall
            {
                Shape = shape,
                Label = label,
                Vx = 0,
                Vy = 0
            };
            PositionLabel(ball);
            double angle = _rnd.NextDouble() * Math.PI * 2;
            double speed = 10 * _rnd.NextDouble() * 70;
            ball.Vx = Math.Cos(angle) * speed;
            ball.Vy = Math.Sin(angle) * speed;

            _balls.Add(ball);
            
            
        }
        private void OnTick(object? sender, EventArgs e)
        {
            const double dt = 0.016;

            foreach (var ball in _balls)
            {
                double x = Canvas.GetLeft(ball.Shape);
                double y = Canvas.GetTop(ball.Shape);

                x += ball.Vx * dt;
                y += ball.Vy * dt;

                double maxX = Field.Width - ball.Shape.Width;
                double maxY = Field.Height - ball.Shape.Height;

                if (x < 0) { x = 0; ball.Vx = -ball.Vx; }
                if (x > maxX) { x = maxX; ball.Vx = -ball.Vx; }
                if (y < 0) { y = 0; ball.Vy = -ball.Vy; }
                if (y > maxY) { y = maxY; ball.Vy = -ball.Vy; }

                Canvas.SetLeft(ball.Shape, x);
                Canvas.SetTop(ball.Shape, y);
                PositionLabel(ball);
            }

            HandleCollisions();
        }
        private void HandleCollisions()
        {
            var toRemove = new HashSet<MyBall>();

            for (int i = 0; i < _balls.Count; i++)
            {
                for (int j = i + 1; j < _balls.Count; j++)
                {
                    var a = _balls[i];
                    var b = _balls[j];

                    if (toRemove.Contains(a) || toRemove.Contains(b))
                        continue;
                    if (IsColliding(a, b))
                    {
                        var winner = a.Shape.Width >= b.Shape.Width ? a : b;
                        var loser = a.Shape.Width <= b.Shape.Width ? a : b;
                        Grow(winner, loser);
                        winner.Kills++;
                        winner.Label.Text = winner.Kills.ToString();
                        toRemove.Add(loser);
                    }
                }
            }
            foreach (var dead in toRemove)
            {
                Field.Children.Remove(dead.Shape);
                Field.Children.Remove(dead.Label);
                _balls.Remove(dead);
            }
        }
        private static bool IsColliding(MyBall a, MyBall b) 
        {
            double ax = Canvas.GetLeft(a.Shape) + a.Shape.Width / 2;
            double ay = Canvas.GetTop(a.Shape) + a.Shape.Height / 2;
            double bx = Canvas.GetLeft(b.Shape) + b.Shape.Width / 2;
            double by = Canvas.GetTop(b.Shape) + b.Shape.Height / 2;

            double dx = ax - bx;
            double dy = ay - by;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            double sumRadii = a.Shape.Width / 2 + b.Shape.Width / 2;

            return dist < sumRadii;
        }

        private void Grow(MyBall winner, MyBall loser) 
        {
            double areaWinner = Math.PI * Math.Pow(winner.Shape.Width / 2, 2);
            double areaLoser = Math.PI * Math.Pow(loser.Shape.Width / 2, 2);

            double newArea = areaWinner + areaLoser * 0.6;
            double newDiameter = 2 * Math.Sqrt(newArea / Math.PI);

            newDiameter = Math.Min(newDiameter, Field.Width * 0.95);

            ResizeKeepingCenter(winner.Shape, newDiameter);
        }
        private static void ResizeKeepingCenter(Ellipse shape, double newSize) 
        {
            double cx = Canvas.GetLeft(shape) + shape.Width / 2;
            double cy = Canvas.GetTop(shape) + shape.Height / 2;

            shape.Width = newSize;
            shape.Height = newSize;

            Canvas.SetLeft(shape, cx - newSize / 2);
            Canvas.SetTop(shape, cy - newSize / 2);
        }
        private static void PositionLabel(MyBall ball) 
        {
            double cx = Canvas.GetLeft(ball.Shape) + ball.Shape.Width / 2;
            double cy = Canvas.GetTop(ball.Shape) + ball.Shape.Height / 2;

            ball.Label.Measure(Size.Infinity);
            var desired = ball.Label.DesiredSize;

            Canvas.SetLeft(ball.Label, cx - desired.Width / 2);
            Canvas.SetTop(ball.Label, cy - desired.Height / 2);
        }
    }
}