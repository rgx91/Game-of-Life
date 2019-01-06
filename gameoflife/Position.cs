using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dont_know
{
    class Position
    {
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
