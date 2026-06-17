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
            public double Vx { get; set; }
            public double Vy { get; set; }
        }
        private readonly List<MyBall> _balls = new();
        private readonly Random _rnd = new();
        public MainWindow()
        {
            InitializeComponent();
            _balls.Add(new MyBall { Shape = Ball, Vx = 180, Vy = 120 });

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
                    (byte)_rnd.Next(80,256),
                    (byte)_rnd.Next(80, 256),
                    (byte)_rnd.Next(80, 256)
                    ))
            };

            double x = _rnd.NextDouble() * (Field.Width - size);
            double y = _rnd.NextDouble() * (Field.Height - size);

            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);

            Field.Children.Add(shape);

            double angle = _rnd.NextDouble() * Math.PI * 2;
            double speed = 10 * _rnd.NextDouble() * 70;

            _balls.Add(new MyBall
            {
                Shape = shape,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed
            });
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
            }
        }
    }
}