using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourServer
{
    public enum Side { None, Red, Black };

    public class ConnectFourBoard
    {
        public Side[,] GameBoard { get; private set; }

        public ConnectFourBoard(int rows, int cols)
        {
            // Instantiate an empty board
            GameBoard = new Side[rows, cols];
            for (int row = 0; row < this.GameBoard.GetLength(0); row++)
                for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                    this.GameBoard[row, col] = Side.None;
        }

        public bool Tied()
        {
            for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                if (GameBoard[0, col] == Side.None)
                    return false;
            return true;
        }

        public Side Winner()
        {
            for (int row = 0; row < this.GameBoard.GetLength(0); row++)
            {
                for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                {
                    if (GameBoard[row, col] != Side.None &&
                        (VerticalConnectFour(row, col) || HorizontalConnectFour(row, col) || ForwardDiagonalConnectFour(row, col) || BackwardDiagonalConnectFour(row, col)))
                        return GameBoard[row, col];
                }
            }
            return Side.None;
        }

        public Side Winner(int row, int col)
        {
            if (GameBoard[row, col] != Side.None &&
                (VerticalConnectFour(row, col) ||
                HorizontalConnectFour(row, col) ||
                ForwardDiagonalConnectFour(row, col) ||
                BackwardDiagonalConnectFour(row, col)))
                return GameBoard[row, col];
            else
                return Side.None;
        }

        #region Winner() helpers

        private bool VerticalConnectFour(int row, int col)
        {
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int rowCursor = row - 1;
            while (rowCursor >= 0 && GameBoard[rowCursor, col] == GameBoard[row, col])
            {
                count++;
                rowCursor--;
            }
            rowCursor = row + 1;
            while (rowCursor < GameBoard.GetLength(0) && GameBoard[rowCursor, col] == GameBoard[row, col])
            {
                count++;
                rowCursor++;
            }
            if (count < 4)
                return false;
            return true;
        }

        private bool HorizontalConnectFour(int row, int col)
        {
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int colCursor = col - 1;
            while (colCursor >= 0 && GameBoard[row, colCursor] == GameBoard[row, col])
            {
                count++;
                colCursor--;
            }
            colCursor = col + 1;
            while (colCursor < GameBoard.GetLength(1) && GameBoard[row, colCursor] == GameBoard[row, col])
            {
                count++;
                colCursor++;
            }
            if (count < 4)
                return false;
            return true;
        }

        private bool ForwardDiagonalConnectFour(int row, int col)
        {
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int rowCursor = row - 1;
            int colCursor = col + 1;
            while (rowCursor >= 0 && colCursor < GameBoard.GetLength(1) && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor--;
                colCursor++;
            }
            rowCursor = row + 1;
            colCursor = col - 1;
            while (rowCursor < GameBoard.GetLength(0) && colCursor >= 0 && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor++;
                colCursor--;
            }
            if (count < 4)
                return false;
            return true;
        }

        private bool BackwardDiagonalConnectFour(int row, int col)
        {
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int rowCursor = row + 1;
            int colCursor = col + 1;
            while (rowCursor < GameBoard.GetLength(0) && colCursor < GameBoard.GetLength(1) && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor++;
                colCursor++;
            }
            rowCursor = row - 1;
            colCursor = col - 1;
            while (rowCursor >= 0 && colCursor >= 0 && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor--;
                colCursor--;
            }
            if (count < 4)
                return false;
            return true;
        }
        #endregion

        public int Insert(Side side, int column)
        {
            // Start from bottom, work up
            for (int row = GameBoard.GetLength(0) - 1; row >= 0; row--)
            {
                if (GameBoard[row, column] == Side.None)
                {
                    GameBoard[row, column] = side;
                    return row;
                }
            }
            return -1;
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

        public int AfterInsert()
        {
            Side winner = Winner();

            if (winner != Side.None)
            {
                return (int)winner;
            }
            else if (Tied())
            {
                return (int)Side.None;
            }
            return -1;
        }
    }
}
