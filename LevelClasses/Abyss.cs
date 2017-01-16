using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace LevelClasses {
    public class Abyss : Challenge {

        internal static Brush ABYSS_BACKGROUND = Brushes.Lavender;
        internal static Pen ABYSS_LINE = new Pen(Brushes.Red, 2);

        // Lagura do abismo
        private int _width;
        public int Width {
            get { return _width; }
            set { _width = value; }
        }

        //Construtor
        public Abyss(int posX, int posY, int width) {
            PosX = posX;
            PosY = posY;
            _width = width;
        }

        public override void Draw(Bitmap img) {
            Rectangle grid = new Rectangle(0, 0, Level.GRID_SIZE, Level.GRID_SIZE);

            using (Graphics g = Graphics.FromImage(img)) {
                for (int xx = 0; xx < _width; xx++) {
                    grid.X = (PosX + xx) * Level.GRID_SIZE;

                    for (int yy = 0; yy < PosY; yy++) {
                        grid.Y = img.Height - yy * Level.GRID_SIZE;
                        g.FillRectangle(ABYSS_BACKGROUND, grid);
                        g.DrawLine(ABYSS_LINE, grid.Left, grid.Bottom, grid.Right, grid.Top);
                    }
                    grid.Y = img.Height - PosY * Level.GRID_SIZE;
                    g.FillRectangle(ABYSS_BACKGROUND, grid);
                }
            }
            
        }

    }
}
