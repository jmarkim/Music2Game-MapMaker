using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicScore;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LevelClasses {
    public class Level
    {
        // Definições globais
        internal static int MEASURE_SIZE = 16; // Tamanho de um compasso em quadros
        internal static int SCREEN_SIZE = 32; // Tamanho de uma tela em quadros (horizontal)
        internal static int CEILING_HEIGHT = 40; // Altura do teto em relação à plataforma mais alta
        internal static int HEIGHT_MODIFIER = 5; // Valor pelo qual "delta" é multiplicado antes de dividido pelo tamanho do compasso
        internal static int MAXIMUM_WIDTH = 16; // Largura máxima (soma) de abismos em uma seção
        internal static int MINIMUM_WIDTH = 4; // Largura mínima (soma) de abismos em um seção
        internal static int ABYSS_THRESHOLD = 8; // Largura mínima de uma abismo para que seja coberto por blocos que caem
        internal static double ABYSS_CHANCE = 0.3; // Define o limiar de intensidade para geração de abismos

        // Definições para geração de Geografia
        internal static int MAXIMUM_HEIGHT = 60; // Altura máxima para plataformas
        internal static int MINIMUM_HEIGHT = 2; // Altura mínima para plataformas
        internal static int STARTING_HEIGHT = 30; // Altura da primeira plataforma
        internal static int DURATION_TO_WIDTH_FACTOR = 2; // Multiplica duração da nota no cálculo de largura relativa, cálculo de novas plataformas sólidas
        internal static int DURATION_TO_HEIGHT_FACTOR = 1; // Divide duração da nota no cálculo de largura relativa, cálculo de  delta-alturas para formação de rampas

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

        //// Lista de abismos
        //private List<Abyss> _abysses;
        //public List<Abyss> Abysses {
        //    get { return _abysses; }
        //}

        // Lista e desafios da fase (inimigos, abismos, obstáculos, etc)
        private List<Challenge> _challenges;
        public List<Challenge> Challenges {
            get { return _challenges; }
        }

        // Construtor a partir de uma música
        //public void BuildHeightsSequence(Score music) {
        //    _width = MEASURE_SIZE * music.MeasureCount;
        //    _geography = new List<int>(_width);
        //    _challenges = new List<Challenge>();
        //    _abysses = new List<Abyss>();
        //    HSGeography(music);
        //    HSChallenges(music);
        //    _height = _geography.Max() + CEILING_HEIGHT;
        //}

        internal class Builder {
            public int abyssCount;
            public int enemyCount;
            public int powerupCount;
            public int height;
            public int lastSectionStart;
            public int lastSectionWidth;
            public int nextObstaclePosition;
            public bool lastSectionHeightIsMdodifiable;
            public bool firstSectionBuilt;
            public bool abyssFlag;
            static Builder instance;

            public void Reset() {
                height = STARTING_HEIGHT;
                abyssCount = 0;
                enemyCount = 0;
                powerupCount = 0;
                lastSectionStart = 0;
                lastSectionWidth = 0;
                nextObstaclePosition = 0;
                lastSectionHeightIsMdodifiable = true;
                firstSectionBuilt = false;
                abyssFlag = false;
            }

            public  static Builder Instance {
                get {
                    if (instance == null) {
                        instance = new Builder();
                    }
                    return instance;
                }
            }

            private Builder() {
                height = STARTING_HEIGHT;
                abyssCount = 0;
                enemyCount = 0;
                powerupCount = 0;
                lastSectionStart = 0;
                lastSectionWidth = 0;
                nextObstaclePosition = 0;
                lastSectionHeightIsMdodifiable = true;
                firstSectionBuilt = false;
                abyssFlag = false;
            }

            public void ToogleAbyss() {
                if (abyssFlag) {
                    abyssFlag = false;
                } else {
                    abyssFlag = true;
                    firstSectionBuilt = false;
                }
            }

            public void CreateSection(Level level, int width) {
                lastSectionStart += lastSectionWidth;
                lastSectionWidth = width;
                nextObstaclePosition = 0;

                for (int x = 0; x < width; x++) {
                    level.Geography.Add(height);
                }

                if (!firstSectionBuilt) {
                    firstSectionBuilt = true;
                }
                lastSectionHeightIsMdodifiable = true; // Habilida alteração de altura para a seção recém criada

                if (width == 0) {
                    Console.WriteLine("   !! >> Seção de largura 0  << !!");
                }
            }

            public void ModifySection(Level level, int deltaHeight) {
                // Transforma região em rampa
                lastSectionHeightIsMdodifiable = false; // Bloqueia novas modificações
                deltaHeight = Math.Min(MAXIMUM_HEIGHT - height, Math.Max(MINIMUM_HEIGHT - height, deltaHeight)); // Limita height à [MIN, MAX]

                // Atualiza alturas da última seção gerada
                for (int x = 1; x < lastSectionWidth; x++) {
                    level.Geography[lastSectionStart + x] += deltaHeight * x / lastSectionWidth;
                }

                // Atualiza altura
                height += deltaHeight;
            }

            public void CreateMovingPlatform(Level level, int width, int moveRange, int leftMargin, int rightMargin, bool verticalMove, bool risingMove = false) {
                lastSectionStart += lastSectionWidth;
                lastSectionWidth = width + leftMargin + rightMargin;

                if (!verticalMove) {
                    // Movimento horizontal, adiciona largura do movimento à largura da seção
                    lastSectionWidth += moveRange;
                }

                // Acrescenta 0s à _geography afim de controlar o tamanho da imagem
                for (int x = 0; x < lastSectionWidth; x++) {
                    level.Geography.Add(0);
                }

                level.Challenges.Add(new MovingPlatform(SCREEN_SIZE + lastSectionStart + leftMargin, height, width, moveRange, verticalMove, risingMove));

                if (verticalMove) {
                    // Movimento vertical, atualiza altura
                    if (risingMove) {
                        height = Math.Min(MAXIMUM_HEIGHT, height + moveRange);
                    } else {
                        height = Math.Max(MINIMUM_HEIGHT, height - moveRange);
                    }
                }

                if (width == 0) {
                    Console.WriteLine("   !! >> Plataforma móvel de largura 0  << !! {0} + {1} + {2} ({3} - {4})", leftMargin, width, rightMargin, verticalMove, risingMove);
                }
            }

            public void CreateFloatingPlatform(Level level, int width, int leftMargin, int rigthMargin, bool isFallingBlock) {
                lastSectionStart += lastSectionWidth;
                lastSectionWidth = width + leftMargin + rigthMargin;

                // Acrescenta 0s à _geography a fim de controlar o tamanho da imagem
                for (int x = 0; x < lastSectionWidth; x++) {
                    level.Geography.Add(0);
                }

                // Adiciona a plataforma à coleção _challenges
                level.Challenges.Add(new FloatPlatform(SCREEN_SIZE + lastSectionStart + leftMargin, height, width, isFallingBlock));

                if (!isFallingBlock) { // Após a geração da 2ª plataforma flutuante "fixa" abyssFlag é desligada
                    abyssCount++;
                    if (abyssCount > 1) {
                        abyssCount = 0;
                        ToogleAbyss();
                    }
                }

                if (width == 0) {
                    Console.WriteLine("   !! >> Plataforma de largura 0  << !! {0} + {1} + {2} ({3})", leftMargin, width, rigthMargin, isFallingBlock);
                }
            }

            public void CreateAbyss(Level level, int offset, int size, int width) {

                int x = lastSectionWidth * offset / size ;
                if (x < nextObstaclePosition) {
                    return;
                }

                // Atualiza alturas
                for (int w = 0; w < width; w++) {
                    level.Geography[Math.Min(lastSectionStart + x + w, level.Geography.Count - 1)] = 0;
                }

                nextObstaclePosition += width + 1;
                lastSectionHeightIsMdodifiable = false;
            }

            public void CreateObstacle(Level level, int offset, int size, int width, int height) {
                int x = Math.Max(0, Math.Min(lastSectionWidth * offset / size, lastSectionWidth - 2 ));
                if (x < nextObstaclePosition || lastSectionWidth < 2) {
                    return;
                }
                // Checa largura do obstáculo (se w > 2, gera abismo)
                if (width > 2) {
                    // Gera abismo
                    for (int w = 0; w < width; w++) {
                        level.Geography[Math.Min(lastSectionStart + x + w, level.Geography.Count - 1)] = 0;
                    }
                    nextObstaclePosition += width + 1;
                } else {
                    // Gera obstáculo
                    level.Challenges.Add(new Obstacle(SCREEN_SIZE + lastSectionStart + x, level.Geography[lastSectionStart + x], 2, (height % 3) + 1));
                    nextObstaclePosition += width + 2;
                }

                lastSectionHeightIsMdodifiable = false;
            }

            public void CreateGroundedEnemy(Level level, int offset, int size, int strength) {
                int x = Math.Min(lastSectionWidth * offset / size, level.Geography.Count - 1);
                if (x < nextObstaclePosition) {
                    return;
                }

                nextObstaclePosition += 2;
                lastSectionHeightIsMdodifiable = false;

                if (enemyCount % 3 == 0) {
                    level.Challenges.Add(new ActiveEnemy(SCREEN_SIZE + lastSectionStart + x, level.Geography[lastSectionStart + x] + 1, strength % 10, false));
                }
                enemyCount++;
            }

            public void CreateFlierEnemy(Level level, int offset, int altitude, int size, int strength) {
                int x = Math.Min(lastSectionWidth * offset / size, level.Geography.Count - 1);
                lastSectionHeightIsMdodifiable = false;

                int y = Math.Max(height, level.Geography[lastSectionStart + x]);

                if (enemyCount % 3 == 0) {
                    level.Challenges.Add(new PassiveEnemy(SCREEN_SIZE + lastSectionStart + x, y + altitude, strength % 10, true));
                }
                enemyCount++;
            }

            public void CreatePowerUp(Level level, int offset, int size, int strength) {
                int x = Math.Min(lastSectionWidth * offset / size, level.Geography.Count - 1);
                lastSectionHeightIsMdodifiable = false;

                if (powerupCount % 5 == 0) {
                    level.Challenges.Add(new PowerUp(SCREEN_SIZE + lastSectionStart + x, Math.Max(height, level.Geography[lastSectionStart + x]) + 4, strength % 10));
                }

                powerupCount++;
            }
        }

        public void SingleLoop(Part instrument, List<Scale> parts) {
            Scale createPlatform = parts[0];
            Scale raiseHeight = parts[3];
            Scale lowerHeight = parts[4];
            Scale toogle = parts[6];
            Scale obstacle = parts[1];
            Scale ground = parts[2];
            Scale aerial = parts[5];

            int size;
            foreach (var msr in instrument.Measures) {
                size = msr.Size;

                foreach (var note in msr.Elements) {
                    if (note.IsChord()) { // Ignora pausas e notas de acorde
                        continue;
                    }

                    if (note.IsRest()) {
                        if (Builder.Instance.firstSectionBuilt || _challenges.Count > 0) {
                            Builder.Instance.CreatePowerUp(this, note.Position, size, note.Duration);
                        }

                    } else {

                        Scale role = note.Note.Role;
                        if (role == toogle) {
                            Builder.Instance.ToogleAbyss();
                        } else if (role == createPlatform) {
                            if (Builder.Instance.abyssFlag) {
                                // Gera plataforma flutuante fixa
                                int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                int offset = MEASURE_SIZE * note.Position / size;
                                int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                Builder.Instance.CreateFloatingPlatform(this, width, lmargin, rmargim, false);
                            } else {
                                // Gera plataforma sólida
                                int width = Math.Max(1, DURATION_TO_WIDTH_FACTOR * MEASURE_SIZE * note.Duration / size);
                                Builder.Instance.CreateSection(this, width);
                            }

                        } else if (role == raiseHeight) {
                            if (Builder.Instance.abyssFlag) {
                                // Gera plataforma móvel vertical (subindo)
                                int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                int offset = MEASURE_SIZE * note.Position / size;
                                int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                int range = Math.Max(5 * Math.Abs(note.Note.Tone) + 5 - width / 2, 5);
                                Builder.Instance.CreateMovingPlatform(this, width, range, lmargin, rmargim, true, true);
                            } else {
                                if (Builder.Instance.firstSectionBuilt) {
                                    if (Builder.Instance.lastSectionHeightIsMdodifiable) {
                                        // Modifica altura
                                        int delta = MEASURE_SIZE * note.Duration / size;
                                        Builder.Instance.ModifySection(this, delta);
                                    } else {
                                        // Gera plataforma sólida
                                        int width = Math.Max(1, DURATION_TO_WIDTH_FACTOR * MEASURE_SIZE * note.Duration / size);
                                        Builder.Instance.CreateSection(this, width);
                                    }
                                } else {
                                    // Gera plataforma móvel vertical (subindo)
                                    int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                    int offset = MEASURE_SIZE * note.Position / size;
                                    int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                    int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                    int range = Math.Max(5 * Math.Abs(note.Note.Tone) + 5 - width / 2, 5);
                                    Builder.Instance.CreateMovingPlatform(this, width, range, lmargin, rmargim, true, true);
                                }
                            }

                        } else if (role == lowerHeight) {
                            if (Builder.Instance.abyssFlag) {
                                // Gera plataforma móvel vertical (descendo)
                                int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                int offset = MEASURE_SIZE * note.Position / size;
                                int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                int range = Math.Max(3 * Math.Abs(note.Note.Tone) + 3 - width / 2, 3);
                                Builder.Instance.CreateMovingPlatform(this, width, range, lmargin, rmargim, true, false);
                            } else {
                                if (Builder.Instance.firstSectionBuilt) {
                                    if (Builder.Instance.lastSectionHeightIsMdodifiable) {
                                        // Modifica altura
                                        int delta = MEASURE_SIZE * note.Duration / size;
                                        Builder.Instance.ModifySection(this, -delta);
                                    } else {
                                        // Gera plataforma sólida
                                        int width = Math.Max(1, DURATION_TO_WIDTH_FACTOR * MEASURE_SIZE * note.Duration / size);
                                        Builder.Instance.CreateSection(this, width);
                                    }
                                } else {
                                    // Gera plataforma móvel vertical (descendo)
                                    int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                    int offset = MEASURE_SIZE * note.Position / size;
                                    int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                    int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                    int range = Math.Max(3 * Math.Abs(note.Note.Tone) + 3 - width / 2, 3);
                                    Builder.Instance.CreateMovingPlatform(this, width, range, lmargin, rmargim, true, false);
                                }
                            }

                        } else if (role == obstacle) {
                            if (Builder.Instance.abyssFlag) {
                                // Gera plataforma flutuante frágil
                                int width = Math.Max(1, MEASURE_SIZE * note.Duration / size / DURATION_TO_WIDTH_FACTOR);
                                int offset = MEASURE_SIZE * note.Position / size;
                                int lmargin = Math.Max(0, Math.Min(4, offset / 2 + note.Note.Tone));
                                int rmargim = Math.Max(0, Math.Min(4, Math.Abs(note.Note.Tone)));
                                Builder.Instance.CreateFloatingPlatform(this, width, lmargin, rmargim, true);
                            } else {
                                if (Builder.Instance.firstSectionBuilt) {
                                    if (Builder.Instance.lastSectionHeightIsMdodifiable) {
                                        // plataforma plana -> obstáculo
                                        int width = MEASURE_SIZE * note.Duration / size;
                                        int height = MEASURE_SIZE * note.Duration / size / DURATION_TO_HEIGHT_FACTOR;
                                        Builder.Instance.CreateObstacle(this, note.Position, size, width, height);
                                    } else {
                                        // plataforma inclinada -> abismo
                                        int width = MEASURE_SIZE * note.Duration / size;
                                        Builder.Instance.CreateAbyss(this, note.Position, size, width);
                                    }
                                } else {
                                    // Gera plataforma flutuante frágil
                                    int width = Math.Max(1, MEASURE_SIZE * note.Duration / size / DURATION_TO_WIDTH_FACTOR);
                                    int offset = MEASURE_SIZE * note.Position / size;
                                    int lmargin = Math.Max(0, Math.Min(4, offset / 2 + note.Note.Tone));
                                    int rmargim = Math.Max(0, Math.Min(4, Math.Abs(note.Note.Tone)));
                                    Builder.Instance.CreateFloatingPlatform(this, width, lmargin, rmargim, true);
                                }
                            }

                        } else if (role == ground) {
                            if (Builder.Instance.abyssFlag) {
                                // Gera plataforma móvel horizontal
                                int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                int offset = MEASURE_SIZE * note.Position / size;
                                int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                int range = Math.Max(5 * Math.Abs(note.Note.Tone) + 5 - width / 2, 3);
                                Builder.Instance.CreateMovingPlatform(this, width, range, lmargin, rmargim, false, false);
                            } else {
                                if (Builder.Instance.firstSectionBuilt) {
                                    // Gera inimigo terrestre
                                    Builder.Instance.CreateGroundedEnemy(this, note.Position, size, note.Duration);
                                } else {
                                    // Gera plataforma móvel horizontal
                                    int width = Math.Max(1, MEASURE_SIZE * note.Duration / size);
                                    int offset = MEASURE_SIZE * note.Position / size;
                                    int lmargin = Math.Max(1, Math.Min(4, offset / 2 + note.Note.Tone));
                                    int rmargim = Math.Max(1, Math.Min(4, Math.Abs(note.Note.Tone)));
                                    int range = Math.Max(5 * Math.Abs(note.Note.Tone) + 5 - width / 2, 3);
                                    Builder.Instance.CreateMovingPlatform(this, width, range, lmargin, rmargim, false, false);
                                }
                            }

                        } else if (role == aerial) {
                            if (Builder.Instance.firstSectionBuilt || _challenges.Count > 0) {
                                // Gera inimigo voador
                                int altitude = Math.Min(3 + 2 * Math.Abs(note.Note.Tone) + 2, 10);
                                Builder.Instance.CreateFlierEnemy(this, note.Position, altitude, size, note.Duration);
                            }
                        }
                    }
                }
            }
        }

        public void BuildSingleLoop(Score music, string rootPath, string name, string ver) {
            List<Scale> orderedScale;
            int validMaps = 0;

            foreach (var part in music.Parts) {
                Builder.Instance.Reset();
                _geography = new List<int>();
                _challenges = new List<Challenge>();
                orderedScale = orderRoles(part);

                // Validação e preparação para a fase
                if (orderedScale.Count < 7) { // Checa se toda nota da escala está presente no instrumento
                    Console.WriteLine();
                    Console.WriteLine("   ! Instrumento não contém toda a escala musical");
                    continue;
                }

                SingleLoop(part, orderedScale);


                //Console.WriteLine();
                //Console.WriteLine("1ª Cria plataforma : {0}", orderedScale[0]);
                //Console.WriteLine("2ª Obstáculo : {0}", orderedScale[1]);
                //Console.WriteLine("3ª : {0}", orderedScale[2]);
                //Console.WriteLine("4ª Elevação : {0}", orderedScale[3]);
                //Console.WriteLine("5ª Depressão : {0}", orderedScale[4]);
                //Console.WriteLine("6ª : {0}", orderedScale[5]);
                //Console.WriteLine("7ª Liga/Desliga plataforma: {0}", orderedScale[6]);

                ///TODO : Criar funções, evitar codificar rotinas dentro do switch

                // Construção
                //int sectionStart = 0; // Início da última seção gerada
                //int sectionWidth = 0; // Largura da última seção gerada
                //int height = STARTING_HEIGHT;
                //bool platform = true; // Criando plataformas? se falso o algorítimo gera áreas de plataformas flutuantes
                //bool heightModifier = true; // Define se a última seção pode ter sua altura modificada
                //int measureSize = 0; // Tamanho do compasso



                // Outras informações
                _width = _geography.Count;
                _height = _geography.Max() + CEILING_HEIGHT;

                Console.WriteLine("Mapa {0} gerado", validMaps + 1);
                SaveImage(rootPath +"Imagens\\" +  ver + "\\" + name + '[' + ( char )('a' + validMaps / 26) + ( char )('a' + validMaps % 26) + ']');
                SaveText(rootPath + "Niveis\\" + ver + "\\",  name + '[' + ( char )('a' + validMaps / 26) + ( char )('a' + validMaps % 26) + ']');
                validMaps++;
                //Console.WriteLine("   w: {0} , h: {1}", _width, _height);
                //foreach (var num in _geography) {
                //    Console.WriteLine("       {0}", num);
                //}
                Console.WriteLine();
            }   
        }

        public List<Scale> orderRoles(Part part) {
            List<Scale> roles = new List<Scale>();
            List<int> roleCount = new List<int>();

            roleCount.Add(part.CountRole(Scale.Tonic));
            roleCount.Add(part.CountRole(Scale.Supertonic));
            roleCount.Add(part.CountRole(Scale.Mediant));
            roleCount.Add(part.CountRole(Scale.Subdominant));
            roleCount.Add(part.CountRole(Scale.Dominant));
            roleCount.Add(part.CountRole(Scale.Submediant));
            roleCount.Add(part.CountRole(Scale.Subtonic));

            int max;
            int maxID;
            for (int ii = 0; ii < 7; ii++) {
                max = roleCount.Max();
                if (max == 0) { // Identifica instrumento de percursão (tambores ou baterias)
                    return roles;
                }
                maxID = roleCount.FindIndex(a => a == max);
                roleCount[maxID] = -1;
                roles.Add(Note.IntToRole(maxID));
            }

            return roles;
        }

        //public void HSGeography(Score music) {
        //    // Definição dos papéis
        //    Scale Reference;
        //    Scale Elevation;
        //    Scale Depression;
        //    Scale ConditionalElevation;
        //    Scale ConditionalDepression;
        //    Scale NegativeCorrection;
        //    Scale PositiveCorrection;

        //    // Contagem das notas da escala
        //    List<int> count = music.RoleCounts();

        //    int max; // Quantidade de notas da escala que mais aparece
        //    int maxId; // Nota da escala que mais aparece
        //    int measureCount = music.MeasureCount; // Quantidade de Compassos da música
        //    int partCount = music.PartCount; // Quantidade de Instrumentos da música
        //    int sum = 0; // Soma dos deltas (valor final de Δh)
        //    int delta; // Contribuição do instrumento (valor parcial de Δh)

        //    Measure currentMeasure; // Compasso sendo trabalhado

        //    // Referência >> Δh += X, se Δh < 0 .OU. Δh -= X, se Δh > 0
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Reference = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Elevação >> Δh += X
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Elevation = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Depressão >> Δh -= X
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Depression = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Elevação Condicional >> Δh += X, se Δh >= 0
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    ConditionalElevation = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Depressão Condicional >> Δh -= X, Δh <= 0
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    ConditionalDepression = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Correção Negativa >> Δh += X, se Δh < 0
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    NegativeCorrection = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Correção Positiva >> Δh -= X, se Δh > 0
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    PositiveCorrection = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    for (int measureNumber = 0; measureNumber < measureCount; measureNumber++) {
        //        sum = 0;

        //        for (int partNumber = 0; partNumber < partCount; partNumber++) {
        //            delta = _geography.LastOrDefault(); // !!! Na "versão original" o algoritmo ignorava o delta calculado pelo instrumento anterior
        //            currentMeasure = music.Parts[partNumber].Measures[measureNumber];

        //            foreach (var note in currentMeasure.Elements) {
        //                if (!note.IsRest()) {

        //                    if(note.Note.Role == Reference) {
        //                        if (delta < 0) {
        //                            delta += note.Duration;
        //                        } else if (delta > 0) {
        //                            delta -= note.Duration;
        //                        }

        //                    } else if (note.Note.Role == Elevation) {
        //                        delta += note.Duration;

        //                    } else if (note.Note.Role == Depression) {
        //                        delta -= note.Duration;

        //                    } else if (note.Note.Role == ConditionalElevation) {
        //                        if (delta >= 0) {
        //                            delta += note.Duration;
        //                        }

        //                    } else if (note.Note.Role == ConditionalDepression) {
        //                        if (delta <= 0) {
        //                            delta -= note.Duration;
        //                        }

        //                    } else if (note.Note.Role == NegativeCorrection) {
        //                        if (delta < 0) {
        //                            delta += note.Duration;
        //                        }

        //                    } else if (note.Note.Role == PositiveCorrection) {
        //                        if (delta > 0) {
        //                            delta -= note.Duration;
        //                        }
        //                    }
        //                }
        //            }

        //            sum += (HEIGHT_MODIFIER * delta) / currentMeasure.Size;
        //        }

        //        _geography.Add(sum / partCount);
        //        for (int xx = 1; xx < MEASURE_SIZE; xx++) { // Gera toda a regão correspondente ao compasso
        //            _geography.Add(0);
        //        }
        //    }

        //    _geography[0] = STARTING_HEIGHT + _geography[0];
        //    for (int ii = 1; ii < _geography.Count; ii++) {
        //        _geography[ii] = _geography[ii - 1] + _geography[ii];
        //    }
        //}

        //internal void HSChallenges(Score music) {
        //    // Definição dos papéis
        //    Scale FloatingBlocks;
        //    Scale Obstacles;
        //    Scale PassiveEnemies;
        //    Scale Others;
        //    Scale ActiveEnemies;
        //    Scale Traps;
        //    Scale Abyss;

        //    // Contagem das notas da escala
        //    List<int> count = music.RoleCounts();
        //    int max; // Quantidade de notas da escala que mais aparece
        //    int maxId; // Nota da escala que mais aparece
        //    int measureCount = music.MeasureCount; // Quantidade de Compassos da música
        //    int partCount = music.PartCount; // Quantidade de Instrumentos da música
        //    List<int> sum = new List<int>(); // Soma dos deltas (valor final de Δh)
        //    List<MeasureElement> abyssNotes = new List<MeasureElement>();
        //    List<MeasureElement> ActiveEnemiesNotes = new List<MeasureElement>();
        //    List<MeasureElement> PassiveEnemiesNotes = new List<MeasureElement>();

        //    Measure currentMeasure = null; // Compasso sendo trabalhado

        //    // Blocos flutuantes - plataformas e blocos sólidos
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    FloatingBlocks = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Obstáculos - Pedras e outras estruturas que impedem o progresso do joador, inofensivos
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Obstacles = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Inimigos Passivos - Inimigos que não tentam causar dano ao jogador ativamente
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    PassiveEnemies = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Função ainda não atribuída - Paisagens e ecompenssas, talvez
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Others = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Inimigos Ativos - Inimigos que atacam o jogador ativamente
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    ActiveEnemies = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Armadilhas - Obstáculos que causam dano ao jogador descuidado
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Traps = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    // Abismos - Buracos extras
        //    max = count.Max(); // Busca papel de maior ocorrência
        //    maxId = count.FindIndex(num => num == max); // Recupera o índice do papel
        //    count[maxId] = -1; // Remove da próxima iteração
        //    Abyss = Note.IntToRole(maxId); // Define nota da escala responsável pela função

        //    for (int ii = 0; ii < 7; ii++) {
        //        sum.Add(0);
        //    }
        //    sum.TrimExcess();

        //    for (int measureNumber = 0; measureNumber < measureCount; measureNumber++) {
        //        for (int ii = 0; ii < 7; ii++) {
        //            sum[ii] = 0;
        //        }
        //        abyssNotes.Clear();
        //        ActiveEnemiesNotes.Clear();
        //        PassiveEnemiesNotes.Clear();

        //        for (int partNumer = 0; partNumer < partCount; partNumer++) {
        //            currentMeasure = music.Parts[partNumer].Measures[measureNumber];

        //            foreach (var note in currentMeasure.Elements) {
        //                if (!note.IsRest()) {

        //                    if (note.Note.Role == FloatingBlocks) {
        //                        sum[0] = note.Duration;
        //                    } else if (note.Note.Role == Obstacles) {
        //                        sum[1] += note.Duration;
        //                    } else if (note.Note.Role == PassiveEnemies) {
        //                        sum[2] += note.Duration;
        //                        PassiveEnemiesNotes.Add(note);
        //                    } else if (note.Note.Role == Others) {
        //                        sum[3] += note.Duration;
        //                    } else if (note.Note.Role == ActiveEnemies) {
        //                        sum[4] += note.Duration;
        //                        ActiveEnemiesNotes.Add(note);
        //                    } else if (note.Note.Role == Traps) {
        //                        sum[5] += note.Duration;
        //                    } else if (note.Note.Role == Abyss) {
        //                        sum[6] += note.Duration;
        //                        abyssNotes.Add(note);
        //                    }

        //                }
        //            }

        //        }

        //        // TODO : Processar resultados
        //        SetAbyss(measureNumber, currentMeasure.Size, partCount, sum[6], abyssNotes);
        //        //SetFloatingBlocks(measureNumber, currentMeasure.Size, sum[0])
        //        //SetObstacles(measureNumber, currentMeasure.Size, sum[1]);
        //        SetPassiveEnemies(measureNumber, currentMeasure.Size, partCount, sum[2], PassiveEnemiesNotes);
        //        //SetOthers(measureNumber, currentMeasure.Size, sum[3]);
        //        SetActiveEnemies(measureNumber, currentMeasure.Size, partCount, sum[4], ActiveEnemiesNotes);
        //        //SetTraps(measureNumber, currentMeasure.Size, sum[5]);
        //    }

        //}

        //internal void SetPassiveEnemies(int measureNumber, int measureSize, int partCount, int intensity, List<MeasureElement> sources) {
        //    // Quantas vezes a nota apareceu no trecho
        //    int occurrences = sources.Count;
        //    // Ocorrência média de notas por instrumento
        //    int perInstrument = occurrences / partCount;

        //    if (perInstrument < 1) {
        //        return;
        //    }

        //    int count = 0;
        //    int averagePosition = 0;
        //    int totalDuration = 0;
        //    int offset = 0;
        //    int measureMark = measureNumber * MEASURE_SIZE;
        //    int position;
        //    bool abyss;
        //    int kind;
        //    sources.Sort((a, b) => a.Position.CompareTo(b.Position));

        //    foreach (var sample in sources) {
        //        if (sample.Position >= offset && sample.Position < offset + measureSize / perInstrument) {
        //            // Acumula para tratamento
        //            count++;
        //            totalDuration += sample.Duration;
        //            averagePosition += sample.Position;
        //        } else {
        //            // Trata valores acumulados
        //            if (count > 0) {
        //                averagePosition /= count; // Média das posições iniciais
        //                kind = totalDuration / partCount;
        //                position = MEASURE_SIZE * averagePosition / measureSize;
        //                abyss = false;

        //                foreach (Abyss abs in _abysses) {
        //                    if (abs.PosX < measureMark) {
        //                        continue;
        //                    }
        //                    if (measureMark + position >= abs.PosX && measureMark + position < abs.PosX + abs.Width) {
        //                        abyss = true;
        //                        break;
        //                    }
        //                }
        //                if (!abyss) {
        //                    _challenges.Add(new PassiveEnemy(SCREEN_SIZE + measureMark + position, _geography[measureMark + position] + 1, kind, false));
        //                }
        //            }

        //            // Reinicia o acumulo
        //            count = 1;
        //            averagePosition = sample.Position;
        //            totalDuration = sample.Duration;
        //            offset += measureSize / perInstrument;
        //        }
        //    }

        //    /* VERSÃO 0.6.1
        //    int position; // Posição dentro da seção
        //    int offset; // Posição absoluta
        //    bool flying;
        //    bool abyss;
        //    int nextValidPosition = 0;

        //    sources.Sort((a, b) => a.Position.CompareTo(b.Position));

        //    if (intensity < measureSize * partCount / 3) {
        //        return;
        //    }

        //    foreach (var src in sources) {
        //        position = MEASURE_SIZE * src.Position / measureSize;
        //        offset = SCREEN_SIZE + measureNumber * MEASURE_SIZE + position;
        //        nextValidPosition = src.Position + src.Duration + 3 * measureSize / MEASURE_SIZE;
        //        flying = intensity > MEASURE_SIZE * 0.90 ;

        //        if (flying) {
        //            _challenges.Add(new PassiveEnemy(offset, _geography[measureNumber] + 11, true));
        //        } else {
        //            abyss = false;
        //            foreach (var abs in _abysses) {
        //                if (abs.PosX < SCREEN_SIZE + measureNumber * MEASURE_SIZE) {
        //                    continue;
        //                }
        //                if (offset >= abs.PosX && offset < abs.PosX + abs.Width) {
        //                    abyss = true;
        //                    break;
        //                }
        //            }
        //            if (!abyss) {
        //                _challenges.Add(new PassiveEnemy(offset, _geography[measureNumber] + 1, false));
        //            }
        //        }

        //    }
        //    */
        //}

        //internal void SetActiveEnemies(int measureNumber, int measureSize, int partCount, int intensity, List<MeasureElement> sources) {
        //    // Quantas vezes a nota apareceu no trecho
        //    int occurrences = sources.Count;
        //    // Ocorrência média de notas por instrumento
        //    int perInstrument = occurrences / partCount;

        //    if (perInstrument < 1) {
        //        return;
        //    }

        //    int count = 0;
        //    int averagePosition = 0;
        //    int totalDuration = 0;
        //    int offset = 0;
        //    int measureMark = measureNumber * MEASURE_SIZE;
        //    int position;
        //    bool abyss;
        //    int kind;
        //    sources.Sort((a, b) => a.Position.CompareTo(b.Position));

        //    foreach (var sample in sources) {
        //        if (sample.Position >= offset && sample.Position < offset + measureSize / perInstrument) {
        //            // Acumula para tratamento
        //            count++;
        //            totalDuration += sample.Duration;
        //            averagePosition += sample.Position;
        //        } else {
        //            // Trata valores acumulados
        //            if (count > 0) {
        //                averagePosition /= count; // Média das posições iniciais
        //                kind = totalDuration / partCount;
        //                position = MEASURE_SIZE * averagePosition / measureSize;
        //                abyss = false;

        //                foreach (Abyss abs in _abysses) {
        //                    if (abs.PosX < measureMark) {
        //                        continue;
        //                    }
        //                    if (measureMark + position >= abs.PosX && measureMark + position < abs.PosX + abs.Width) {
        //                        abyss = true;
        //                        break;
        //                    }
        //                }
        //                if (!abyss) {
        //                    _challenges.Add(new ActiveEnemy(SCREEN_SIZE + measureMark + position, _geography[measureMark + position] + 1, kind, false));
        //                }
        //            }

        //            // Reinicia o acumulo
        //            count = 1;
        //            averagePosition = sample.Position;
        //            totalDuration = sample.Duration;
        //            offset += measureSize / perInstrument;
        //        }
        //    }


        //    /* VERSÃO 0.6.1
        //    int position; // Posição dentro da seção
        //    int offset; // Posição absoluta
        //    bool flying;
        //    bool abyss;
        //    int nextValidPosition = 0;

        //    sources.Sort((a, b) => a.Position.CompareTo(b.Position));

        //    if (intensity < measureSize * partCount / 2) {
        //        return;
        //    }

        //    foreach (var src in sources) {
        //        position = MEASURE_SIZE * src.Position / measureSize;
        //        offset = SCREEN_SIZE + measureNumber * MEASURE_SIZE + position;
        //        nextValidPosition = src.Position + src.Duration + 6 * measureSize / MEASURE_SIZE;
        //        flying = intensity > MEASURE_SIZE * 0.90;

        //        if (flying) {
        //            _challenges.Add(new ActiveEnemy(offset, _geography[measureNumber] + 11, true));
        //        } else {
        //            abyss = false;
        //            foreach (var abs in _abysses) {
        //                if (abs.PosX < SCREEN_SIZE + measureNumber * MEASURE_SIZE) {
        //                    continue;
        //                }
        //                if (offset >= abs.PosX && offset < abs.PosX + abs.Width) {
        //                    abyss = true;
        //                    break;
        //                }
        //            }
        //            if (!abyss) {
        //                _challenges.Add(new ActiveEnemy(offset, _geography[measureNumber] + 1, false));
        //            }
        //        }

        //    }
        //    */
        //}

        //internal void SetAbyss(int measureNumber, int measureSize, int partCount, int intensity, List<MeasureElement> sources) {
        //    int nextValidPosition = 0;
        //    int width; 
        //    int position; // Posição dentro da seção
        //    int offset; // Posição absoluta

        //    sources.Sort((a,b) => a.Position.CompareTo(b.Position));

        //    foreach (var src in sources) {
        //        if (src.Position >= nextValidPosition) {
        //            width = MEASURE_SIZE * src.Duration / measureSize / 2;
        //            position = MEASURE_SIZE * src.Position / measureSize;
        //            offset = /*SCREEN_SIZE +*/ measureNumber * MEASURE_SIZE + position + intensity % (width + 1);
        //            if (offset >= _width) {
        //                continue;
        //            }
        //            if (width < 2) {
        //                nextValidPosition = src.Position + measureSize * 3 / MEASURE_SIZE;
        //                width = 2;
        //            } else {
        //                nextValidPosition = src.Position + src.Duration;
        //            }
        //            if (position + width > MEASURE_SIZE) {
        //                width = MEASURE_SIZE - position;
        //            }

        //            for (int xx = offset; xx < offset + width; xx++) {
        //                _geography[xx] = 0;
        //            }

        //            //_abysses.Add(new Abyss(offset, _geography[measureNumber], width));

        //        }
        //    }
        //}

        public void SaveText(string path, string musicName) {
            using (StreamWriter file = new StreamWriter(path + musicName + ".txt", false)) {
                // Cabeçalho
                file.WriteLine(musicName);
                file.Write(_width + 2 * SCREEN_SIZE);
                file.WriteLine(" " + _height);

                //Alturas
                //   Início
                file.Write(_geography.First());
                for (int ii = 1; ii < SCREEN_SIZE; ii++) {
                    file.Write(" " + _geography.First());
                }
                //   Música
                //      Cria estrutura temporária para armazenar alturas
                List<int> gridGeography = new List<int>(_width);
                foreach (int h in _geography) {
                    for (int ii = 0; ii < MEASURE_SIZE; ii++) {
                        gridGeography.Add(h);
                    }
                }
                //      Atualiza alturas para introduzir abismos
                //foreach (var abs in _abysses) {
                //    //Console.WriteLine(" !! Abismo: p: {0}, w: {1}; W: {2}", abs.PosX, abs.Width, _width);
                //    for (int xx = abs.PosX; xx < abs.PosX + abs.Width; xx++) {
                //        gridGeography[xx] = 0;
                //    }
                //}
                //      Escreve geografia
                foreach (int h in gridGeography) {
                    file.Write(" " + h);
                }
                //   Fim
                for (int ii = 0; ii < SCREEN_SIZE; ii++) {
                    file.Write(" " + _geography.Last());
                }

                // Lista inimigos e outros recursos;
                file.WriteLine();
                foreach (var chl in _challenges) {
                    file.WriteLine(chl.PosX + " " + chl.PosY + " " + chl.GetType());
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
                DrawSpecialSection(img, false, Builder.Instance.height);

                //foreach (var abyss in _abysses) {
                //    abyss.Draw(img);
                //}

                foreach (var challenge in _challenges) {
                    challenge.Draw(img);  
                }

                DrawMeasureBounds(img);
                DrawScreenBounds(img);
                try {
                    img.Save(name + ".png", ImageFormat.Png);
                } catch (Exception er) {
                    Console.WriteLine("Não foi possível construir a imagem ({0})", er.Message);
                }
                

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
                
                    grid.X = (SCREEN_SIZE + sectionNumber) * GRID_SIZE;

                    for (int yy = 0; yy < sectionHeight; yy++) {
                        grid.Y = img.Height - yy * GRID_SIZE;
                        g.FillRectangle(PLATFORM_DEEP, grid);
                        g.DrawLine(PLATFORM_DEEP_LINE, grid.Left, grid.Bottom, grid.Right, grid.Top);
                    }

                    grid.Y = img.Height - sectionHeight * GRID_SIZE;
                    g.FillRectangle(PLATFORM_SURFACE, grid);
                
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
