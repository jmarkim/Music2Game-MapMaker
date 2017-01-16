using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicScore;
using System.Drawing;
using System.Drawing.Imaging;

namespace LevelClasses {
    public class Level
    {
        // Definições globais
        internal static int STARTING_HEIGHT = 30; // Altura da primeira plataforma
        internal static int MEASURE_SIZE = 16; // Tamanho de um compasso em quadros
        internal static int SCREEN_SIZE = 32; // Tamanho de uma tela em quadros (horizontal)
        internal static int CEILING_HEIGHT = 40; // Altura do teto em relação à plataforma mais alta
        internal static int HEIGHT_MODIFIER = 5; // Valor pelo qual "delta" é multiplicado antes de dividido pelo tamanho do compasso
        internal static int MAXIMUM_WIDTH = 16; // Largura máxima (soma) de abismos em uma seção
        internal static int MINIMUM_WIDTH = 4; // Largura mínima (soma) de abismos em um seção
        internal static int ABYSS_THRESHOLD = 8; // Largura mínima de uma abismo para que seja coberto por blocos que caem
        internal static double ABYSS_CHANCE = 0.3; // Define o limiar de intensidade para geração de abismos

        // Definições para geração de Imagens
        internal static int GRID_SIZE = 10; // Tamanho do lado do grid (usado para gerção da imagem)
        internal static Brush BACKGROUND_SPECIAL = Brushes.MistyRose;
        internal static Brush BACKGROUND_NORMAL = Brushes.Lavender;
        internal static Brush PLATFORM_DEEP = Brushes.Sienna;
        internal static Brush PLATFORM_SURFACE = Brushes.LawnGreen;
        internal static Pen PLATFORM_DEEP_LINE = new Pen(Brushes.Maroon, 2);
        internal static Pen SCREEN_BOUNDS_LINE = Pens.Red;
        internal static Pen MEASURE_BOUNDS_LINE = Pens.PaleVioletRed;


        // Largura da fase em quadros (corresponde à área de música)
        private int _width;
        public int Width {
            get { return _width; }
            set { _width = value; }
        }

        // Altura da fase em quadros
        private int _height;
        public int Height {
            get { return _height; }
            set { _height = value; }
        }

        // Define a geografia da fase, definida como altura das plataformas em cada quadro
        private List<int> _geography;
        public List<int> Geography {
            get { return _geography; }
        }

        // Lista de abismos
        private List<Abyss> _abysses;
        public List<Abyss> Abysses {
            get { return _abysses; }
        }

        // Lista e desafios da fase (inimigos, abismos, obstáculos, etc)
        private List<Challenge> _challenges;
        public List<Challenge> Challenges {
            get { return _challenges; }
        }

        // Construtor a partir de uma música
        public Level(Score music) {
            _width = MEASURE_SIZE * music.MeasureCount;
            _geography = new List<int>(music.MeasureCount);
            _challenges = new List<Challenge>();
            _abysses = new List<Abyss>();
            BuildGeography(music);
            BuildChallenges(music);
            _height = _geography.Max() + CEILING_HEIGHT;
        }

