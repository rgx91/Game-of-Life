using System;
using System.Threading.Tasks;

namespace Game_of_Life
{
    internal class Generator
    {
        public static int GenNum = 0;
        private readonly Cell[][] CellJaggedArray;
        private readonly Random R;
        public Generator(Cell[][] arrayofAllCells)
        {
            CellJaggedArray = arrayofAllCells;
            R = new Random();

        }
        private struct FriendandEnemyCount
        {
            public readonly int Friend;
            public readonly int Enemy;
            public FriendandEnemyCount(int friend, int enemy)
            {
                Friend = friend;
                Enemy = enemy;
            }
        }
        public void CalculateNextGen()
        {
            //We calculate here every cell's friend and enemy count
            var SorrundingValues = new FriendandEnemyCount[CellJaggedArray.Length][];
            Parallel.For(0, CellJaggedArray.Length, i =>
                  {
                      SorrundingValues[i] = new FriendandEnemyCount[CellJaggedArray[0].Length];
                      for (var j = 0; j < CellJaggedArray[0].Length; j++)
                      {
                          SorrundingValues[i][j] = EnvirenmentCount(i, j);

                      }
                  }
                );




            Parallel.For(0, CellJaggedArray.Length,  i =>
              {
                  for (var j = 0; j < CellJaggedArray[i].Length; j++)
                  {
                      CellJaggedArray[i][j].NeedColorUpdate(false);
                      if (CellJaggedArray[i][j].GetCondition() == Cell.FriendCellByte && SorrundingValues[i][j].Friend < 2 || CellJaggedArray[i][j].GetCondition() == Cell.EnemyCellByte && SorrundingValues[i][j].Enemy < 2)
                      {//Any  cell with fewer than two live neighbors dies, as if by underpopulation
                          CellJaggedArray[i][j].SetCondition(Cell.ChangedCellByte);
                          CellJaggedArray[i][j].NeedColorUpdate(true);
                          continue;
                      }
                      if (CellJaggedArray[i][j].GetCondition() == Cell.FriendCellByte && SorrundingValues[i][j].Friend > 3 || CellJaggedArray[i][j].GetCondition() == Cell.EnemyCellByte && SorrundingValues[i][j].Enemy > 3)
                      {//Any live cell with more than three live neighbors dies, as if by overpopulation
                          CellJaggedArray[i][j].SetCondition(Cell.ChangedCellByte);
                          CellJaggedArray[i][j].NeedColorUpdate(true);
                          continue;
                      }
                      if (CellJaggedArray[i][j].GetCondition() == Cell.DeadCellByte && SorrundingValues[i][j].Friend == 3 && SorrundingValues[i][j].Enemy != 3 || CellJaggedArray[i][j].GetCondition() == Cell.ChangedCellByte && SorrundingValues[i][j].Friend == 3 && SorrundingValues[i][j].Enemy != 3)
                      {//Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction
                          CellJaggedArray[i][j].SetCondition(Cell.FriendCellByte);
                          CellJaggedArray[i][j].NeedColorUpdate(true);
                          continue;
                      }

                      if (CellJaggedArray[i][j].GetCondition() == Cell.DeadCellByte && SorrundingValues[i][j].Enemy == 3 && SorrundingValues[i][j].Friend != 3 || CellJaggedArray[i][j].GetCondition() == Cell.ChangedCellByte && SorrundingValues[i][j].Enemy == 3 && SorrundingValues[i][j].Friend != 3)
                      {//Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction
                          CellJaggedArray[i][j].SetCondition(Cell.EnemyCellByte);
                          CellJaggedArray[i][j].NeedColorUpdate(true);
                          continue;
                      }
                      if (CellJaggedArray[i][j].GetCondition() == Cell.FriendCellByte && SorrundingValues[i][j].Enemy == 3 || CellJaggedArray[i][j].GetCondition() == Cell.EnemyCellByte && SorrundingValues[i][j].Friend == 3)
                      {
                          //Any live cell with exactly three enemy neighbors should fight
                          Fight(i, j);//You can change this behavior in the expression for more or less aggressive life
                      }


                  }
              }
            );
            GenNum++;
        }

