using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Examples
{
    class RepositoryExamples
    {
        public static void RunExamples()
        {
            ExampleHelper.ExampleSetPrint("Repository Examples", typeof(RepositoryExamples));
            LoadRavenExample();
        }
        public static void LoadRavenExample()
        {
            ExampleHelper.ExampleMethodPrint("Generate Raven Repository Instance", MethodInfo.GetCurrentMethod());
            var prova = new { data_dir = "..\\..\\Resource\\Database" };            
            Persistence.RepositoryConfiguration conf = new Persistence.RepositoryConfiguration(prova);
            //conf.SetConfig("data-dir", "/home/losciamano/pippo");
            Persistence.Repository p = Persistence.RepositoryFactory.GetRepositoryInstance("Raven",conf);
            Console.WriteLine(p.GetType().FullName+" - " + p.RepositoryType);
        }
    }
}
