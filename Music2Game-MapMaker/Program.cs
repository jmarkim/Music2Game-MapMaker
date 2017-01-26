using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicScore;
using System.Drawing;
using System.Drawing.Imaging;
using LevelClasses;
using System.IO;
using System.IO.Compression;

namespace Music2Game_MapMaker {
    class Program {

        // Constantes
        private static int BASE_HEIGHT = 50; // Altura da primeira tela;
        private static int SCREEN = 32; // Tamanho em quadros da tela
        private static int MEASURE = 16; // Tamanho em quadros de um compasso
        private static int GRID = 10; // Lados do quadro do grid (em px)
        private static int MAXID = 9; // Quantidade máxima de desafios em cada categoria
        private static int DELTA_THRESHOLD = int.MaxValue;
        private static int MEASURE_FRACTION = 3; // Tamanho mínima de nota geradora de abismos (fração do tamanho do compasso)
        private static int MINIMUN_HEIGHT = 3; // Alutra mínima paa plataformas flutuantes
        private static string VERSION = "0.7.0";

        static void Main(string[] args) {
            //Variáveis
            string root = AppDomain.CurrentDomain.BaseDirectory; // Diretório da aplicação
            string musicPath = root + "Music"; // Diretório de músicas
            string[] files; // Arquivos no diretório de músicas
            string musicName;
            bool isMusicXML;
            FileInfo musicFile;
            StreamReader reader;
            Score musicScore;
            Level map;

            // Log (Console) de Características
            Console.WriteLine("Versão do algoitmo : {0}", VERSION);
            Console.WriteLine("Tamanho do quadro : {0}px", GRID);
            Console.WriteLine("Tamanho do compasso : {0} quadros", MEASURE);
            Console.WriteLine("Tamanho da tela : {0} quadros", SCREEN);

            Console.WriteLine();
            Console.WriteLine("Diretório base da aplicação : {0}", root);

            // Localiza diretório de músicas
            if (!System.IO.Directory.Exists(musicPath)) {
                Console.WriteLine("  Diretório de músicas inexistente ({0})", musicPath);
            }

            // Localiza ou cria diretório de Imagens
            if (!Directory.Exists(root + "Imagens")) {
                Directory.CreateDirectory(root + "Imagens");
            }

            // Localiza ou cria sub-diretório da versão atual
            if (!Directory.Exists(root + "Imagens\\" + VERSION)) {
                Directory.CreateDirectory(root + "Imagens\\" + VERSION);
            }

           // Localiza ou cria sub-diretório de Níveis em TXT
           if (!Directory.Exists(root + "Niveis")) {
                Directory.CreateDirectory(root + "Niveis\\");
            }

            // Localiza ou cria sub-diretório de Níveis em TXT da versão atual
            if (!Directory.Exists(root + "Niveis\\" + VERSION)) {
                Directory.CreateDirectory(root + "Niveis\\" + VERSION);
            }

            // Recupera músicas no diretório
            files = System.IO.Directory.GetFiles(musicPath);
            Console.WriteLine("Músicas encontradas :");

            // Itera sobre arquivos
            foreach (var music in files) {
                musicFile = new System.IO.FileInfo(music);

                // Checa se arquivo é musicXML
                if (musicFile.Extension == ".mxl") {
                    musicName = musicFile.Name.Remove(musicFile.Name.Length - 4);
                    Console.Write("   -> {0} ", musicName);

                    // musicXML é um arquivo compactado sob o padrão zip, iterar sobre os arquivos internos em busca do xml com a partitura
                    foreach (var entry in ZipFile.OpenRead(musicFile.FullName).Entries) {
                        isMusicXML = false;

                        // Identifica arquivos xml
                        if (entry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) {

                            // Confirma se o arquivo xml realmente é um musicXML, o .mxl também contém outro xml (\META-INF\container.xml)
                            reader = new StreamReader(entry.Open());
                            reader.ReadLine();
                            if (reader.ReadLine().Contains("score") ) {
                                isMusicXML = true;
                            }
                            reader.Close();

                            // Checa e deleta tempMusic.xml para evitar conflitos
                            if (File.Exists(musicPath + "\\tempMusic.xml")) {
                                File.Delete(musicPath + "\\tempMusic.xml");
                            }
                            
                            // Gera os arquivos de fase
                            if (isMusicXML) {
                                entry.ExtractToFile(musicPath + "\\tempMusic.xml");
                                musicScore = ScoreBuilder.FromXML(musicPath + "\\tempMusic.xml");
                                map = new Level();
                                map.BuildHeightsSequence(musicScore);
                                map.SaveImage(root + "Imagens\\" + VERSION + "\\" + musicName);
                                map.SaveText(root + "Niveis\\" + VERSION + "\\", musicName);
                                Console.Write(" >> Imagem criada");
                                System.IO.File.Delete(musicPath + "\\tempMusic.xml");
                            }

                        }

                    }

                    Console.WriteLine();
                }

            }
            //System.IO.DirectoryInfo musicDir = new System.IO.DirectoryInfo(musicPath);


            // Representação da partitura
            //Score music = ScoreBuilder.FromXML(args[0]);

            // Nome do arquivo (sem extensão)
            //string name = args[0].Remove(args[0].Length - 4);

            // Objeto Level
            //LevelClasses.Level map = new LevelClasses.Level(music);
            //map.SaveImage(name + "_final[" + VERSION + "]");

            // Métdo : Sequência de alturas por compasso
            //Console.WriteLine();
            //Console.WriteLine("Criando mapas utilizando o método: Sequência de alturas");
            //try {
            //Refrência Estática
            //HeightsSequence(music, name, true, true, true, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, true, true, false);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, true, false, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, true, false, false);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, false, true, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, false, true, false);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, false, false, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, true, false, false, false);
            //Console.WriteLine();

            //Referência Dinâmica
            //HeightsSequence(music, name, false, true, true, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, true, true, false);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, true, false, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, true, false, false);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, false, true, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, false, true, false);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, false, false, true);
            //Console.WriteLine();
            //HeightsSequence(music, name, false, false, false, false);
            //} catch (Exception ex) {
            //    Console.Error.WriteLine(ex.Message);
            //}
        }

