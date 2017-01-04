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
        static void Main(string[] args) {
            const int SCREEN = 32; // Tamanho em quadros da tela
            const int MEASURE = 16; // Tamanho em quadros de um compasso
            const int BASEHEIGHT = 10; // Altura da primeira tela;
            const int GRID = 5; // Lados do quadro do grid (em px)

            // Representação da partitura
            Score music = ScoreBuilder.FromXML(args[0]);

            // Nome do arquivo (sem extensão)
            string name = args[0].Remove(args[0].Length - 4);

            // Sequencia de alturas das "telas"
            List<int> heights = new List<int>(music.Parts[0].Measures.Count + 4);
            heights.Add(BASEHEIGHT); // Primeira tela 1 de 2
            heights.Add(BASEHEIGHT); // Priemira tela 2 de 2

            // Auxiliares para o loop
            int measureCount = music.Parts[0].Measures.Count; // Total de compassos da música
            int partCount = music.Parts.Count; // Total de partes (instrumentos) da música
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
                heights.Add(heights.Last() + sum);
            }

            heights.Add(heights.Last()); // Área final 1 de 2
            heights.Add(heights.Last()); // Área final 2 de 2

            // Auxiliares para a imagem
            int width = heights.Count * MEASURE * GRID; // Largura da imagem gerada
            int height = (heights.Max() + 2 * SCREEN) * GRID;
            int posX = 0; 
            using (Bitmap map = new Bitmap(width, height)) {
                using (Graphics g = Graphics.FromImage(map)) {
                    g.FillRectangle(Brushes.GhostWhite, 0, 0, SCREEN * GRID, height); // Denota área inicial (padrão)
                    g.FillRectangle(Brushes.WhiteSmoke, SCREEN * GRID, 0, width, height); // Denota área gerada pela música
                    g.FillRectangle(Brushes.GhostWhite, width - (SCREEN * GRID), 0, width, height); // Denota área final (padrão)

                    foreach (var h in heights) {
                        g.FillRectangle(Brushes.ForestGreen, posX * GRID, height - (h * GRID), MEASURE * GRID, h * GRID);
                        posX += MEASURE;
                    }

                    map.Save(name + ".png", ImageFormat.Png);
                }
            }
        }
    }
}
