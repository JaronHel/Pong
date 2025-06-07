using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Pong
{
    public partial class MainWindow : Window
    {
        Random random = new Random();

        private bool wPressed = false;
        private bool sPressed = false;
        private bool upPressed = false;
        private bool downPressed = false;

        private bool gameRunningNormal = false;
        private bool gameRunningTraining = false;

        private readonly double paddleSpeed = 4.5;
        private readonly double ballSpeed = 4.5;

        private Vector ballDirection;

        private Point leftPaddlePosition;
        private Point rightPaddlePosition;
        private Point ballPosition;

        private string defaultBackgroundColor = "#222222";

        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (gameRunningNormal || gameRunningTraining)
            {
                GoStartPosition();
            } else
            {
                resizeUI();
            }
        }

        private async void StartNormal(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Hidden;
            PlayfieldCanvas.Visibility = Visibility.Visible;
            Counter.Visibility = Visibility.Visible;

            GoStartPosition();

            await StartCounter();

            gameRunningNormal = true;

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            CompositionTarget.Rendering += OnRendering;

            this.Focusable = true;
            this.Focus();
            Keyboard.Focus(this);
        }

        private async void StartTraining(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Hidden;
            PlayfieldCanvas.Visibility = Visibility.Visible;
            Counter.Visibility = Visibility.Visible;

            GoStartPosition();

            await StartCounter();

            gameRunningTraining = true;

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            CompositionTarget.Rendering += OnRendering;

            this.Focusable = true;
            this.Focus();
            Keyboard.Focus(this);
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Hidden;
            SettingsPanel.Visibility = Visibility.Visible;
        }

        private void GoToMenu(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Visible;
            SettingsPanel.Visibility = Visibility.Hidden;

            PlayfieldCanvas.Background = new SolidColorBrush(BackgroundColorPicker.SelectedColor ?? (Color)ColorConverter.ConvertFromString(defaultBackgroundColor));

            LeftPaddle.Fill = new SolidColorBrush(PaddleColorPicker.SelectedColor ?? Colors.White);
            RightPaddle.Fill = new SolidColorBrush(PaddleColorPicker.SelectedColor ?? Colors.White);

            Ball.Fill = new SolidColorBrush(PaddleColorPicker.SelectedColor ?? Colors.Red);
        }

        private void GoStartPosition()
        {
            leftPaddlePosition = new Point(
                0,
                (PlayfieldCanvas.ActualHeight - LeftPaddle.ActualHeight) / 2);
            Canvas.SetLeft(LeftPaddle, leftPaddlePosition.X);
            Canvas.SetTop(LeftPaddle, leftPaddlePosition.Y);

            rightPaddlePosition = new Point(
                (PlayfieldCanvas.ActualWidth - RightPaddle.ActualWidth),
                (PlayfieldCanvas.ActualHeight - RightPaddle.ActualHeight) / 2);
            Canvas.SetLeft(RightPaddle, rightPaddlePosition.X);
            Canvas.SetTop(RightPaddle, rightPaddlePosition.Y);

            ballPosition = new Point(
                (PlayfieldCanvas.ActualWidth - Ball.ActualWidth) / 2,
                (PlayfieldCanvas.ActualHeight - Ball.ActualHeight) / 2);
            Canvas.SetLeft(Ball, ballPosition.X);
            Canvas.SetTop(Ball, ballPosition.Y);

            double angle = random.Next(20, 40);
            if (random.Next(2) == 0) angle += 180;
            ballDirection = new Vector(Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180));
            ballDirection.Normalize();
        }

        private void resizeUI()
        {
            MenuPanel.Width = PlayfieldCanvas.ActualWidth / 5;
            SettingsPanel.Width = PlayfieldCanvas.ActualWidth / 5;
        }

        private async Task StartCounter()
        {
            for (int i = 3; i > 0; i--) 
            {
                Counter.Content = i;
                await Task.Delay(1000);
            }
            Counter.Visibility = Visibility.Hidden;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W && !gameRunningTraining)
            {
                wPressed = true;
            }
            else if (e.Key == Key.S && !gameRunningTraining)
            {
                sPressed = true;
            }
            else if (e.Key == Key.Up)
            {
                upPressed = true;
            }
            else if (e.Key == Key.Down)
            {
                downPressed = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W && !gameRunningTraining)
            {
                wPressed = false;
            }
            else if (e.Key == Key.S && !gameRunningTraining)
            {
                sPressed = false;
            }
            else if (e.Key == Key.Up)
            {
                upPressed = false;
            }
            else if (e.Key == Key.Down)
            {
                downPressed = false;
            }
        }

        private void OnRendering(object sender, System.EventArgs e)
        {
            if (gameRunningTraining && ballPosition.X <= (PlayfieldCanvas.ActualWidth / 4))
            {
                double currentTop = Canvas.GetTop(LeftPaddle);
                double targetTop = ballPosition.Y - (LeftPaddle.ActualHeight / 2) + (Ball.ActualHeight / 2);

                double delta = targetTop - currentTop;

                if (Math.Abs(delta) > paddleSpeed)
                    delta = paddleSpeed * Math.Sign(delta);

                double newTop = currentTop + delta;

                if (newTop < 0) newTop = 0;
                if (newTop > PlayfieldCanvas.ActualHeight - ((FrameworkElement)LeftPaddle).ActualHeight)
                    newTop = PlayfieldCanvas.ActualHeight - ((FrameworkElement)LeftPaddle).ActualHeight;

                Canvas.SetTop(LeftPaddle, newTop);
            }

            if (wPressed)
                MovePaddle(LeftPaddle, -paddleSpeed);

            if (sPressed)
                MovePaddle(LeftPaddle, paddleSpeed);

            if (upPressed)
                MovePaddle(RightPaddle, -paddleSpeed);

            if (downPressed)
                MovePaddle(RightPaddle, paddleSpeed);

            MoveBall();
        }

        private void MovePaddle(UIElement paddle, double deltaY)
        {
            double currentTop = Canvas.GetTop(paddle);
            double newTop = currentTop + deltaY;

            if (newTop < 0) newTop = 0;
            if (newTop > PlayfieldCanvas.ActualHeight - ((FrameworkElement)paddle).ActualHeight)
                newTop = PlayfieldCanvas.ActualHeight - ((FrameworkElement)paddle).ActualHeight;

            Canvas.SetTop(paddle, newTop);
        }

        private async void MoveBall()
        {
            if (ballPosition.Y <= 1)
            {
                ballPosition.Y = 1;
                ballDirection.Y = Math.Abs(ballDirection.Y);
            }
            else if (ballPosition.Y >= PlayfieldCanvas.ActualHeight - Ball.ActualHeight - 1)
            {
                ballPosition.Y = PlayfieldCanvas.ActualHeight - Ball.ActualHeight - 1;
                ballDirection.Y = -Math.Abs(ballDirection.Y);
            }

            else if (ballPosition.X < Canvas.GetLeft(LeftPaddle) + LeftPaddle.ActualWidth - (Ball.ActualWidth / 2) ||
                ballPosition.X > Canvas.GetLeft(RightPaddle) - (Ball.ActualWidth / 2))
            {
                gameRunningNormal = false;
                gameRunningTraining = false;

                CompositionTarget.Rendering -= OnRendering;
                EndScreen.Visibility = Visibility.Visible;
                await Task.Delay(2000);

                EndScreen.Visibility = Visibility.Hidden;
                PlayfieldCanvas.Visibility = Visibility.Hidden;
                MenuPanel.Visibility = Visibility.Visible;
                return;
            }

            Rect ballRect = new Rect(ballPosition.X, ballPosition.Y, Ball.ActualWidth, Ball.ActualHeight);
            Rect leftPaddleRect = new Rect(Canvas.GetLeft(LeftPaddle), Canvas.GetTop(LeftPaddle), LeftPaddle.ActualWidth, LeftPaddle.ActualHeight);
            Rect rightPaddleRect = new Rect(Canvas.GetLeft(RightPaddle), Canvas.GetTop(RightPaddle), RightPaddle.ActualWidth, RightPaddle.ActualHeight);
            if (ballRect.IntersectsWith(leftPaddleRect))
            {
                double paddleCenter = Canvas.GetTop(LeftPaddle) + LeftPaddle.ActualHeight / 2;
                double ballCenter = ballPosition.Y + Ball.ActualHeight / 2;
                double relativeIntersectY = (paddleCenter - ballCenter);

                double normalizedRelativeIntersectY = relativeIntersectY / (LeftPaddle.ActualHeight / 2);
                double bounceAngle = normalizedRelativeIntersectY * 60;

                ballDirection.X = Math.Cos(bounceAngle * Math.PI / 180);
                ballDirection.Y = -Math.Sin(bounceAngle * Math.PI / 180);
            }
            if (ballRect.IntersectsWith(rightPaddleRect))
            {
                double paddleCenter = Canvas.GetTop(RightPaddle) + RightPaddle.ActualHeight / 2;
                double ballCenter = ballPosition.Y + Ball.ActualHeight / 2;
                double relativeIntersectY = (paddleCenter - ballCenter);

                double normalizedRelativeIntersectY = relativeIntersectY / (RightPaddle.ActualHeight / 2);
                double bounceAngle = normalizedRelativeIntersectY * 50;

                ballDirection.X = -Math.Cos(bounceAngle * Math.PI / 180);
                ballDirection.Y = -Math.Sin(bounceAngle * Math.PI / 180);
            }

            ballPosition += ballDirection * ballSpeed;

            Canvas.SetLeft(Ball, ballPosition.X);
            Canvas.SetTop(Ball, ballPosition.Y);
        }
    }
}