        private void Fight(int i, int j)
        {
            if (Cell.FriendCount == Cell.EnemyCount)
            {//random chance to win
                if (R.Next(0, 1000) < 500)
                {
                    CellJaggedArray[i][j].SetCondition(Cell.FriendCellByte);
                    CellJaggedArray[i][j].NeedColorUpdate(true);
                }
                else
                {
                    CellJaggedArray[i][j].SetCondition(Cell.EnemyCellByte);
                    CellJaggedArray[i][j].NeedColorUpdate(true);
                }
            }
            else if (Cell.FriendCount > Cell.EnemyCount)
            {//Friend has a bigger chance to win because numerical superiority causes increased moral
                var Proportion = (500 / (Cell.FriendCount / (double)Cell.EnemyCount));
                if (R.NextDouble()*1000 < (500 + (500 - Proportion)))
                {
                    CellJaggedArray[i][j].SetCondition(Cell.FriendCellByte);
                    CellJaggedArray[i][j].NeedColorUpdate(true);
                }
                else
                {
                    CellJaggedArray[i][j].SetCondition(Cell.EnemyCellByte);
                    CellJaggedArray[i][j].NeedColorUpdate(true);
                }
            }
            else
            {//Enemy has a bigger chance to win because numerical superiority causes increased moral
                var Proportion = (500 / (Cell.EnemyCount / (double)Cell.FriendCount));
                if (R.NextDouble()*1000 < 500 -(500- Proportion))
                {
                    CellJaggedArray[i][j].SetCondition(Cell.FriendCellByte);
                    CellJaggedArray[i][j].NeedColorUpdate(true);
                }
                else
                {
                    CellJaggedArray[i][j].SetCondition(Cell.EnemyCellByte);
                    CellJaggedArray[i][j].NeedColorUpdate(true);
                }
            }
        }

        private FriendandEnemyCount EnvirenmentCount(int x, int y)
        {
            var Friend = NeighborCounter(Cell.FriendCellByte, x, y);
            var Enemy = NeighborCounter(Cell.EnemyCellByte, x, y);
            return new FriendandEnemyCount(Friend, Enemy);

        }
        private int NeighborCounter(byte condition, int x, int y)
        {
            if (condition==Cell.EnemyCellByte && Cell.EnemyCount==0)
            {
                return 0;
            }
            if (condition==Cell.FriendCellByte&& Cell.FriendCount==0)
            {
                return 0;
            }
            var count = 0;
            if (x > 0)
            {
                if (CellJaggedArray[x - 1][y].GetCondition() == condition)
                {
                    count += 1;
                }
                if (y > 0)
                {
                    if (CellJaggedArray[x - 1][y - 1].GetCondition() == condition)
                    {
                        count += 1;
                    }
                }
                if (y < CellJaggedArray[0].Length - 1)
                {
                    if (CellJaggedArray[x - 1][y + 1].GetCondition() == condition)
                    {
                        count += 1;
                    }
                }
            }
            if (y > 0)
            {
                if (CellJaggedArray[x][y - 1].GetCondition() == condition)
                {
                    count += 1;
                }
            }
            if (y < CellJaggedArray[0].Length - 1)
            {
                if (CellJaggedArray[x][y + 1].GetCondition() == condition)
                {
                    count += 1;
                }
            }
            if (x < CellJaggedArray.Length - 1)
            {
                if (CellJaggedArray[x + 1][y].GetCondition() == condition)
                {
                    count += 1;
                }
                if (y > 0)
                {
                    if (CellJaggedArray[x + 1][y - 1].GetCondition() == condition)
                    {
                        count += 1;
                    }
                }
                if (y < CellJaggedArray[0].Length - 1)
                {
                    if (CellJaggedArray[x + 1][y + 1].GetCondition() == condition)
                    {
                        count += 1;
                    }
                }
            }

            return count;

        }
    }
}

