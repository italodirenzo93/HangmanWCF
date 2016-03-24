using System;
using System.ServiceModel;
using HangmanLibrary;

namespace HangmanServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;
            try
            {
                servHost = new ServiceHost(typeof(GameState));

                // All endpoint information is set up in the App.config file.

                servHost.Open();
                Console.WriteLine("Hangman Service started. Press any key to quit...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