        public static void HeightsSequence(Score music, string name, bool staticRoles, bool globalReference, bool restCount, bool mostActive) {

            if (staticRoles) {
                name += "_SR";
            } else {
                name += "_DR";
            }

            if (globalReference) {
                name += "_GR";
            } else {
                name += "_LR";
            }

            // Sequencia de alturas das "telas"
            List<int> baseHeights = SetHeights(music, staticRoles, globalReference);

            // Transforma a sequencia de alturas base (por compasso) em alturas "por quadro"
            List<int> gridHeights = MeasureToGrid(baseHeights);

            // Lista de desafios
            Console.Write("Adicionando desafios... ");
            List<Tuple<int, int>> enemies = SetEnemies(music);
            Console.WriteLine("OK");

            //foreach(var chl in enemies) {
            //    Console.WriteLine("{0} >> {1}", chl.Item1, chl.Item2);
            //}

            // Gera novos abismos (ou plataformas e regiões "abissais")
            if (restCount) {
                Console.WriteLine("   Usando pausas");
                name += "_RC";
            } else {
                Console.WriteLine("   Usando notas 'grandes'");
                name += "_BN";
            }
            if (mostActive) {
                Console.WriteLine("   Usando isntrumento mais ativo");
                name += "_ma";
            } else {
                Console.WriteLine("   Usando isntrumento menos ativo");
                name += "_la";
            }
            Console.Write("   Adicionando novos abismos... ");
            gridHeights = Abyss(music, gridHeights, mostActive, restCount);
            Console.WriteLine("OK");

            //Console.WriteLine("\n\ngridHeights({0}) : ");
            //foreach (var num in gridHeights) {
            //    Console.WriteLine("   {0}", num);
            //}

            // Desanha mapa, como definido pela sequência de alturas
            Console.Write("   Criando Imagem... ");
            DrawMap(name + "_HS", gridHeights, enemies);
            Console.WriteLine("OK");
        }

