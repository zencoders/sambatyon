using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Persistence
{
    public class RepositoryConfiguration: System.Collections.Generic.Dictionary<string,string>
    {
        public RepositoryConfiguration(): base() {}
        public RepositoryConfiguration(Object anon)
            : this()
        {
            PropertyInfo[] props = anon.GetType().GetProperties();
            for (int k = 0; k < props.Length; k++)
            {
                this.SetConfig(props[k].Name ,props[k].GetValue(anon, null).ToString());
            }
        }
        public string GetConfig(string key)
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return "";
            }
        }
        public void SetConfig(string key, string value)
        {
            this.Add(key, value);
        }            
    }
}
