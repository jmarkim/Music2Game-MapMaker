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
        public static string VERSION = "fdg-2";
        private const string FILE_MODE = "midi";

        static void Main(string[] args) {
            //Variáveis
            string root = AppDomain.CurrentDomain.BaseDirectory; // Diretório da aplicação
            string musicPath = root + "Music\\" + FILE_MODE; // Diretório de músicas
            string[] files; // Arquivos no diretório de músicas
            string musicName;
            bool isMusicXML;
            FileInfo musicFile;
            StreamReader reader;
            Score musicScore;
            Level map;

            // Log (Console) de Características
            Console.WriteLine("Versão do algoitmo : {0}", VERSION);

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

                if (FILE_MODE == "midi") {
                    // Modo midi

                    if (musicFile.Extension == ".mid") {
                        musicName = musicFile.Name.Remove(musicFile.Name.Length - 4);
                        Console.Write("   -> {0} ", musicName);
                        try {
                            musicScore = ScoreBuilder.FromMIDI(musicFile.FullName);
                            map = new Level();
                            map.BuildSingleLoop(musicScore, root, musicName, VERSION);

                        } catch (Exception ex) { }
                    }

                } else {
                    // Modo musicXML

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
                                if (reader.ReadLine().Contains("score")) {
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

                                    //map.BuildHeightsSequence(musicScore);
                                    //Console.Write(" -- w: {0}  h: {1}", map.Width, map.Height);
                                    //map.SaveImage(root + "Imagens\\" + VERSION + "\\" + musicName);
                                    //Console.Write(" >> Imagem criada");
                                    //map.SaveText(root + "Niveis\\" + VERSION + "\\", musicName);
                                    //Console.Write("  >> lvl Criado");

                                    map.BuildSingleLoop(musicScore, root, musicName, VERSION);

                                    System.IO.File.Delete(musicPath + "\\tempMusic.xml");
                                }

                            }

                        }

                        Console.WriteLine();
                    }

                }

            }

            Console.WriteLine("Operação concluída");
            Console.ReadKey();
        }
    }
}