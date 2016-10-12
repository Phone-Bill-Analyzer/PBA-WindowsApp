using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBA_Application
{
    public sealed class ServiceProvider
    {
        internal ServiceProvider(string code, string name)
        {
            _code = code;
            _name = name;
        }

        private string _code, _name;

        public string Code { get { return _code; } }
        public string Name { get { return _name; } }

        public override string ToString()
        {
            return Name;
        }
    }
}