        public void BuildGeography(Score music) {
            // Definição dos papéis
            Scale Reference;
            Scale Elevation;
            Scale Depression;
            Scale ConditionalElevation;
            Scale ConditionalDepression;
            Scale NegativeCorrection;
            Scale PositiveCorrection;

            // Contagem das notas da escala
            List<int> count = music.RoleCounts();

            int max; // Quantidade de notas da escala que mais aparece
            int maxId; // Nota da escala que mais aparece
            int measureCount = music.MeasureCount; // Quantidade de Compassos da música
            int partCount = music.PartCount; // Quantidade de Instrumentos da música
            int sum = 0; // Soma dos deltas (valor final de Δh)
            int delta; // Contribuição do instrumento (valor parcial de Δh)

            Measure currentMeasure; // Compasso sendo trabalhado

            // Referência >> Δh += X, se Δh < 0 .OU. Δh -= X, se Δh > 0
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Reference = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Elevação >> Δh += X
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Elevation = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Depressão >> Δh -= X
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Depression = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Elevação Condicional >> Δh += X, se Δh >= 0
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            ConditionalElevation = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Depressão Condicional >> Δh -= X, Δh <= 0
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            ConditionalDepression = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Correção Negativa >> Δh += X, se Δh < 0
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            NegativeCorrection = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Correção Positiva >> Δh -= X, se Δh > 0
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            PositiveCorrection = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            for (int measureNumber = 0; measureNumber < measureCount; measureNumber++) {
                sum = 0;

                for (int partNumber = 0; partNumber < partCount; partNumber++) {
                    delta = _geography.LastOrDefault(); // !!! Na "versão original" o algoritmo ignorava o delta calculado pelo instrumento anterior
                    currentMeasure = music.Parts[partNumber].Measures[measureNumber];

                    foreach (var note in currentMeasure.Elements) {
                        if (!note.IsRest()) {

                            if(note.Note.Role == Reference) {
                                if (delta < 0) {
                                    delta += note.Duration;
                                } else if (delta > 0) {
                                    delta -= note.Duration;
                                }

                            } else if (note.Note.Role == Elevation) {
                                delta += note.Duration;

                            } else if (note.Note.Role == Depression) {
                                delta -= note.Duration;

                            } else if (note.Note.Role == ConditionalElevation) {
                                if (delta >= 0) {
                                    delta += note.Duration;
                                }

                            } else if (note.Note.Role == ConditionalDepression) {
                                if (delta <= 0) {
                                    delta -= note.Duration;
                                }

                            } else if (note.Note.Role == NegativeCorrection) {
                                if (delta < 0) {
                                    delta += note.Duration;
                                }

                            } else if (note.Note.Role == PositiveCorrection) {
                                if (delta > 0) {
                                    delta -= note.Duration;
                                }
                            }
                        }
                    }

                    sum += (HEIGHT_MODIFIER * delta) / currentMeasure.Size;
                }

                _geography.Add(sum / partCount);
            }

            _geography[0] = STARTING_HEIGHT + _geography[0];
            for (int ii = 1; ii < _geography.Count; ii++) {
                _geography[ii] = _geography[ii - 1] + _geography[ii];
            }
        }

        internal void BuildChallenges(Score music) {
            // Definição dos papéis
            Scale FloatingBlocks;
            Scale Obstacles;
            Scale PassiveEnemies;
            Scale Others;
            Scale ActiveEnemies;
            Scale Traps;
            Scale Abyss;

            // Contagem das notas da escala
            List<int> count = music.RoleCounts();
            int max; // Quantidade de notas da escala que mais aparece
            int maxId; // Nota da escala que mais aparece
            int measureCount = music.MeasureCount; // Quantidade de Compassos da música
            int partCount = music.PartCount; // Quantidade de Instrumentos da música
            List<int> sum = new List<int>(); // Soma dos deltas (valor final de Δh)
            List<MeasureElement> abyssNotes = new List<MeasureElement>();

            Measure currentMeasure = null; // Compasso sendo trabalhado

            // Blocos flutuantes - plataformas e blocos sólidos
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            FloatingBlocks = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Obstáculos - Pedras e outras estruturas que impedem o progresso do joador, inofensivos
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Obstacles = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Inimigos Passivos - Inimigos que não tentam causar dano ao jogador ativamente
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            PassiveEnemies = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Função ainda não atribuída - Paisagens e ecompenssas, talvez
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Others = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Inimigos Ativos - Inimigos que atacam o jogador ativamente
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            ActiveEnemies = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Armadilhas - Obstáculos que causam dano ao jogador descuidado
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Traps = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            // Abismos - Buracos extras
            max = count.Max(); // Busca papel de maior ocorrência
            maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
            count[maxId] = -1; // Remove da próxima iteração
            Abyss = Note.IntToRole(maxId); // Define nota da escala responsável pela função

            for (int ii = 0; ii < 7; ii++) {
                sum.Add(0);
            }
            sum.TrimExcess();

            for (int measureNumber = 0; measureNumber < measureCount; measureNumber++) {
                for (int ii = 0; ii < 7; ii++) {
                    sum[ii] = 0;
                }
                abyssNotes.Clear();

                for (int partNumer = 0; partNumer < partCount; partNumer++) {
                    currentMeasure = music.Parts[partNumer].Measures[measureNumber];

                    foreach (var note in currentMeasure.Elements) {
                        if (!note.IsRest()) {

                            if (note.Note.Role == FloatingBlocks) {
                                sum[0] = note.Duration;
                            } else if (note.Note.Role == Obstacles) {
                                sum[1] += note.Duration;
                            } else if (note.Note.Role == PassiveEnemies) {
                                sum[2] += note.Duration;
                            } else if (note.Note.Role == Others) {
                                sum[3] += note.Duration;
                            } else if (note.Note.Role == ActiveEnemies) {
                                sum[4] += note.Duration;
                            } else if (note.Note.Role == Traps) {
                                sum[5] += note.Duration;
                            } else if (note.Note.Role == Abyss) {
                                sum[6] += note.Duration;
                                abyssNotes.Add(note);
                            }

                        }
                    }

                }

                // TODO : Processar resultados
                SetAbyss(measureNumber, currentMeasure.Size, partCount, sum[6], abyssNotes);
                //SetFloatingBlocks(measureNumber, currentMeasure.Size, sum[0])
                //SetObstacles(measureNumber, currentMeasure.Size, sum[1]);
                //SetPassiveEnemies(measureNumber, currentMeasure.Size, sum[2]);
                //SetOthers(measureNumber, currentMeasure.Size, sum[3]);
                //SetActiveEnemies(measureNumber, currentMeasure.Size, sum[4]);
                //SetTraps(measureNumber, currentMeasure.Size, sum[5]);
            }

        }

