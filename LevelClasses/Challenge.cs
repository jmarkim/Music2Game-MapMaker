using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace LevelClasses {
    public abstract class Challenge {

        // Posição X(grid) do desafio
        private int x;
        public int PosX {
            get { return x; }
            set { x = value; }
        }

        // Posição Y(grid) do desafio
        private int y;
        public int PosY {
            get { return y; }
            set { y = value; }
        }

        public abstract void Draw(Bitmap img);
    }
}
