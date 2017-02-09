using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelClasses {
    class FloatPlatform : Challenge {
        internal static Brush FLOATING_BLOCK_COLOR = Brushes.ForestGreen;
        internal static Pen FLOATING_BLOCK_LINE = Pens.White;

        // Largura da plataforma
        private int _width;
        public int Width {
            get { return _width; }
            set { _width = value; }
        }

        // Define se o bloco cairá com o personagem
        private bool _fall;
        public bool IsFallingBlock {
            get { return _fall; }
            set { _fall = value; }
        }

        public FloatPlatform(int x, int y, int width, bool isFallingBlock) {
            PosX = x;
            PosY = y;
            _width = width;
            _fall = isFallingBlock;
        }

        public override void Draw(Bitmap img) {
            using (Graphics g = Graphics.FromImage(img)) {
                if (_fall) {
                    PointF[] grid = new PointF[3];
                    grid[0] = new PointF(PosX * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE);
                    grid[1] = new PointF((PosX + _width) * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE);
                    grid[2] = new PointF((PosX + _width / 2.0f) * Level.GRID_SIZE, img.Height - (PosY - 1) * Level.GRID_SIZE);
                    g.FillPolygon(FLOATING_BLOCK_COLOR, grid);
                    g.DrawPolygon(FLOATING_BLOCK_LINE, grid);
                } else {
                    Rectangle grid = new Rectangle(PosX * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE, _width * Level.GRID_SIZE, Level.GRID_SIZE);
                    g.FillRectangle(FLOATING_BLOCK_COLOR, grid);
                    g.DrawRectangle(FLOATING_BLOCK_LINE, grid);
                }
            }
        }
    }
}
