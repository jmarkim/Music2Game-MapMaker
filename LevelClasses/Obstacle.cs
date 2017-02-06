using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LevelClasses {
    class Obstacle : Challenge {

        internal static Brush OBSTACLE_COLOR = Brushes.DimGray;

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
            Rectangle grid = new Rectangle(PosX * Level.GRID_SIZE, img.Height - (PosY + _height) * Level.GRID_SIZE, _width, _height);

            using (Graphics g = Graphics.FromImage(img)) {
                g.FillEllipse(OBSTACLE_COLOR, grid);
            }
        }
    }
}
