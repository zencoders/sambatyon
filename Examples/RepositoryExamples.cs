using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Client;

namespace Examples
{
    class RepositoryExamples
    {
        public static void RunExamples()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Repository Examples (RepositoryExaples)");
            Console.ResetColor();
            LoadRavenExample();
        }
        public static void LoadRavenExample()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Generate Raven Repository Instance (RepositoryExamples.LoadRavenExample");
            Console.ResetColor();
            var prova = new { data_dir = "pippo" };            
            Persistence.RepositoryConfiguration conf = new Persistence.RepositoryConfiguration(prova);
            //conf.SetConfig("data-dir", "/home/losciamano/pippo");
            Persistence.Repository p = Persistence.RepositoryFactory.GetRepositoryInstance("Raven",conf);
            Console.WriteLine(p.GetType().FullName+" - " + p.RepositoryType);
        }
    }
}
