﻿using System.Collections.ObjectModel;

namespace RSADClassesParser
{
    internal class ParsedOperation
    {
        private static readonly Dictionary<string, string> NamesThatReturnList = new Dictionary<string, string>()
        { 
            { "ricercaPacchetti", "SWPacchettoVacanza" } 
        }; // insert them manually here
        
        private static readonly IList<string> ConstructorNames = new ReadOnlyCollection<string>(new List<string> { "create" }); // even if you should always use "create"

        private string id;
        private ParsedElement owner;
        private string visibility;
        private Boolean isStatic;
        private Boolean isAbstract;
        private Boolean returnsList;
        private string returnType;
        private string name;
        private List<ParsedParameter> parameters;

        public string Visibility { get { return visibility; } }

        // TODO body

        public ParsedOperation(string id, ParsedElement owner, string visibility, Boolean isStatic, Boolean isAbstract, string returnType, string name)
        {
            this.id = id;
            this.owner = owner;
            this.visibility = visibility;
            this.isStatic = isStatic;
            this.isAbstract = isAbstract;
            this.returnsList = ParsedOperation.NamesThatReturnList.ContainsKey(name);
            this.returnType = !ParsedOperation.NamesThatReturnList.ContainsKey(name) ? returnType : ParsedOperation.NamesThatReturnList[name];
            this.name = name;
            this.parameters = new List<ParsedParameter>();
        }

        public void AddParameters(List<ParsedParameter> list)
        {
            foreach (ParsedParameter parameter in list)
            {
                if (!parameters.Contains(parameter))
                {
                    parameters.Add(parameter);
                }
            }
        }

        public Boolean IsConstructor()
        {
            return ConstructorNames.Contains(this.name);
        }

        public override string ToString()
        {
            string ret = "";
            ret += this.visibility + " ";
            if (!IsConstructor())
            {
                // I won't check if both static and abstract are true, idk why RSAD lets you do that, they shouldn't coexist
                ret += (this.isStatic ? "static " : "");
                ret += (this.isAbstract ? "abstract " : "");

                ret += (this.returnsList ? ("List<" + this.returnType + ">") : this.returnType) + " ";
                ret += this.name + "(";
            }
            else
            {
                ret += owner.Name + "(";
            }

            for (int i = 0; i < this.parameters.Count() - 1; i++)
            {
                ret += this.parameters[i].ToString() + ", ";
            }
            if (this.parameters.Count() > 0)
            {
                ret += this.parameters[this.parameters.Count() - 1];
            }
            ret += ")";
            if (owner.GetType().Name.Equals("ParsedClass"))
            {
                ret += " { } // TODO body";
            }
            else
            {
                ret += ";";
            }
            return ret;
        }
    }
}