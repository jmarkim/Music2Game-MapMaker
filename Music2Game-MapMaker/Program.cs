using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicScore;
using System.Drawing;
using System.Drawing.Imaging;

namespace Music2Game_MapMaker {
    class Program {

        // Constantes
        private static int BASEHEIGHT = 10; // Altura da primeira tela;
        private static int SCREEN = 32; // Tamanho em quadros da tela
        private static int MEASURE = 16; // Tamanho em quadros de um compasso
        private static int GRID = 5; // Lados do quadro do grid (em px)
        private static int MAXID = 3; // Quantidade máxima de desafios em cada categoria
        private static string VERSION = "0.2.0";

        static void Main(string[] args) {

            // Log (Console) de Características
            Console.WriteLine("Versão do algoitmo : {0}", VERSION);
            Console.WriteLine("Tamanho do quadro : {0}px", GRID);
            Console.WriteLine("Tamanho do compasso : {0} quadros", MEASURE);
            Console.WriteLine("Tamanho da tela : {0} quadros", SCREEN);

            // Representação da partitura
            Score music = ScoreBuilder.FromXML(args[0]);

            // Nome do arquivo (sem extensão)
            string name = args[0].Remove(args[0].Length - 4);

            // Métdo : Sequência de alturas por compasso
            Console.WriteLine();
            Console.WriteLine("Criando mapas utilizando o método: Sequência de alturas");
            try {
                HeightsSequence(music, name, true);
                Console.WriteLine();
                HeightsSequence(music, name, false);
            } catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void HeightsSequence(Score music, string name, bool mostActive) {

            // Sequencia de alturas das "telas"
            List<int> baseHeights = SetHeights(music);

            // Lista de inimigos
            List<Tuple<int, int>> enemies = SetEnemies(music);

            // Transforma a sequencia de alturas base (por compasso) em alturas "por quadro"
            List<int> gridHeights = MeasureToGrid(baseHeights);

            // Gera novos abismos (ou plataformas e regiões "abissais")
            if (mostActive) {
                Console.WriteLine("Usando isntrumento mais ativo");
                name += "_ma";
            } else {
                Console.WriteLine("Usando isntrumento menos ativo");
                name += "_la";
            }
            Console.Write("   Adicionando novos abismos... ");
            gridHeights = Abyss(music, gridHeights, mostActive);
            Console.WriteLine("OK");

            // Desanha mapa, como definido pela sequência de alturas
            DrawMap(name + "_HS", gridHeights);
        }

        public static List<Tuple<int, int>> SetEnemies(Score music) {
            List<Tuple<int, int>> enemies = new List<Tuple<int, int>>();
            Part part = music.MostActive();

            if (part == null) {
                throw new Exception("Falha em MusicScore.Score.MostActive()");
            }

            int measureCount = 0;
            Tuple<int, int> enm;
            int posX;
            int kind;

            foreach (var measure in part.Measures) {
                foreach (var note in measure.Elements) {
                    if (note.Type == MeasureElementType.Chord || note.Type == MeasureElementType.Rest) {
                        continue;
                    }

                    if (Note.RoleToInt(note.Note.Role) <= 0) {
                        continue;
                    }

                    posX = SCREEN + measureCount * MEASURE + ( int )(note.Position * (( double )MEASURE / measure.Size));
                    kind = 10 * Note.RoleToInt(note.Note.Role) + Math.Min(measure.Size / note.Duration, MAXID);
                    enemies.Add(new Tuple<int, int>(posX, kind));
                }

                measureCount++;
            }

            return enemies;
        }

        public static List<int> Abyss(Score music, List<int> baseHeights, bool mostActive) {
            List<int> final = baseHeights;

            // Encontra parte (instrumento) menos ativo (que toca menos notas)
            Part part;
            if (mostActive) {
                part = music.MostActive();
            } else {
                part = music.LeastActive();
            }

            if (part == null) {
                throw (new Exception("Falha em MusicScore.Score.LeastActive() ou MusicScore.Score.MostActive()"));
            }

            // Varra cada nota (ou base de acorde) gerando novos abismos
            int measureCount = 0;
            int start;
            int finish;
            
            foreach (var measure in part.Measures) {
                foreach (var note in measure.Elements) {
                    if (note.Type == MeasureElementType.Note || note.Type == MeasureElementType.ChordBase) {
                        if (note.Duration > measure.Size / 2) {
                            start = SCREEN + measureCount * MEASURE + ( int )(note.Position * (( double )MEASURE / measure.Size));
                            finish = start + ( int )(16 * (( double )note.Duration / (2 * measure.Size)));
                            for (int xx = start; xx <= finish; xx++) {
                                if (final[xx] >= 0) {
                                    final[xx] = 0;
                                } else {
                                    final[xx] = baseHeights[xx - MEASURE];
                                }
                            }
                        }
                    }
                }

                measureCount++;
            }

            return final;
        }

        public static List<int> MeasureToGrid(List<int> baseHeights) {
            List<int> gridHeights = new List<int>(baseHeights.Capacity * MEASURE);

            // Adiciona tela extra de início
            for (int gg = 0; gg < SCREEN; gg++) {
                gridHeights.Add(BASEHEIGHT);
            }

            // Calcula altura finais a partir dos "delta" em baseHeights
            int height;
            foreach (int h in baseHeights) {
                height = gridHeights.Last() + h;
                for (int gg = 0; gg < MEASURE; gg++) {
                    gridHeights.Add(height);
                }
            }

            // Adiciona tela extra de fim
            for (int gg = 0; gg < SCREEN; gg++) {
                gridHeights.Add(gridHeights.Last());
            }

            return gridHeights;
        }

        public static List<int> SetHeights(Score music) {

            int measureCount = music.Parts[0].Measures.Count; // Total de compassos da música
            int partCount = music.Parts.Count; // Total de partes (instrumentos) da música
            List<int> heights = new List<int>(measureCount + 4);

            //int ratio = ( int )Math.Ceiling(( double )SCREEN / MEASURE);
            //for (int ii = 0; ii < ratio; ii++) {
            //    heights.Add(BASEHEIGHT);
            //}

            //heights.Add(BASEHEIGHT); // Primeira tela 1 de 2
            //heights.Add(BASEHEIGHT); // Priemira tela 2 de 2

            // Auxiliares para o loop
            Measure msr;
            int delta = 0; // Contibuição da parte
            int sum = 0; // Soma dos delta

            for (int mm = 0; mm < measureCount; mm++) {
                sum = 0;

                for (int pp = 0; pp < partCount; pp++) {
                    delta = 0;
                    msr = music.Parts[pp].Measures[mm];

                    foreach (var elmnt in msr.Elements) {
                        if (elmnt.Type == MeasureElementType.Rest) {
                            continue;
                        }
                        switch (elmnt.Note.Role) {
                            case Scale.Tonic:
                                if (delta < 0) {
                                    delta += elmnt.Duration;
                                } else if (delta > 0) {
                                    delta -= elmnt.Duration;
                                }
                                break;

                            case Scale.Supertonic:
                                if (delta >= 0) {
                                    delta += elmnt.Duration;
                                }
                                break;

                            case Scale.Mediant:
                                if (delta < 0) {
                                    delta += elmnt.Duration;
                                }
                                break;

                            case Scale.Subdominant:
                                delta -= elmnt.Duration;
                                break;

                            case Scale.Dominant:
                                delta += elmnt.Duration;
                                break;

                            case Scale.Submediant:
                                if (delta > 0) {
                                    delta -= elmnt.Duration;
                                }
                                break;

                            case Scale.Subtonic:
                                if (delta <= 0) {
                                    delta -= elmnt.Duration;
                                }
                                break;

                            default:
                                break;
                        }
                    }
                    sum += (5 * delta) / msr.Size;
                }
                heights.Add(sum / partCount);
                //Console.WriteLine(heights.Last());
            }

            //for (int ii = 0; ii < ratio; ii++) {
            //    heights.Add(heights.Last());
            //}

            //Console.WriteLine("Compassos adicionados ao início e fim : {0}", ratio);

            //heights.Add(heights.Last()); // Área final 1 de 2
            //heights.Add(heights.Last()); // Área final 2 de 2

            return heights;
        }

        public static void DrawMap(string name, List<int> heights, bool drawGrid = true, bool drawMeasureBounds = true, bool drawScreenBounds = true) {
            
            // Auxiliares para a imagem
            int width = heights.Count * GRID; // Largura da imagem gerada
            int height = (heights.Max() + 2 * SCREEN) * GRID;
            int posX = 0;
            using (Bitmap map = new Bitmap(width, height)) {
                using (Graphics g = Graphics.FromImage(map)) {
                    DrawBackGround(g, height, heights.Count - 2 * SCREEN);
                    foreach (var h in heights) {
                        DrawColumn(g, posX, h, height);
                        posX++;
                    }
                    if (drawGrid) {
                        DrawGridOverlay(g, width, height);
                    }
                    if (drawMeasureBounds) {
                        DrawMeasureBounds(g, width, height);
                    }
                    if (drawScreenBounds) {
                        DrawScreenBounds(g, width, height);
                    }
                    map.Save(name + "[" + VERSION + "].png", ImageFormat.Png);
                }
            }
        }

        public static void DrawBackGround(Graphics graphics, int height, int musicWidth) {
            graphics.FillRectangle(Brushes.MistyRose, 0, 0, SCREEN * GRID, height); // Denota área inicial (padrão)
            graphics.FillRectangle(Brushes.Lavender, SCREEN * GRID, 0, musicWidth * GRID, height); // Denota área gerada pela música
            graphics.FillRectangle(Brushes.MistyRose, (musicWidth + SCREEN) * GRID, 0, SCREEN * GRID, height); // Denota área final (padrão)
        }

        public static void DrawColumn(Graphics graphics, int X, int columnHeight, int mapHeight) {
            graphics.FillRectangle(Brushes.ForestGreen, X * GRID, mapHeight - (columnHeight * GRID), GRID, columnHeight * GRID);
        }

        //public static void DrawPlatform(Graphics graphics, int X, int platformHeight, int mapHeight) {
        //    graphics.FillRectangle(Brushes.ForestGreen, X * GRID, mapHeight - (platformHeight * GRID), MEASURE * GRID, platformHeight * GRID);
        //}

        public static void DrawScreenBounds(Graphics graphics, int mapWidth, int mapHeight) {
            int X = 0;
            while (X <= mapWidth) {
                graphics.DrawLine(new Pen(Brushes.DarkBlue, 2), X, 0, X, mapHeight);
                X += SCREEN * GRID;
            }
        }

        public static void DrawMeasureBounds(Graphics graphics, int mapWidth, int mapHeight) {
            int X = 0;
            while (X <= mapWidth) {
                graphics.DrawLine(new Pen(Brushes.DarkRed, 2), X, 0, X, mapHeight);
                X += MEASURE * GRID;
            }
        }

        public static void DrawGridOverlay(Graphics graphics, int mapWidth, int mapHeight) {
            Pen line = new Pen(Brushes.Black, 1);
            // Linhas Verticais
            int X = 0;
            while (X <= mapWidth) {
                graphics.DrawLine(line, X, 0, X, mapHeight);
                X += GRID;
            }

            // Linhas Horizontais
            int Y = 0;
            while (Y <= mapHeight) {
                graphics.DrawLine(line, 0, Y, mapWidth, Y);
                Y += GRID;
            }
        }

    }
}
