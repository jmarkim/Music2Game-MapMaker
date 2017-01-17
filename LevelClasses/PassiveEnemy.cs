using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelClasses {
    class PassiveEnemy : Challenge{

        internal static Brush PASSIVE_ENEMY_COLOR = Brushes.Crimson;
        internal static Pen PASSIVE_ENEMY_FLY = Pens.Red;

        // Indica se o inimigo é voador
        private bool _fly;
        public bool Fly {
            get { return _fly; }
            set { _fly = value; }
        }

        // Construtor
        public PassiveEnemy(int posX, int posY, bool flyingEnemy) {
            PosX = posX;
            PosY = posY;
            _fly = flyingEnemy;
        }

        public override void Draw(Bitmap img) {
            Rectangle grid = new Rectangle(PosX * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE, Level.GRID_SIZE, Level.GRID_SIZE);

            using (Graphics g = Graphics.FromImage(img)) {
                g.FillRectangle(PASSIVE_ENEMY_COLOR, grid);
                if (_fly) {
                    g.DrawLine(PASSIVE_ENEMY_FLY, grid.Left, grid.Top, grid.Right - 1, grid.Bottom - 1);
                }
            }
        }
    }
}
