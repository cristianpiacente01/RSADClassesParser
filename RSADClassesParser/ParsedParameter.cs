using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSADClassesParser
{
    internal class ParsedParameter
    {
        // I suppose no List is passed as a parameter in the operations, but if you need List support let me know
        private string type;
        private string name;
        public ParsedParameter(string type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public string Type { get { return this.type; } }
        public string Name { get { return this.name; } }

        public override string ToString()
        {
            return this.type + " " + this.name;
        }
    }
}
