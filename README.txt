Projeto de TCC

Depende da biblioteca MusicScore ( https://github.com/jmarkim/Music2Game-MusicScore )

Gerador procedural de fases, utiliza musicXML ou MIDI de entrada.

Utilização
  1. Crie no diretório da aplicação uma pasta Music (<appDir>\Music) e uma pasta Niveis (<appDir>\Niveis).
  2. Crie dois novos subdiretórios .\Music\midi e .\Music\xml
  3. Adicione aos subdiretórios de Music suas músicas, na pasta referente à extensão do arquivo
  4. Execute o programa, sem argumentos
  
  O programa irá criar uma pasta Imagens no diretório da aplicação, caso a pasta ainda não exista. Em Imagens o programa criará também um sub diretório para a versão do código.
  As imagens criadas terão o mesmo nome que o arquivo fonte
  
  Para utilizar midi ou musicXML é necessário definir o valor da constante FILE_MODE em program.cs "midi" para usar arquivos midi ou "xml" para arquivos musicXML.
  
  O branch FDG é o mais completo.

Para saber mais sobre como a fase é criada, leia "Método.txt"

Jmarkim -- 14/02/2017