        public static List<Tuple<int, int>> SetEnemies(Score music) {
            List<Tuple<int, int>> enemies = new List<Tuple<int, int>>();
            Part part = music.MostActive();

            if (part == null) {
                throw new Exception("Falha em MusicScore.Score.MostActive()");
            }

            int measureCount = 0;
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

        public static List<int> Abyss(Score music, List<int> baseHeights, bool mostActive, bool restCount) {
            List<int> final = baseHeights;

            // Encontra parte (instrumento) menos ativo (que toca menos notas)
            Part part;
            if (mostActive) {
                part = music.MostActive(restCount);
            } else {
                part = music.LeastActive(restCount);
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
                    if (restCount) {
                        if (note.Type == MeasureElementType.Rest) {
                            start = SCREEN + measureCount * MEASURE + ( int )(note.Position * (( double )MEASURE / measure.Size));
                            finish = start + ( int )(16 * (( double )note.Duration / (2 * measure.Size)));
                            for (int xx = start; xx <= finish; xx++) {
                                if (final[xx] >= 0) {
                                    final[xx] = 0;
                                } else {
                                    final[xx] = 1;
                                }
                            }
                        }
                    } else {
                        if (note.Type == MeasureElementType.Note || note.Type == MeasureElementType.ChordBase) {
                            if (note.Duration > measure.Size / MEASURE_FRACTION) {
                                start = SCREEN + measureCount * MEASURE + ( int )(note.Position * (( double )MEASURE / measure.Size));
                                finish = start + ( int )(16 * (( double )note.Duration / (2 * measure.Size)));
                                for (int xx = start; xx <= finish; xx++) {
                                    if (final[xx] >= 0) {
                                        final[xx] = 0;
                                    } else {
                                        final[xx] = 1;
                                    }
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
                gridHeights.Add(BASE_HEIGHT);
            }

            // Calcula altura finais a partir dos "delta" em baseHeights
            int height;
            foreach (int h in baseHeights) {
                //Console.WriteLine("   !!! {0}", h);
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

        public static Scale IntToRole(int intRole) {
            switch (intRole) {
                case 0:
                    return Scale.Tonic;

                case 1:
                    return Scale.Supertonic;

                case 2:
                    return Scale.Mediant;

                case 3:
                    return Scale.Subdominant;

                case 4:
                    return Scale.Dominant;

                case 5:
                    return Scale.Submediant;

                case 6:
                    return Scale.Subtonic;

                default:
                    return Scale.NA;
            }
        }

        public static List<int> SetHeights(Score music, bool staticRoles, bool globalReference) {
            Scale Reference = Scale.Tonic;
            Scale Elevation = Scale.Dominant;
            Scale Depression = Scale.Subdominant;
            Scale ConditionalElevation = Scale.Supertonic;
            Scale ConditionalDepression = Scale.Subtonic;
            Scale NegativeCorrection = Scale.Mediant;
            Scale PositiveCorrection = Scale.Submediant;

            if (staticRoles) {
                Console.WriteLine("Funções estáticas :");
            } else {
                Console.WriteLine("Funções dinâmicas :");
                List<int> count = music.RoleCounts();
                int max;
                int maxId;

                Console.Write("      Contagem :");
                foreach (var num in count) {
                    Console.Write(" {0}", num);
                }

                // Referência >> Δh += X, se Δh < 0 .OU. Δh -= X, se Δh > 0
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                Reference = IntToRole(maxId); // Define nota da escala responsável pela função

                // Elevação >> Δh += X
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                Elevation = IntToRole(maxId); // Define nota da escala responsável pela função

                // Depressão >> Δh -= X
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                Depression = IntToRole(maxId); // Define nota da escala responsável pela função

                // Elevação Condicional >> Δh += X, se Δh >= 0
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                ConditionalElevation = IntToRole(maxId); // Define nota da escala responsável pela função

                // Depressão Condicional >> Δh -= X, Δh <= 0
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                ConditionalDepression = IntToRole(maxId); // Define nota da escala responsável pela função

                // Correção Negativa >> Δh += X, se Δh < 0
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                NegativeCorrection = IntToRole(maxId); // Define nota da escala responsável pela função

                // Correção Positiva >> Δh -= X, se Δh > 0
                max = count.Max(); // Busca papel de maior ocorrência
                maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
                count[maxId] = -1; // Remove da próxima iteração
                PositiveCorrection = IntToRole(maxId); // Define nota da escala responsável pela função

            }

            Console.WriteLine();
            Console.WriteLine("   Referência .......... : {0}", Reference);
            Console.WriteLine("   Elevação ............ : {0}", Elevation);
            Console.WriteLine("   Depressão ........... : {0}", Depression);
            Console.WriteLine("   Elevação Condicional  : {0}", ConditionalElevation);
            Console.WriteLine("   Depressão Condicional : {0}", ConditionalDepression);
            Console.WriteLine("   Correção Negativa ... : {0}", NegativeCorrection);
            Console.WriteLine("   Correção Positiva ... : {0}", PositiveCorrection);

            if (globalReference) {
                Console.WriteLine("Usando referência global");
            } else {
                Console.WriteLine("Usando referência local");
            }

            int measureCount = music.Parts[0].Measures.Count; // Total de compassos da música
            int partCount = music.Parts.Count; // Total de partes (instrumentos) da música
            List<int> heights = new List<int>(measureCount);

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
                    if (globalReference) {
                        if (heights.Count() != 0) {
                            delta = heights.Last();
                        } else {
                            delta = 0;
                        }
                    } else { 
                        delta = 0;
                    }
                    msr = music.Parts[pp].Measures[mm];

                    foreach (var elmnt in msr.Elements) {
                        if (elmnt.Type == MeasureElementType.Rest) {
                            continue;
                        }
                        if (elmnt.Note.Role == Reference) {
                            if (delta < 0) {
                                delta += (elmnt.Duration * (Math.Abs(delta) / DELTA_THRESHOLD + 1));
                            } else if (delta > 0) {
                                delta -= (elmnt.Duration * (Math.Abs(delta) / DELTA_THRESHOLD + 1));
                            }

                        } else if (elmnt.Note.Role == Elevation) {
                            delta += (elmnt.Duration / (Math.Abs(delta) / DELTA_THRESHOLD + 1));

                        } else if (elmnt.Note.Role == Depression) {
                            delta -= (elmnt.Duration / (Math.Abs(delta) / DELTA_THRESHOLD + 1));

                        } else if (elmnt.Note.Role == ConditionalElevation) { 
                            if (delta >= 0) {
                                delta += (elmnt.Duration / (Math.Abs(delta) / DELTA_THRESHOLD + 1));
                            }

                        } else if (elmnt.Note.Role == ConditionalDepression) {
                            if (delta <= 0) {
                                delta -= (elmnt.Duration / (Math.Abs(delta) / DELTA_THRESHOLD + 1));
                            }

                        } else if (elmnt.Note.Role == NegativeCorrection) { 
                            if (delta < 0) {
                                delta += (elmnt.Duration * (Math.Abs(delta) / DELTA_THRESHOLD + 1));
                            }

                        } else if (elmnt.Note.Role == PositiveCorrection) { 
                            if (delta > 0) {
                                delta -= (elmnt.Duration * (Math.Abs(delta) / DELTA_THRESHOLD + 1));
                            }
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

        public static void DrawMap(string name, List<int> heights, List<Tuple<int, int>> challenges, bool drawGrid = true, bool drawMeasureBounds = true, bool drawScreenBounds = true) {
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
                    foreach (var chllng in challenges) {
                        DrawChallenge(g, chllng.Item1, chllng.Item2, heights[chllng.Item1], height);
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

        public static void DrawChallenge(Graphics graphics, int X, int kind, int platformHeight, int mapHeight) {
            if (platformHeight <= 0) {
                return;
            }

            Brush colorCode;
            switch (kind / 10) {
                case 1:
                    colorCode = Brushes.Orange;
                    break;

                case 2:
                    colorCode = Brushes.PaleVioletRed;
                    break;

                case 3:
                    colorCode = Brushes.LightYellow;
                    break;

                case 4:
                    colorCode = Brushes.DarkRed;
                    break;

                case 5:
                    colorCode = Brushes.DarkSlateGray;
                    break;

                case 6:
                    colorCode = Brushes.LightSlateGray;
                    break;

                default:
                    colorCode = Brushes.Lavender;
                    break;
            }
            graphics.FillRectangle(colorCode, X * GRID, mapHeight - (platformHeight + 1) * GRID, GRID, GRID);
            graphics.DrawLine(new Pen(Brushes.Black, 1), X * GRID, mapHeight - (platformHeight + 1) * GRID, X * GRID + GRID, mapHeight - (platformHeight + 1) * GRID + GRID);
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
