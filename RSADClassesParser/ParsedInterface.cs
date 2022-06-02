using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSADClassesParser
{
    internal class ParsedInterface : ParsedElement
    {
        public ParsedInterface(string id, string name) : base(id, name)
        {
        }

        public override string ToString()
        {
            string ret = "public interface " + this.name + " {" + Environment.NewLine;

            foreach (ParsedOperation op in this.operations)
            {
                ret += "\t" + op.ToString() + Environment.NewLine;
            }

            ret += "}";
            return ret;
        }
    }
}
