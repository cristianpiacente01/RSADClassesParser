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

        private List<ParsedClass> extendedClasses;

        private List<ParsedInterface> implementedInterfaces;

        private List<string> multipleInheritedClassNames;


        public ParsedClass(string id, string name, bool isAbstract) : base(id, name)
        {
            this.isAbstract = isAbstract;
            this.attributes = new List<ParsedAttribute>();
            this.extendedClasses = new List<ParsedClass>();
            this.implementedInterfaces = new List<ParsedInterface>();
            this.multipleInheritedClassNames = new List<string>();
        }

        public void addExtendedClasses(List<ParsedClass> list)
        {
            foreach (ParsedClass p in list)
            {
                if (!this.extendedClasses.Contains(p))
                {
                    this.extendedClasses.Add(p);
                }
            }
        }

        private void updateInheritedAttributes(ParsedClass c)
        {
            // used to support multiple inheritance
            foreach (ParsedAttribute attr in c.attributes)
            {
                if (!attr.Visibility.Equals("private"))
                {
                    this.addAttribute(attr);
                }
            }
        }

        private string getExtends()
        {
            string ret = "";
            if (this.extendedClasses.Count() != 0)
            {
                ret += this.extendedClasses[0].Name;
            }
            if (this.extendedClasses.Count() > 1)
            {
                List<ParsedClass> toRemove = new List<ParsedClass>();
                int count = this.extendedClasses.Count() - 1;
                foreach (ParsedClass c in this.extendedClasses.GetRange(1, count))
                {
                    this.updateInheritedAttributes(c);
                    toRemove.Add(c);
                    this.multipleInheritedClassNames.Add(c.Name);
                }
                this.extendedClasses.RemoveAll(c => toRemove.Contains(c));
            }
            return ret;
        }

        private string getImplements()
        {
            string ret = "";
            foreach (ParsedInterface i in implementedInterfaces)
            {
                ret += i.Name + ", ";
            }
            if (ret.Length >= 2)
            {
                int newLength = ret.Length - 2;
                ret = ret.Substring(0, newLength);
            }
            return ret;
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
            ret += " class " + this.name;

            string extendsStr = this.getExtends();
            if (extendsStr.Length > 0)
            {
                ret += " extends " + extendsStr;
            }

            string implementsStr = this.getImplements();
            if (implementsStr.Length > 0)
            {
                ret += " implements " + implementsStr;
            }

            ret += " {" + Environment.NewLine;

            foreach (string m in multipleInheritedClassNames)
            {
                ret += "\t" + "// WARNING: multiple inheritance found, inherited manually " + m + Environment.NewLine;
            }

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
