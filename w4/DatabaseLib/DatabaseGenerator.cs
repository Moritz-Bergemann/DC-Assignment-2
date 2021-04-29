using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLib
{
    internal class DatabaseGenerator
    {
        private static string[] firstNames = { "Ethyl", "Rebekah", "Milissa", "Liberty", "Ma", "Romelia", "Aileen", "Son", "Kasie", "Donita", "Fumiko", "Emilie", "Violette", "Stacia", "Krysta", "Russel", "Rosaria", "Felicia", "Lulu", "Nyla", "Humberto", "Fabian", "Jordan", "Brittney", "Jesenia", "Robbi", "Erinn", "Brett", "Sharla", "Freida", "Letisha", "Toi", "Lorenza", "Gerda", "Glennie", "Geoffrey", "Cassy", "Janis", "Margy", "Esperanza", "Leonor", "Claudie", "Shaquana", "Harriett", "Ona", "Rikki", "Cherrie", "Eduardo", "Johanna", "Jamel" };
        private static string[] lastNames = { "Cronkhite", "Gilcrease", "Easterling", "Kenner", "Culotta", "Yingling", "Dauenhauer", "Bohland", "Stansberry", "Hobgood", "Adair", "Finley", "Lebaron", "Steinberg", "Emmer", "Kehl", "Miland", "Guillermo", "Palmateer", "Romines", "Walko", "Tinkler", "Spurr", "Linz", "Latorre", "Kwiecien", "Olin", "Lodge", "Rogowski", "Mcmurtry", "Binns", "Brandstetter", "Ostby", "Sherburne", "Brewer", "Arboleda", "Ackman", "Ahlgren", "Rainwater", "Bushell", "Bitton", "Dauphin", "Short", "Benjamin", "Fergerson", "Aigner", "Kilman", "Esterly", "Bueche", "Dulle" };
        private static string resPath = @"C:\Users\morit\Source\Repos\DC-Workshops\DatabaseLib\res\";
        private static string[] imagePaths = { resPath + @"1.jpg", resPath + @"2.jpg", resPath + @"3.jpg", resPath + @"4.jpg", resPath + @"5.jpg", resPath + @"6.jpg", resPath + @"7.jpg", resPath + @"8.jpg", resPath + @"9.jpg", resPath + @"10.jpg" };

        private Random m_rand;

        public DatabaseGenerator()
        {
            Console.WriteLine("generating...");
            m_rand = new Random();
        }

        public void GetNextAccount(out uint pin, out uint acctNo, out string firstName, out string lastName, out int balance, out string imagePath)
        {
            pin = GetPIN();
            acctNo = GetAcctNo();
            firstName = GetFirstname();
            lastName = GetLastname();
            balance = GetBalance();
            imagePath = GetImagePath();
        }

        private string GetFirstname()
        {
            return firstNames[m_rand.Next(firstNames.Length)];
        }
        private string GetLastname()
        {
            return lastNames[m_rand.Next(lastNames.Length)];
        }

        private uint GetPIN()
        {
            return Convert.ToUInt32(m_rand.Next(999999));
        }
        private uint GetAcctNo()
        {
            return Convert.ToUInt32(m_rand.Next(999999));
        }
        private int GetBalance()
        {
            return m_rand.Next(999999);
        }

        private string GetImagePath()
        {
            return imagePaths[m_rand.Next(imagePaths.Length)];
        }
    }
}
