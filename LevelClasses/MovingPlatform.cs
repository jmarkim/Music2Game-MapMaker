using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LevelClasses {
    class MovingPlatform : Challenge{

        internal static Brush MOVING_PLATFORM_COLOR = Brushes.RoyalBlue;
        internal static Pen MOVE_RANGE_LINE = Pens.Purple;

        // Largura da plataforma
        private int _width;
        public int Width {
            get { return _width; }
            set { _width = value; }
        }

        // Tamanho do deslocamente
        private int _length;
        public int MovementLength {
            get { return _length; }
            set { _length = value; }
        }

        // Define direção domovimento
        private bool _vertical;
        public bool VerticalMovement {
            get { return _vertical; }
            set { _vertical = value; }
        }

        // Define orientação do movimento vertical
        private bool _up;
        public bool RisingPlatform {
            get { return _up; }
            set { _up = value; }
        }

        public MovingPlatform(int x, int y, int width, int range, bool verticalMove, bool risingMove) {
            PosX = x;
            PosY = y;
            _width = width;
            _length = range;
            _vertical = verticalMove;
            _up = risingMove;
        }

        public override void Draw(Bitmap img) {
            Rectangle platform = new Rectangle(PosX * Level.GRID_SIZE, img.Height - PosY * Level.GRID_SIZE, _width * Level.GRID_SIZE, Level.GRID_SIZE);
            Rectangle range;
            if (!_vertical) {
                // Movimento horizontal
                range = new Rectangle(platform.Left, platform.Top, (_width + _length) * Level.GRID_SIZE, Level.GRID_SIZE);
            } else {
                if (_up) {
                    // Movimento ascendente, plataforma posicionada na base
                    range = new Rectangle(platform.Left, platform.Top - _length * Level.GRID_SIZE, platform.Width, (_length + 1) * Level.GRID_SIZE);
                } else {
                    // Movimento descendente, plataforma posicionada no topo
                    range = new Rectangle(platform.Left, platform.Top, platform.Width, (_length + 1) * Level.GRID_SIZE);
                }
            }

            using (Graphics g = Graphics.FromImage(img)) {
                g.FillRectangle(MOVING_PLATFORM_COLOR, platform);
                g.DrawRectangle(MOVE_RANGE_LINE, range);
            }
        }
    }
}
