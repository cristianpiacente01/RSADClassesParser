using System.Collections.ObjectModel;

namespace RSADClassesParser
{
    internal class ParsedOperation
    {
        private static readonly IList<string> NamesThatReturnList = new ReadOnlyCollection<string>(new List<string> { "ricercaPacchetti" }); // insert them manually here
        private static readonly IList<string> ConstructorNames = new ReadOnlyCollection<string>(new List<string> { "create" }); // even if you should always use create

        private string id;
        private ParsedElement owner;
        private string visibility;
        private Boolean isStatic;
        private Boolean returnsList;
        private string returnType;
        private string name;
        private List<ParsedParameter> parameters;

        private List<ParsedOperation> calls; // TODO

        public ParsedOperation(string id, ParsedElement owner, string visibility, Boolean isStatic, string returnType, string name)
        {
            this.id = id;
            this.owner = owner;
            this.visibility = visibility;
            this.isStatic = isStatic;
            this.returnsList = ParsedOperation.NamesThatReturnList.Contains(name);
            this.returnType = returnType;
            this.name = name;
            this.parameters = new List<ParsedParameter>();
            //this.calls = new List<ParsedOperation>();
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

        public override string ToString()
        {
            string ret = "";
            ret += this.visibility + " ";
            if (!ConstructorNames.Contains(this.name))
            {
                ret += (this.isStatic ? "static " : "");
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
            ret += ");";
            if (owner.GetType().Name.Equals("ParsedClass"))
            {
                ret += " // TODO body";
            }
            return ret;
        }
    }
}