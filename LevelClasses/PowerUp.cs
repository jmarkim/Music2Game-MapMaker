using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LevelClasses {
    class PowerUp : Challenge{
        internal static Brush POWERUP_COLOR = Brushes.Gold;
        internal static Pen POWERUP_LINE = Pens.Yellow;

        // Tipo de power-up
        private int _kind;
        public int Kind {
            get { return _kind; }
            set { _kind = value; }
        }

        public PowerUp(int x, int y, int kind) {
            PosX = x;
            PosY = y;
            _kind = kind;
        }

        public override void Draw(Bitmap img) {
            Rectangle grid = new Rectangle(PosX * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE, Level.GRID_SIZE, Level.GRID_SIZE);
            using (Graphics g = Graphics.FromImage(img)) {
                g.FillRectangle(POWERUP_COLOR, grid);
                g.DrawRectangle(POWERUP_LINE, grid);
            }
        }
    }
}
