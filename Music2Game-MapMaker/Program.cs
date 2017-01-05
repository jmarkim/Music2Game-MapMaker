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

        static void Main(string[] args) {

            // Representação da partitura
            Score music = ScoreBuilder.FromXML(args[0]);

            // Nome do arquivo (sem extensão)
            string name = args[0].Remove(args[0].Length - 4);

            // Sequencia de alturas das "telas"
            List<int> heights = SetHeights(music);

            // Desanha mapa, como definido pela sequência de alturas
            DrawMap(name, heights);
            
        }

        public static List<int> SetHeights(Score music) {

            int measureCount = music.Parts[0].Measures.Count; // Total de compassos da música
            int partCount = music.Parts.Count; // Total de partes (instrumentos) da música
            List<int> heights = new List<int>(measureCount + 4);

            heights.Add(BASEHEIGHT); // Primeira tela 1 de 2
            heights.Add(BASEHEIGHT); // Priemira tela 2 de 2

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
                heights.Add(heights.Last() + sum / partCount);
                //Console.WriteLine(heights.Last());
            }

            heights.Add(heights.Last()); // Área final 1 de 2
            heights.Add(heights.Last()); // Área final 2 de 2

            return heights;
        }

        public static void DrawMap(string name, List<int> heights) {
            
            // Auxiliares para a imagem
            int width = heights.Count * MEASURE * GRID; // Largura da imagem gerada
            int height = (heights.Max() + 2 * SCREEN) * GRID;
            int posX = 0;
            using (Bitmap map = new Bitmap(width, height)) {
                using (Graphics g = Graphics.FromImage(map)) {
                    DrawBackGround(g, height, heights.Count - 4);
                    foreach (var h in heights) {
                        DrawPlatform(g, posX, h, height);
                        posX += MEASURE;
                    }
                    DrawGridOverlay(g, width, height);
                    DrawMeasureBounds(g, width, height);
                    DrawScreenBounds(g, width, height);
                    map.Save(name + "[div][0.1.6].png", ImageFormat.Png);
                }
            }
        }

        public static void DrawBackGround(Graphics graphics, int height, int musicWidth) {
            graphics.FillRectangle(Brushes.MistyRose, 0, 0, SCREEN * GRID, height); // Denota área inicial (padrão)
            graphics.FillRectangle(Brushes.Lavender, SCREEN * GRID, 0, musicWidth * MEASURE * GRID, height); // Denota área gerada pela música
            graphics.FillRectangle(Brushes.MistyRose, musicWidth * MEASURE * GRID + SCREEN * GRID, 0, SCREEN * GRID, height); // Denota área final (padrão)
        }

        public static void DrawPlatform(Graphics graphics, int X, int platformHeight, int mapHeight) {
            graphics.FillRectangle(Brushes.ForestGreen, X * GRID, mapHeight - (platformHeight * GRID), MEASURE * GRID, platformHeight * GRID);
        }

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
