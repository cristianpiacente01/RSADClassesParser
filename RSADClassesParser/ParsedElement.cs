using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSADClassesParser
{
    internal abstract class ParsedElement
    {
        protected string id; // used for further parsing
        protected string name;

        protected List<object> operations; // TODO

        public ParsedElement(string id, string name)
        {
            this.id = id;
            this.name = name;
            //this.operations = new List<object>();
        }

        public string Id { get { return this.id; } }

        public string Name { get { return this.name; } }
    }
}