        internal void SetAbyss(int measureNumber, int measureSize, int partCount, int intensity, List<MeasureElement> sources) {
            int nextValidPosition = 0;
            int width;
            int position;
            int offset;

            sources.Sort((a,b) => a.Position.CompareTo(b.Position));

            foreach (var src in sources) {
                if (src.Position >= nextValidPosition) {
                    width = MEASURE_SIZE * src.Duration / measureSize / 2;
                    position = MEASURE_SIZE * src.Position / measureSize;
                    offset = SCREEN_SIZE + measureNumber * MEASURE_SIZE + position + intensity % (width + 1);
                    if (width < 2) {
                        nextValidPosition = src.Position + measureSize * 3 / MEASURE_SIZE;
                        width = 2;
                    } else {
                        nextValidPosition = src.Position + src.Duration;
                    }
                    if (position + width > MEASURE_SIZE) {
                        width = MEASURE_SIZE - position;
                    }
                    _abysses.Add(new Abyss(offset, _geography[measureNumber], width));

                }
            }
        }

        public void SaveImage(string name) {
            using (Bitmap img = new Bitmap((_width + 2 * SCREEN_SIZE) * GRID_SIZE, _height * GRID_SIZE)) {
                DrawBackground(img);
                //Console.WriteLine("   !!! Level.SaveImage() : Número de seções = {0}", _geography.Count);
                DrawSpecialSection(img, true, STARTING_HEIGHT);
                for (int section = 0; section < _geography.Count; section++) {
                    //Console.WriteLine("      !!! Seção {0} : Altura {1}", section, _geography[section]);
                    DrawSection(img, section, _geography[section]);
                }
                DrawSpecialSection(img, false, _geography.Last());

                foreach (var abyss in _abysses) {
                    abyss.Draw(img);
                }

                foreach (var challenge in _challenges) {
                    challenge.Draw(img);  
                }

                DrawMeasureBounds(img);
                DrawScreenBounds(img);
                img.Save(name + ".png", ImageFormat.Png);

            }
        }

