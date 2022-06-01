using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSADClassesParser
{
    internal class ParsedClass : ParsedElement
    {
        private Boolean isAbstract;

        private List<ParsedAttribute> attributes;

        public ParsedClass(string id, string name, bool isAbstract) : base(id, name)
        {
            this.isAbstract = isAbstract;
            this.attributes = new List<ParsedAttribute>();
        }

        public Boolean IsAbstract { get; }

        public void addAttribute(ParsedAttribute a)
        {
            this.attributes.Add(a);
        }

        public override string ToString()
        {
            string ret = "public";
            ret += (this.isAbstract ? " abstract" : "");
            ret += " class " + this.name + " {" + Environment.NewLine;

            foreach (ParsedAttribute a in this.attributes)
            {
                ret += "\t" + a.ToString() + ";";
                if (a.IsList)
                {
                    ret += " // check on RSAD if this name makes sense, if it does remove this comment";
                }
                ret += Environment.NewLine;
            }

            ret += "\t" + "// TODO operations" + Environment.NewLine;

            ret += "}";

            return ret;
        }

    }
}
