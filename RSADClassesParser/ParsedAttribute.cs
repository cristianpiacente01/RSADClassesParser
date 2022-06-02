using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSADClassesParser
{
    internal class ParsedAttribute
    {
        private string visibility;
        private Boolean isStatic;
        private Boolean isList;
        private string type;
        private string name;
        public ParsedAttribute(string visibility, Boolean isStatic, Boolean isList, string type, string name)
        {
            this.visibility = visibility;
            this.isStatic = isStatic;
            this.isList = isList;
            this.type = type;
            this.name = name;
        }

        public string Visibility { get { return this.visibility; } }

        public Boolean IsStatic { get { return this.isStatic; } }

        public Boolean IsList { get { return this.isList; } }

        public string Type { get { return this.type; } }

        public String Name { get { return this.name; } }

        public override string ToString()
        {
            return this.visibility + " " + (this.isStatic ? "static " : "") + (this.isList ? ("List<" + this.type + ">") : this.type) + " " + this.name;
        }

    }
}
