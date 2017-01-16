using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelClasses {
    class FallingBlock : Challenge {

        internal static Brush FALLINGBLOCK_COLOR = Brushes.ForestGreen;
        internal static Pen FALLINGBLOCK_LINES = Pens.Azure;

        public FallingBlock(int posX, int posY) {
            PosX = posX;
            PosY = posY;
        }

        public override void Draw(Bitmap img) {
            Rectangle grid = new Rectangle(PosX * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE, Level.GRID_SIZE, Level.GRID_SIZE);

            using (Graphics g = Graphics.FromImage(img)) {
                g.FillRectangle(FALLINGBLOCK_COLOR, grid);
                g.DrawLine(FALLINGBLOCK_LINES, grid.Left, grid.Top, grid.Right, grid.Bottom);
                g.DrawLine(FALLINGBLOCK_LINES, grid.Right, grid.Top, grid.Left, grid.Bottom);
            }
        }
    }
}
