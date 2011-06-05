/* 
 FATEC - Carapicu�ba

 Trabalho de conclus�o de curso - ASTI Jogos

Gabriel Sacramento do Amaral - gabsacramento@gmail.com
Leandro Roque Razori - roque.razori@gmail.com
Renato Ibanhez - renato.ibanhez@gmail.com
Sergio Alberto Pasqualino - sergio.pasqualino@gmail.com

Amude � um software-livre; voc� pode redistribu�-lo e/ou modific�-lo sobre os termos da 
LGPL: GNU Lesser General Public License como publicado pela Free Software Foundation na 
vers�o 3 desta licen�a ou qualquer vers�o posterior.
    
Setembro/2010
 */

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Amude
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (!getPrevInstance())
            {
                using (Amude game = new Amude())
                {
                    game.Run();
                }
            }
            else
            {
                MessageBox.Show("Uma inst�ncia do Amude j� est� em execu��o.", 
                                "Alerta", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Information);
            }
        }

        private static bool getPrevInstance()
        {
            string processName = Process.GetCurrentProcess().ProcessName;

            Process[] process = Process.GetProcessesByName(processName);

            if (process.Length > 1)
            {
                return true; 
            }
            else
            {
                return false;
            }
        }
    }


}