        internal void DrawScreenBounds(Bitmap img) {
            using (Graphics g = Graphics.FromImage(img)) {
                int posX = SCREEN_SIZE * GRID_SIZE;
                while (posX < img.Width) {
                    g.DrawLine(SCREEN_BOUNDS_LINE, posX, 0, posX, img.Height);
                    posX += SCREEN_SIZE * GRID_SIZE;
                }
            }
        }

        internal void DrawMeasureBounds(Bitmap img) {
            using (Graphics g = Graphics.FromImage(img)) {
                int posX = MEASURE_SIZE * GRID_SIZE;
                while (posX < img.Width) {
                    g.DrawLine(MEASURE_BOUNDS_LINE, posX, 0, posX, img.Height);
                    posX += MEASURE_SIZE * GRID_SIZE;
                }
            }
        }

    //internal void DrawAbyss(Bitmap img, Abyss abyss) {
    //    Rectangle grid = new Rectangle(0, 0, GRID_SIZE, GRID_SIZE);
    //    using (Graphics g = Graphics.FromImage(img)) {
    //        for (int xx = 0; xx < abyss.Width; xx++) {
    //            grid.X = (abyss.PosX + xx) * GRID_SIZE;

    //            for (int yy = 0; yy < abyss.PosY; yy++) {
    //                grid.Y = img.Height - yy * GRID_SIZE;
    //                g.FillRectangle(BACKGROUND_SPECIAL, grid);
    //                g.DrawLine(new Pen(Brushes.HotPink, 2), grid.Left, grid.Bottom, grid.Right, grid.Top);
    //            }
    //        }
    //    }
    //}

    internal void DrawSection(Bitmap img, int sectionNumber, int sectionHeight) {
            Rectangle grid = new Rectangle(0, 0, GRID_SIZE, GRID_SIZE);
            using (Graphics g = Graphics.FromImage(img)) {
                for (int xx = 0; xx < MEASURE_SIZE; xx++) {
                    grid.X = (SCREEN_SIZE + sectionNumber * MEASURE_SIZE + xx) * GRID_SIZE;

                    for (int yy = 0; yy < sectionHeight; yy++) {
                        grid.Y = img.Height - yy * GRID_SIZE;
                        g.FillRectangle(PLATFORM_DEEP, grid);
                        g.DrawLine(PLATFORM_DEEP_LINE, grid.Left, grid.Bottom, grid.Right, grid.Top);
                    }

                    grid.Y = img.Height - sectionHeight * GRID_SIZE;
                    g.FillRectangle(PLATFORM_SURFACE, grid);
                }
            }
        }

        internal void DrawSpecialSection(Bitmap img, bool startingArea, int height) {
            Rectangle grid = new Rectangle(0, 0, GRID_SIZE, GRID_SIZE);
            using (Graphics g = Graphics.FromImage(img)) {
                for (int xx = 0; xx < SCREEN_SIZE; xx++) {
                    if (startingArea) {
                        grid.X = xx * GRID_SIZE;
                    } else {
                        grid.X = (SCREEN_SIZE + _width + xx) * GRID_SIZE;
                    }

                    for (int yy = 0; yy < height; yy++) {
                        grid.Y = img.Height - yy * GRID_SIZE;
                        g.FillRectangle(PLATFORM_DEEP, grid);
                        g.DrawLine(new Pen(Brushes.Maroon, 2), grid.Left, grid.Bottom, grid.Right, grid.Top);
                    }

                    grid.Y = img.Height - height * GRID_SIZE;
                    g.FillRectangle(PLATFORM_SURFACE, grid);
                }
            }
        }

        internal void DrawBackground(Bitmap img) {
            using (Graphics g = Graphics.FromImage(img)) {
                g.FillRectangle(BACKGROUND_SPECIAL, 0, 0, SCREEN_SIZE * GRID_SIZE, img.Height);
                g.FillRectangle(BACKGROUND_NORMAL, SCREEN_SIZE * GRID_SIZE, 0, _width * GRID_SIZE, img.Height);
                g.FillRectangle(BACKGROUND_SPECIAL, (SCREEN_SIZE + _width) * GRID_SIZE, 0, SCREEN_SIZE * GRID_SIZE, img.Height);
            }
        }
    }
}
