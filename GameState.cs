using System;
using System.Collections.Generic;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public Direction Dir2 { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Direction> dirChanges2 = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly LinkedList<Position> snakePositions2 = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;
            Dir2 = Direction.Left;

            AddSnake(snakePositions, Rows / 2, 1);
            AddSnake(snakePositions2, Rows / 2, Cols - 3);

            AddFood();
        }

        private void AddSnake(LinkedList<Position> snake, int startRow, int startCol)
        {
            Grid[startRow, startCol] = GridValue.Snake;
            snake.AddFirst(new Position(startRow, startCol));
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public Position HeadPosition() => snakePositions.First.Value;

        public Position HeadPosition2() => snakePositions2.First.Value;

        public Position TailPosition() => snakePositions.Last.Value;

        public Position TailPosition2() => snakePositions2.Last.Value;

        public IEnumerable<Position> SnakePositions() => snakePositions;

        public IEnumerable<Position> SnakePositions2() => snakePositions2;

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir, Dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        public void ChangeDirection2(Direction dir2)
        {
            if (CanChangeDirection(dir2, Dir2))
            {
                dirChanges2.AddLast(dir2);
            }
        }

        private Direction GetLastDirection(LinkedList<Direction> directionChanges, Direction currentDir)
        {
            if (directionChanges.Count == 0)
            {
                return currentDir;
            }

            return directionChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir, Direction currentDir)
        {
            return newDir != currentDir && newDir != currentDir.Opposite();
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private bool firstMove = true;

        private GridValue WillHit(Position newHeadPos, LinkedList<Position> snake)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (firstMove)
            {
                firstMove = false;
                return Grid[newHeadPos.Row, newHeadPos.Col];
            }

            // If the snake's new head position is the same as its tail, we allow it to move into that position.
            if (newHeadPos.Equals(snake.Last.Value))
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            if (dirChanges2.Count > 0)
            {
                Dir2 = dirChanges2.First.Value;
                dirChanges2.RemoveFirst();
            }

            Position newHeadPos1 = GetNewHeadPosition(HeadPosition(), Dir);
            Position newHeadPos2 = GetNewHeadPosition(HeadPosition2(), Dir2);

            GridValue hit1 = WillHit(newHeadPos1, snakePositions);
            GridValue hit2 = WillHit(newHeadPos2, snakePositions2);

            if (hit1 == GridValue.Outside || hit1 == GridValue.Snake || hit2 == GridValue.Outside || hit2 == GridValue.Snake)
            {
                GameOver = true;
            }
            else
            {
                if (hit1 == GridValue.Food)
                {
                    AddHead(newHeadPos1, snakePositions);
                    Score++;
                    AddFood();
                }
                else
                {
                    AddHead(newHeadPos1, snakePositions);
                    RemoveTail(snakePositions);
                }

                if (hit2 == GridValue.Food)
                {
                    AddHead(newHeadPos2, snakePositions2);
                    Score++;
                    AddFood();
                }
                else
                {
                    AddHead(newHeadPos2, snakePositions2);
                    RemoveTail(snakePositions2);
                }
            }
        }
        
        private void AddBody(LinkedList<Position> snake, Position pos)
        {
            Grid[pos.Row, pos.Col] = GridValue.Snake;
            snake.AddLast(pos);
        }

        private Position GetNewHeadPosition(Position currentHead, Direction dir)
        {
            return new Position(currentHead.Row + dir.RowOffset, currentHead.Col + dir.ColOffset);
        }

        private void AddHead(Position pos, LinkedList<Position> snake)
        {
            snake.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail(LinkedList<Position> snake)
        {
            Position tail = snake.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snake.RemoveLast();
        }
    }
}
