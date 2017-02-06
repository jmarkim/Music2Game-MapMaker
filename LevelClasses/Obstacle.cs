using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LevelClasses {
    class Obstacle : Challenge {

        internal static Brush OBSTACLE_COLOR = Brushes.DimGray;
        internal static Pen OBSTACLE_LINE = Pens.White;

        // Altura
        private int _height;
        public int Height {
            get { return _height; }
            set { _height = value; }
        }

        // Largura
        private int _width;
        public int Width {
            get { return _width; }
            set { _width = value; }
        }

        // Construtor
        public Obstacle(int x, int y, int width, int height) {
            PosX = x;
            PosY = y;
            _width = width;
            _height = height;
        }

        public override void Draw(Bitmap img) {
            Rectangle grid = new Rectangle(0, 0, Level.GRID_SIZE, Level.GRID_SIZE);

            using (Graphics g = Graphics.FromImage(img)) {
                for(int w = 0; w < _width; w++) {
                    grid.X = (PosX + w) * Level.GRID_SIZE;
                    for (int h = 1; h <= _height; h++) {
                        grid.Y = img.Height - (PosY + h) * Level.GRID_SIZE;
                        g.FillRectangle(OBSTACLE_COLOR, grid);
                        g.DrawLine(OBSTACLE_LINE, grid.Left, grid.Bottom, grid.Right, grid.Top);
                    }
                }
                g.DrawRectangle(OBSTACLE_LINE, PosX * Level.GRID_SIZE, img.Height - (PosY + _height) * Level.GRID_SIZE, _width * Level.GRID_SIZE, _height * Level.GRID_SIZE);
            }
        }
    }
}
