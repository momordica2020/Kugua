using Microsoft.AspNetCore.Mvc;
using NvAPIWrapper.Native.Display.Structures;
using System;
using System.Data;
using System.Text;

namespace Kugua.Mods{
    public class GameTest
    {
        
    }

    public class Game2048
    {
        private int[,] grid = new int[4, 4];
        public int score = 0;

        public Game2048()
        {

        }



        public void Initialize()
        {
            score = 0;
            Array.Clear(grid, 0, grid.Length);
            AddNewNumber();
            AddNewNumber();
        }

        public string GetGridString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                //sb.AppendLine("+####+----+----+----+");
                for (int j = 0; j < 4; j++)
                {
                    sb.Append("  ");
                    sb.Append(grid[i, j].ToString().PadLeft(8).PadRight(10).Substring(0, 8));
                }
                //if(i==0)sb.Append("          ")
                sb.AppendLine("  ");
            }
            //sb.AppendLine("+----+----+----+----+");
            sb.AppendLine($"得分：{score}");
            return sb.ToString();
        }

        public bool Move(int direction)
        {
            return direction switch
            {
                1 => MoveUp(),
                2 => MoveDown(),
                3 => MoveLeft(),
                4 => MoveRight(),
                _ => false
            };
        }

        private bool MoveUp() => TransposeMove(MoveLeft);
        private bool MoveDown() => TransposeMove(MoveRight);

        private bool MoveLeft()
        {
            bool moved = false;
            for (int i = 0; i < 4; i++)
            {
                var (newRow, changed) = ProcessRow(GetRow(i));
                if (changed)
                {
                    SetRow(i, newRow);
                    moved = true;
                }
            }
            return moved;
        }

        private bool MoveRight()
        {
            bool moved = false;
            for (int i = 0; i < 4; i++)
            {
                var row = GetRow(i).Reverse().ToArray();
                var (newRow, changed) = ProcessRow(row);
                Array.Reverse(newRow);
                if (changed)
                {
                    SetRow(i, newRow);
                    moved = true;
                }
            }
            return moved;
        }

        private (int[], bool) ProcessRow(int[] row)
        {
            // 压缩空白
            var compressed = row.Where(n => n != 0).Concat(new int[4]).Take(4).ToArray();

            // 合并相邻
            for (int i = 0; i < 3; i++)
            {
                if (compressed[i] != 0 && compressed[i] == compressed[i + 1])
                {
                    compressed[i] *= 2;
                    compressed[i + 1] = 0;
                    score += compressed[i];
                }
            }

            // 再次压缩
            var merged = compressed.Where(n => n != 0).Concat(new int[4]).Take(4).ToArray();
            return (merged, !merged.SequenceEqual(row));
        }

        private int[] GetRow(int index)
        {
            int[] row = new int[4];
            for (int j = 0; j < 4; j++) row[j] = grid[index, j];
            return row;
        }

        private void SetRow(int index, int[] values)
        {
            for (int j = 0; j < 4; j++) grid[index, j] = values[j];
        }

        private bool TransposeMove(Func<bool> move)
        {
            Transpose();
            bool moved = move();
            Transpose();

            return moved;
        }

        private void Transpose()
        {
            int[,] newGrid = new int[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    newGrid[i, j] = grid[j, i];
            grid = newGrid;
        }

        public void AddNewNumber()
        {
            var empty = new List<(int, int)>();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grid[i, j] == 0) empty.Add((i, j));

            if (empty.Count > 0)
            {
                var (i, j) = empty[MyRandom.Next(empty.Count)];
                grid[i, j] = MyRandom.Next(0,2)==0? 2:4;
                score += grid[i, j];
            }
        }

        public bool IsGameOver()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grid[i, j] == 0) return false;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    if (grid[i, j] == grid[i, j + 1] || grid[j, i] == grid[j + 1, i])
                        return false;

            return true;
        }

        //public string Show()
        //{
        //    StringBuilder sb = new StringBuilder();
            
            
        //    for(int i = 0; i < 4; i++)
        //    {
        //        sb.AppendLine($"+----+----+----+----+");
        //        sb.AppendLine($"|    |    |    |    |");
        //        sb.Append($"|");
        //        for (int j = 0; j < 4; j++)
        //        {
        //            var data = Data[i * 4 + j];
        //            if (data != 0) sb.Append($"{data.ToString().PadLeft(4)}|");
        //            else sb.Append($"    |");
        //        }
        //        sb.AppendLine($"+");
        //        sb.AppendLine($"|    |    |    |    |");
        //    }
        //    sb.AppendLine($"+----+----+----+----+");

        //    return sb.ToString();
        //    //+ $"+{string.Join("+", Data[0..4])}+";
        //}


        //int EmptyCheck(int loc, int pos)
        //{
        //    int emptyIndex = -1;
        //    while (GetLocData(loc + posStep(pos)) == 0)
        //    {
        //        emptyIndex = loc + posStep(pos);
        //        loc = emptyIndex;
        //    }
         
        //    return emptyIndex;
        //}

        //int GetLocData(int loc)
        //{
        //    if (loc < 0 || loc >= 16) return -1;
        //    else return Data[loc];
        //}

        //int posStep(int position)
        //{
        //    if (position == 0) return -4;
        //    else if (position == 1) return 1;
        //    else if (position == 2) return 4;
        //    else if (position == 3) return -1;
        //    else return 1;
        //}

        ////       0
        ////    3     1
        ////       2
        ////
        //public bool Move(int position)
        //{
        //    int next = 0;
        //    if (position == 0)
        //    {
        //        for(int row = 0; row < 4; row++)
        //        {
                    
        //            for (int col = 3; col >=0; col--)
        //            {
        //                var empty = EmptyCheck(row * 4 + col, position);
        //                if (empty >= 0)
        //                {
        //                    // has empty.move

        //                }
        //                for(int tarcol = col + 1; tarcol < 4; tarcol++)
        //                {
        //                    if (Data[tarcol]==0)
        //                }
                        
        //            }
        //        }
        //    }
        //    else if (position == 1) next = 1;
        //    else if (position == 2) next = 4;
        //    else if (position == 3) next = -1;

            

        //        return true;
        //}
    }
}