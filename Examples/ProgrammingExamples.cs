using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;


namespace Examples
{
    class ProgrammingExamples
    {
        static void Main(string[] args)
        {
            //LOG4NET
            XmlConfigurator.Configure();
            int num_args = args.Length;
            if (num_args == 0 || args.Contains("tag"))
            {
                TagExamples.RunExamples();
            }
            if (num_args == 0 || args.Contains("rep"))
            {
                RepositoryExamples.RunExamples();
            }
            if (num_args == 0 || args.Contains("kad_rep"))
            {
                KademliaRepositoryExamples.RunExamples();
            }
        }
    }
}
