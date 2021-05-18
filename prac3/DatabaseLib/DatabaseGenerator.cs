using System;
using System.Drawing;
using System.IO;

namespace DatabaseLib
{
    /// <summary>
    /// Generator for a number of users for use in a database. Randomly generates PIN, account number and balance, and randomly selects first name, last name and profile image from a set of possible values.
    /// </summary>
    internal class DatabaseGenerator
    {
        private static string[] firstNames = { "Ethyl", "Rebekah", "Milissa", "Liberty", "Ma", "Romelia", "Aileen", "Son", "Kasie", "Donita", "Fumiko", "Emilie", "Violette", "Stacia", "Krysta", "Russel", "Rosaria", "Felicia", "Lulu", "Nyla", "Humberto", "Fabian", "Jordan", "Brittney", "Jesenia", "Robbi", "Erinn", "Brett", "Sharla", "Freida", "Letisha", "Toi", "Lorenza", "Gerda", "Glennie", "Geoffrey", "Cassy", "Janis", "Margy", "Esperanza", "Leonor", "Claudie", "Shaquana", "Harriett", "Ona", "Rikki", "Cherrie", "Eduardo", "Johanna", "Jamel" };
        private static string[] lastNames = { "Cronkhite", "Gilcrease", "Easterling", "Kenner", "Culotta", "Yingling", "Dauenhauer", "Bohland", "Stansberry", "Hobgood", "Adair", "Finley", "Lebaron", "Steinberg", "Emmer", "Kehl", "Miland", "Guillermo", "Palmateer", "Romines", "Walko", "Tinkler", "Spurr", "Linz", "Latorre", "Kwiecien", "Olin", "Lodge", "Rogowski", "Mcmurtry", "Binns", "Brandstetter", "Ostby", "Sherburne", "Brewer", "Arboleda", "Ackman", "Ahlgren", "Rainwater", "Bushell", "Bitton", "Dauphin", "Short", "Benjamin", "Fergerson", "Aigner", "Kilman", "Esterly", "Bueche", "Dulle" };
        private static string resPath = Path.GetFullPath("../../../res/");
        private static string[] imagePaths = { resPath + @"1.jpg", resPath + @"2.jpg", resPath + @"3.jpg", resPath + @"4.jpg", resPath + @"5.jpg", resPath + @"6.jpg", resPath + @"7.jpg", resPath + @"8.jpg", resPath + @"9.jpg", resPath + @"10.jpg" };

        private readonly Random _rand;

        private Bitmap[] _images;

        public DatabaseGenerator()
        {
            LoadImages();

            _rand = new Random();
        }

        /// <summary>
        /// Generates data for the next account.
        /// </summary>
        /// <param name="imageNum">Number/Id of image to be used for this account</param>
        public void GetNextAccount(out uint pin, out uint acctNo, out string firstName, out string lastName, out int balance, out int imageNum)
        {
            pin = GetPIN();
            acctNo = GetAcctNo();
            firstName = GetFirstname();
            lastName = GetLastname();
            balance = GetBalance();
            imageNum = GetImageNum();
        }

        /// <summary>
        /// Retrieves an image based on its "image-number", a value specific to this generator used to retrieve from a static set of images.
        /// </summary>
        /// <param name="num">Image number of image to retrieve</param>
        /// <returns>Profile image</returns>
        public Bitmap GetImageByNum(int num)
        {
            return _images[num];
        }

        /// <summary>
        /// Load the "cached" set of images from the list of image file paths
        /// </summary>
        private void LoadImages()
        {
            _images = new Bitmap[imagePaths.Length];

            for (int ii = 0; ii < imagePaths.Length; ii++)
            {
                _images[ii] = new Bitmap(imagePaths[ii], false);
            }
        }

        private string GetFirstname()
        {
            return firstNames[_rand.Next(firstNames.Length)];
        }
        private string GetLastname()
        {
            return lastNames[_rand.Next(lastNames.Length)];
        }

        private uint GetPIN()
        {
            return Convert.ToUInt32(_rand.Next(999999));
        }
        private uint GetAcctNo()
        {
            return Convert.ToUInt32(_rand.Next(999999));
        }
        private int GetBalance()
        {
            return _rand.Next(999999);
        }

        private int GetImageNum()
        {
            return _rand.Next(_images.Length);
        }
    }
}
