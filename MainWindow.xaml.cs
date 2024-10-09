using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private readonly int rows = 30, cols = 30;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver1 || gameState.GameOver2)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.W:
                    gameState.ChangeDirection2(Direction.Up);
                    break;
                case Key.S:
                    gameState.ChangeDirection2(Direction.Down);
                    break;
                case Key.A:
                    gameState.ChangeDirection2(Direction.Left);
                    break;
                case Key.D:
                    gameState.ChangeDirection2(Direction.Right);
                    break;
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver1 && !gameState.GameOver2)
            {
                await Task.Delay(100);
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"Score {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            DrawSnakeHead(gameState.SnakePositions());
            DrawSnakeHead(gameState.SnakePositions2());
        }

        private void DrawSnakeHead(IEnumerable<Position> snakePositions)
        {
            var positions = snakePositions.ToList();
            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                Image image = gridImages[pos.Row, pos.Col];

                if (i == 0)
                {
                    image.Source = Images.Head;
                }
                else
                {
                    image.Source = Images.Body;
                }

                if (i == 0)
                {
                    int rotation = dirToRotation[gameState.Dir];
                    image.RenderTransform = new RotateTransform(rotation);
                }
            }
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions1 = new List<Position>(gameState.SnakePositions());
            List<Position> positions2 = new List<Position>(gameState.SnakePositions2());

            // Handle snake 1 (losing snake if game over for snake 2)
            for (int i = 0; i < positions1.Count; i++)
            {
                Position pos = positions1[i];
                ImageSource source = (gameState.GameOver2) ? 
                    (i == 0 ? Images.DeadHead : Images.DeadBody) 
                    : (i == 0 ? Images.Head : Images.Body);
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }

            // Handle snake 2 (losing snake if game over for snake 1)
            for (int i = 0; i < positions2.Count; i++)
            {
                Position pos = positions2[i];
                ImageSource source = (gameState.GameOver1) ? 
                    (i == 0 ? Images.DeadHead : Images.DeadBody) 
                    : (i == 0 ? Images.Head : Images.Body);
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            if (gameState.GameOver1)
            {
                OverlayText.Text = "Snake 2 wins! PRESS ANY KEY TO START";
            }
            else
            {
                OverlayText.Text = "Snake 1 wins! PRESS ANY KEY TO START";
            }
        }
    }
}
