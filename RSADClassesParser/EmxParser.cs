using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RSADClassesParser
{
    public class EmxParser
    {
        private static XNamespace xmi = "http://www.omg.org/XMI";

        private Dictionary<string, KeyValuePair<XElement, ParsedElement>> bigMap; // the only keys with a null value at the end will be DataTypes, but it's fine

        private XDocument doc;

        // constants for actions
        private const int Generalizations = 0;
        private const int InterfacesRealization = 1;

        private static Boolean IsSWClassOrInterface(string type, string name)
        {
            if (!type.Equals("uml:Interface") && !type.Equals("uml:Class"))
            {
                return false;
            }

            return name.StartsWith("SW") // change this depending on your RSAD project
                   || (name.StartsWith("I") && Char.IsUpper(name.ToCharArray()[1]));
        }

        public EmxParser(String path)
        {
            this.doc = XDocument.Load(path);
            this.bigMap = new Dictionary<string, KeyValuePair<XElement, ParsedElement>>();
        }

        private IEnumerable<XElement> GetSWElements()
        {
            return from e in doc.Root.Descendants("packagedElement")
                   where e.Attribute(xmi + "type") != null
                   && e.Attribute("name") != null
                   && EmxParser.IsSWClassOrInterface(e.Attribute(xmi + "type").Value, e.Attribute("name").Value)
                   orderby e.Attribute("name").Value
                   select e;
        }

        private void UpdateMapWithDataTypes()
        {
            IEnumerable<XElement> query = from e in doc.Root.Descendants("packagedElement")
                                          where e.Attribute(xmi + "type") != null
                                          && e.Attribute(xmi + "type").Value.Equals("uml:DataType")
                                          select e;

            foreach (XElement dataType in query)
            {
                KeyValuePair<XElement, ParsedElement> pair = new KeyValuePair<XElement, ParsedElement>(dataType, null);
                this.bigMap.Add(dataType.Attribute(xmi + "id").Value, pair);
            }
        }

        private ParsedElement ParseGenOrInterfHelper(string id, int action)
        {
            ParsedElement p = this.bigMap[id].Value;

            if (action == EmxParser.Generalizations)
            {
                List<ParsedClass> temp = this.ParseGenOrInterf(this.bigMap[p.Id].Key, p.Id, action).Cast<ParsedClass>().ToList();
                ((ParsedClass)p).AddExtendedClasses(temp);
            }

            return p;
        }
        
        private List<ParsedElement> ParseGenOrInterf(XElement parent, string parentId, int action)
        {
            List<ParsedElement> ret = new List<ParsedElement>();

            IEnumerable<XElement> query;

            switch (action)
            {
                case EmxParser.Generalizations:
                    query = from g in parent.Descendants("generalization")
                            where g.Attribute("general") != null
                            select g;
                    break;
                case EmxParser.InterfacesRealization:
                    query = from i in parent.Descendants("interfaceRealization")
                            where i.Attribute("supplier") != null
                            select i;
                    break;
                default: // what
                    return ret;
            }

            string xname = action == EmxParser.Generalizations ? "general" : "supplier";

            foreach (XElement element in query)
            {
                string id = element.Attribute(xname).Value;
                if (this.bigMap.ContainsKey(id))
                {
                    ParsedElement parsedElement = ParseGenOrInterfHelper(id, action);
                    ret.Add(parsedElement);
                }
            }

            return ret;
        }

        private IEnumerable<ParsedElement> CreateElements(IEnumerable<XElement> elements)
        {
            List<ParsedElement> list = new List<ParsedElement>();

            foreach (XElement e in elements)
            {
                string id = e.Attribute(xmi + "id").Value;
                string type = e.Attribute(xmi + "type").Value;
                string name = e.Attribute("name").Value;

                ParsedElement parsedElement;

                if (type.Equals("uml:Interface"))
                {
                    parsedElement = new ParsedInterface(id, name);
                } 
                else // uml:Class
                {
                    Boolean isAbstract = e.Attribute("isAbstract") != null
                        && e.Attribute("isAbstract").Value.Equals("true");

                    parsedElement = new ParsedClass(id, name, isAbstract);
                }

                KeyValuePair<XElement, ParsedElement> pair = new KeyValuePair<XElement, ParsedElement>(e, parsedElement);
                this.bigMap.Add(e.Attribute(xmi + "id").Value, pair);

                list.Add(parsedElement);

            }

            this.UpdateMapWithDataTypes(); // uml:DataType

            return list;
        }

        private void AddAttributes(IEnumerable<ParsedElement> parsedElements) // TODO make this shit prettier, well not now
        {
            foreach (ParsedElement p in parsedElements)
            {
                if (p.GetType().Name.Equals("ParsedClass"))
                {
                    XElement e = this.bigMap[p.Id].Key;

                    IEnumerable<XElement> query = from a in e.Descendants("ownedAttribute")
                                where a != null
                                select a;

                    foreach (XElement a in query)
                    {
                        string type = this.GetTypeString(a);
                        string name = a.Attribute("name").Value;
                        string visibility = a.Attribute("visibility").Value;
                        Boolean isStatic = a.Attribute("isStatic") != null
                            && a.Attribute("isStatic").Value.Equals("true");

                        IEnumerable<XElement> otherQuery = from up in a.Descendants("upperValue")
                                         where up != null
                                         select up;

                        Boolean isList = otherQuery.Count() > 0 && otherQuery.First().Attribute("value").Value.EndsWith("*");
                        ParsedAttribute attr = new ParsedAttribute(visibility, isStatic, isList, type, name);
                        ((ParsedClass)p).AddAttribute(attr);
                    }
                }
            }
        }

        private string GetTypeString(XElement ownedAttrOrParam)
        {
            string type;
            if (ownedAttrOrParam.Attribute("type") != null)
            {
                type = this.bigMap[ownedAttrOrParam.Attribute("type").Value].Key.Attribute("name").Value;
            }
            else // there's a <type> with xmi:type="uml:PrimitiveType" and stuff in href
            {
                XElement typeElement = ownedAttrOrParam.Descendants("type").First();
                type = typeElement.Attribute("href").Value.Substring(typeElement.Attribute("href").Value.LastIndexOf("#") + 1);
                if (type.StartsWith("_") && type.EndsWith("?"))
                {
                    type = type.Substring(type.LastIndexOf("/") + 1);
                    type = type.Substring(0, type.Length - 1);
                }
            }
            return type;
        }

        private void AddOperations(IEnumerable<ParsedElement> parsedElements) // TODO make this shit prettier, well not now
        {
            foreach (ParsedElement p in parsedElements)
            {
                XElement e = this.bigMap[p.Id].Key;

                IEnumerable<XElement> queryOp = from op in e.Descendants("ownedOperation")
                                              where op != null
                                              select op;

                foreach (XElement opElement in queryOp)
                {
                    string name = opElement.Attribute("name").Value;

                    string id = opElement.Attribute(xmi + "id").Value;

                    string visibility = opElement.Attribute("visibility") == null
                        ? "public" : opElement.Attribute("visibility").Value;

                    Boolean isStatic = opElement.Attribute("isStatic") != null
                            && opElement.Attribute("isStatic").Value.Equals("true");

                    string returnType = "void";

                    IEnumerable<XElement> returnTypeQuery = from returnP in opElement.Descendants("ownedParameter")
                                                            where returnP != null
                                                            && returnP.Attribute("direction") != null
                                                            && returnP.Attribute("direction").Value.Equals("return")
                                                            select returnP;

                    if (returnTypeQuery.Count() > 0)
                    {
                        returnType = this.bigMap[returnTypeQuery.First().Attribute("type").Value].Value.Name;
                    }

                    ParsedOperation parsedOperation = new ParsedOperation(id, p, visibility, isStatic, returnType, name);

                    IEnumerable<XElement> paramsQuery = from par in opElement.Descendants("ownedParameter")
                                                        where par != null
                                                        && par.Attribute("direction") == null
                                                        && par.Attribute("name") != null
                                                        select par;

                    List<ParsedParameter> parameters = new List<ParsedParameter>();

                    foreach (XElement param in paramsQuery)
                    {
                        string paramType = this.GetTypeString(param);
                        ParsedParameter newParam = new ParsedParameter(paramType, param.Attribute("name").Value);
                        parameters.Add(newParam);
                    }

                    parsedOperation.AddParameters(parameters);

                    this.bigMap[p.Id].Value.AddOperation(parsedOperation);

                }
            }
        }

        private void UpdateGenOrImpl(IEnumerable<ParsedElement> elements, int action)
        {
            foreach (ParsedElement p in elements)
            {
                if (p.GetType().Name.Equals("ParsedClass"))
                {
                    XElement xelement = this.bigMap[p.Id].Key;
                    List<ParsedElement> parsedElements = this.ParseGenOrInterf(xelement, p.Id, action);
                    if (action == EmxParser.Generalizations)
                    {
                        ((ParsedClass)p).AddExtendedClasses(parsedElements.Cast<ParsedClass>().ToList());
                    }
                    else if (action == EmxParser.InterfacesRealization)
                    {
                        ((ParsedClass)p).AddImplementedInterfaces(parsedElements.Cast<ParsedInterface>().ToList());
                    }
                }
            }
        }

        public void Parse()
        {
            if (doc.Root == null)
            {
                Program.Options.Error("Document root is null!");
            }

            IEnumerable<XElement> elements = this.GetSWElements();

            if (elements.Count() == 0)
            {
                Program.Options.Error("0 SW classes/interfaces found!");
            }

            Program.Options.Log("Found " + elements.Count() + " SW classes/interfaces!");

            IEnumerable<ParsedElement> parsedElements = this.CreateElements(elements); // this also updates bigMap

            this.AddAttributes(parsedElements);

            this.UpdateGenOrImpl(parsedElements, EmxParser.Generalizations);

            this.UpdateGenOrImpl(parsedElements, EmxParser.InterfacesRealization);

            this.AddOperations(parsedElements);

            foreach (ParsedElement p in parsedElements)
            {
                Console.WriteLine(p);
            }

        }
    }
}
