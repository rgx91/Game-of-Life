using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;

namespace Game_of_Life
{
    internal class Cell
    {
        public static int DeadCount = 0;
        public static int FriendCount = 0;
        public static int ChangedCount = 0;
        public static int EnemyCount = 0;
        public static byte DeadCellByte = 0;
        public static byte FriendCellByte = 1;
        public static byte ChangedCellByte = 2;
        public static byte EnemyCellByte = 3;
        public static Color AliveColor;
        public static Color DeadColor;
        public static Color ChangedColor;
        public static Color EnemyColor;
        private byte Condition;
        private bool NeedUpdate;//if true this cell need to refreshed at ui

        public Cell( byte condition)
        {
            
            Condition = condition;
            NeedUpdate = false;
            if (condition==DeadCellByte)
            {
                DeadCount++;
            }
            else if (condition==ChangedCellByte)
            {
                ChangedCount++;
            }
            else if(condition==FriendCellByte)
            {
                FriendCount++;
            }
            else
            {
                EnemyCount++;
            }
        }
        static Cell()
        {
            AliveColor = Colors.Red;
            DeadColor = Colors.Blue;
            ChangedColor = Colors.Yellow;
            EnemyColor = Colors.Black;
        }
        public bool IsColorUpdateNeeded()//check if refresh needed
        {
            return NeedUpdate;
        }
        public void NeedColorUpdate(bool need)//set refresh state
        {
            NeedUpdate = need;
        }
        public Color GetColor()
        {
            if (Condition==DeadCellByte)
            {
                return DeadColor;
            }
            else if (Condition==ChangedCellByte)
            {
                return ChangedColor;
            }
            else if(Condition==FriendCellByte)
            {
                return AliveColor;
            }
            else
            {
                return EnemyColor;
            }
            
        }
        public byte GetCondition()
        {
            return Condition;
        }
        public void SetCondition(byte condition)
        {

            if (Condition != condition)
            {//If we change a cell condition we should lower the original condition's counter and increment the new value's counter
                if (Condition == FriendCellByte)
                {
                    Interlocked.Decrement(ref FriendCount);
                }
                else if (Condition == DeadCellByte)
                {
                    Interlocked.Decrement(ref DeadCount);
                }
                else if(Condition==ChangedCellByte)
                {
                    Interlocked.Decrement(ref ChangedCount);
                }
                else
                {
                    Interlocked.Decrement(ref EnemyCount);
                }
            Condition = condition;
                if (condition == FriendCellByte)
                {
                    Interlocked.Increment(ref FriendCount);
                }
                else if (condition == DeadCellByte)
                {
                    Interlocked.Increment(ref DeadCount);
                }
                else if(condition==ChangedCellByte)
                {
                    Interlocked.Increment(ref ChangedCount);
                }
                else
                {
                    Interlocked.Increment(ref EnemyCount);
                }
                
            }

        }

    }
}
