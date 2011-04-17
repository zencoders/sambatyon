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
            TagExamples.RunExamples();
            RepositoryExamples.RunExamples();
        }
    }
}
