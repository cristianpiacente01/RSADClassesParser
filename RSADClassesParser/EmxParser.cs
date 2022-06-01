using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RSADClassesParser
{
    internal class EmxParser
    {
        private static XNamespace xmi = "http://www.omg.org/XMI";

        private Dictionary<string, XElement> idMap;

        private XDocument doc;

        public EmxParser(String path)
        {
            doc = XDocument.Load(path);
            idMap = new Dictionary<string, XElement>();
        }

        private Boolean isSWClassOrInterface(string type, string name)
        {
            if (!type.Equals("uml:Interface") && !type.Equals("uml:Class"))
            {
                return false;
            }

            return name.StartsWith("SW") 
                   || (name.StartsWith("I") && Char.IsUpper(name.ToCharArray()[1]));
        }

        private IEnumerable<XElement> getSWElements()
        {
            return from e in doc.Root.Descendants("packagedElement")
                   where e.Attribute(xmi + "type") != null
                   && e.Attribute("name") != null
                   && isSWClassOrInterface(e.Attribute(xmi + "type").Value, e.Attribute("name").Value)
                   orderby e.Attribute("name").Value
                   select e;
        }

        private void updateMap(IEnumerable<XElement> elements)
        {
            // uml:DataType
            IEnumerable<XElement> query = from e in doc.Root.Descendants("packagedElement")
                        where e.Attribute(xmi + "type") != null
                        && e.Attribute("name") != null
                        && e.Attribute(xmi + "type").Value.Equals("uml:DataType")
                        select e;

            foreach (XElement dataType in query)
            {
                idMap.Add(dataType.Attribute(xmi + "id").Value, dataType);
            }

            // uml:Class and uml:Interface
            foreach (XElement e in elements)
            {
                idMap.Add(e.Attribute(xmi + "id").Value, e);
            }
        }

        private IEnumerable<ParsedElement> createElements(IEnumerable<XElement> elements)
        {
            List<ParsedElement> list = new List<ParsedElement>();

            foreach (XElement e in elements)
            {
                string id = e.Attribute(xmi + "id").Value;
                string type = e.Attribute(xmi + "type").Value;
                string name = e.Attribute("name").Value;

                if (type.Equals("uml:Interface"))
                {
                    list.Add(new ParsedInterface(id, name));
                } 
                else // uml:Class
                {
                    Boolean isAbstract = e.Attribute("isAbstract") != null
                        && e.Attribute("isAbstract").Value.Equals("true");

                    list.Add(new ParsedClass(id, name, isAbstract));
                }
            }

            return list;
        }

        private void addAttributes(IEnumerable<ParsedElement> parsedElements) // TODO make this shit prettier
        {
            foreach (ParsedElement p in parsedElements)
            {
                if (p.GetType().Name.Equals("ParsedClass"))
                {
                    XElement e = idMap[p.Id];

                    IEnumerable<XElement> query = from a in e.Descendants("ownedAttribute")
                                where a != null
                                select a;

                    foreach (XElement a in query)
                    {
                        string type;
                        if (a.Attribute("type") != null)
                        {
                            type = idMap[a.Attribute("type").Value].Attribute("name").Value;
                        }
                        else // there's a <type> with xmi:type="uml:PrimitiveType" and stuff in href
                        {
                            XElement typeElement = a.Descendants("type").First();
                            type = typeElement.Attribute("href").Value.Substring(typeElement.Attribute("href").Value.LastIndexOf("#") + 1);
                            if (type.StartsWith("_") && type.EndsWith("?"))
                            {
                                type = type.Substring(type.LastIndexOf("/") + 1);
                                type = type.Substring(0, type.Length - 1);
                            }
                        }
                        string name = a.Attribute("name").Value;
                        string visibility = a.Attribute("visibility").Value;
                        Boolean isStatic = a.Attribute("isStatic") != null
                            && a.Attribute("isStatic").Value.Equals("true");

                        IEnumerable<XElement> otherQuery = from up in a.Descendants("upperValue")
                                         where up != null
                                         select up;

                        Boolean isList = otherQuery.Count() > 0 && otherQuery.First().Attribute("value").Value.EndsWith("*");
                        ParsedAttribute attr = new ParsedAttribute(visibility, isStatic, isList, type, name);
                        ParsedClass c = (ParsedClass)p;
                        c.addAttribute(attr);
                    }
                }
            }
        }

        public void parse()
        {
            if (doc.Root == null)
            {
                Program.Options.Error("Document root is null!");
            }

            IEnumerable<XElement> elements = getSWElements();

            if (elements.Count() == 0)
            {
                Program.Options.Error("0 SW classes/interfaces found!");
            }

            Program.Options.Log("Found " + elements.Count() + " SW classes/interfaces!");

            updateMap(elements);

            IEnumerable<ParsedElement> parsedElements = createElements(elements);

            /*foreach (ParsedElement p in parsedElements)
            {
                Console.WriteLine(p.GetType().Name + " " + p.Name);
            }*/

            addAttributes(parsedElements);

            foreach (ParsedElement p in parsedElements)
            {
                Console.WriteLine(p.ToString());
            }

        }
    }
}
