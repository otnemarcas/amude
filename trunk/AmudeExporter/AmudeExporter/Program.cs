using System.IO;
using System;

/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

Gabriel Sacramento do Amaral - gabsacramento@gmail.com
Leandro Roque Razori - roque.razori@gmail.com
Renato Ibanhez - renato.ibanhez@gmail.com
Sergio Alberto Pasqualino - sergio.pasqualino@gmail.com

Amude é um software-livre; você pode redistribuí-lo e/ou modificá-lo sobre os termos da 
LGPL: GNU Lesser General Public License como publicado pela Free Software Foundation na 
versão 3 desta licença ou qualquer versão posterior.
    
Setembro/2010
 */

namespace AmudeExporter
{
    class Program
    {

        const string AMUDE_CRYPTO_KEY = "96aaa6dd-0d86-4305-83ec-8e38c06df1c1";
        const string ROOT_DIR = @"\data";
        const string EXPORT_EXTENSION = ".amude";

        static void Main(string[] args)
        {
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory+ROOT_DIR);
            foreach (FileInfo fileInfo in dir.GetFiles())
            {
                Console.WriteLine("Converting: "+fileInfo.Name);
                try
                {
                    CryptoHelp.EncryptFile(fileInfo.FullName, fileInfo.FullName + EXPORT_EXTENSION, AMUDE_CRYPTO_KEY);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("\tErro ao converter "+ fileInfo.Name + "\n\t" + ex.Message);
                }
            }
            Console.WriteLine("Done. Press any key.");
            Console.ReadKey();
        }
    }
}
