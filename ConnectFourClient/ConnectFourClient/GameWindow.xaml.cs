using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ConnectFourClient
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    /// 
    public partial class GameWindow : Window
    {
        public ConnectFourServiceClient client { get; set; }
        public ClientCallback Callback { get; set; }
        public string currentUser { get; set; }
        public enum Side { None, Red, Black };


        const int circleSize = 80;
        private Side[,] GameBoard;
        // private ConnectFourBoard board;
        private DispatcherTimer animationTimer;
        private bool inputLock;

        private Side currentSide;
        private Ellipse currentCircle;
        private int currentColumn;

        public GameWindow(Side playerColor)
        {
            InitializeComponent();
            NewGame(playerColor);

        }

        private void NewGame(Side playerColor)
        {
            inputLock = true;
            GameBoard = new Side[6, 7];
            for (int row = 0; row < this.GameBoard.GetLength(0); row++)
                for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                    this.GameBoard[row, col] = Side.None;

            currentSide = playerColor;
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 15);
            animationTimer.Start();
            GameCanvas.Children.Clear();
            DrawBackground();
            inputLock = false;
            EnableAllInsertButtons();
        }

        private void InsertButton_Click(int column)
        {
            if (inputLock == false)
            {

                /*
                 * Check if that column is full in client, if yes show proper message
                 * 
                 * Send Insert to server with column and user color
                 * If its not my turn, server must return exception
                 * Server must return x and y if success
                 * add the X and Y to this board
                 * 
                 * 
                 * 
                 */
                if (!client.IsMyTurn(currentUser))
                {
                    MessageBox.Show("its not your turn!!");
                    return;
                }
                if (GameBoard[0, column] != Side.None)
                {
                    MessageBox.Show("Column is full, therefore you cant enter circle on it");
                    return;
                }
                Thread t = new Thread(() => insertCellThread(column, currentUser, currentSide));
                t.Start();
            }
        }

        public void insertCellThread(int column, string currentUser, Side currentSide)
        {
            try
            {
                InsertResult insertResult = client.Insert(column, currentUser);
                GameBoard[insertResult.Row_index, column] = currentSide;
                currentColumn = column;
                Application.Current.Dispatcher.Invoke(new Action(() => { DrawCircle(currentSide, column); }));

                if (insertResult.Move_result == MOVE_RESULT.Win)
                {
                    MessageBox.Show("Congrats, you won");

                }
                else if (insertResult.Move_result == MOVE_RESULT.Draw)
                {
                    MessageBox.Show("Game ended with Draw");
                }

            } catch (FaultException<UserNotFoundFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
                return;
            }
        }

        #region InsertButton_Click Methods
        private void InsertButton0_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(0);
        }

        private void InsertButton1_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(1);
        }

        private void InsertButton2_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(2);
        }

        private void InsertButton3_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(3);
        }

        private void InsertButton4_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(4);
        }

        private void InsertButton5_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(5);
        }

        private void InsertButton6_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(6);
        }
        #endregion

        private void DrawBackground()
        {
            for (int row = 0; row < GameBoard.GetLength(0); row++)
            {
                for (int column = 0; column < GameBoard.GetLength(1); column++)
                {
                    Rectangle square = new Rectangle();
                    square.Height = circleSize;
                    square.Width = circleSize;
                    square.Fill = (column % 2 == 0) ? Brushes.White : Brushes.LightGray;
                    Canvas.SetBottom(square, circleSize * row);
                    Canvas.SetRight(square, circleSize * column);
                    GameCanvas.Children.Add(square);
                }
            }
        }

        private void DrawCircle(Side side, int col)
        {
            inputLock = true;

            Ellipse circle = new Ellipse();
            circle.Height = circleSize;
            circle.Width = circleSize;
            circle.Fill = (side == Side.Red) ? Brushes.Red : Brushes.Black;
            Canvas.SetTop(circle, 0);
            Canvas.SetLeft(circle, col * 80);
            GameCanvas.Children.Add(circle);
            currentCircle = circle;
            animationTimer.Tick += DropCircleAnimation;
        }

        private void DropCircleAnimation(object sender, EventArgs e)
        {
            int dropLength = circleSize * (GameBoard.GetLength(1) - 1 - PiecesInCol(currentColumn));
            int dropRate = 40;
            if (Canvas.GetTop(currentCircle) < dropLength)
            {
                Canvas.SetTop(currentCircle, Canvas.GetTop(currentCircle) + dropRate);
            }
            else
            {
                animationTimer.Tick -= DropCircleAnimation;
                inputLock = false;
            }
        }

        public int PiecesInCol(int column)
        {
            int numOfPieces = 0;
            for (int row = GameBoard.GetLength(0) - 1; row >= 0; row--)
            {
                if (GameBoard[row, column] != Side.None)
                {
                    numOfPieces++;
                }
            }
            return numOfPieces;
        }

        private void DisableAllInsertButtons()
        {
            InsertButton0.IsEnabled = false;
            InsertButton1.IsEnabled = false;
            InsertButton2.IsEnabled = false;
            InsertButton3.IsEnabled = false;
            InsertButton4.IsEnabled = false;
            InsertButton5.IsEnabled = false;
            InsertButton6.IsEnabled = false;
        }

        private void EnableAllInsertButtons()
        {
            InsertButton0.IsEnabled = true;
            InsertButton1.IsEnabled = true;
            InsertButton2.IsEnabled = true;
            InsertButton3.IsEnabled = true;
            InsertButton4.IsEnabled = true;
            InsertButton5.IsEnabled = true;
            InsertButton6.IsEnabled = true;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame(currentSide);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Playing as player: " + currentUser;
            Callback.updateCellFunc += updateCell;
        }


        //Insert result from player 1 callback
        private void updateCell(int row, int col, MOVE_RESULT result)
        {
            //paint with th other color
            Side addColor;
            if(currentSide == Side.Red)
            {
                addColor = Side.Black;
            } else
            {
                addColor = Side.Red;
            }
            GameBoard[row, col] = addColor;
            currentColumn = col;
            Application.Current.Dispatcher.Invoke(new Action(() => { DrawCircle(addColor, col); }));
            if(result == MOVE_RESULT.Win)
            {
                MessageBox.Show("Oops, looks like you lost :(");
            } else if(result == MOVE_RESULT.Draw)
            {
                MessageBox.Show("Game ended with draw");
            }
        }
    }
}
