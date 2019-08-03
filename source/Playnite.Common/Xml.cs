using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Playnite.Common
{
    public class Xml
    {

        public static bool AreEqual(XElement elem1, XElement elem2)
        {
            if (elem1.Name.ToString() != elem2.Name.ToString())
            {
                return false;
            }

            if (elem1.Value != elem2.Value)
            {
                return false;
            }

            var atts1 = elem1.Attributes().OrderBy(a => a.Name.ToString()).ToList();
            var atts2 = elem2.Attributes().OrderBy(a => a.Name.ToString()).ToList();

            if (atts1.Count != atts2.Count)
            {
                return false;
            }

            if (atts1.Count > 0)
            {
                for (int i = 0; i < atts1.Count; i++)
                {
                    var att1 = atts1[i];
                    var att2 = atts2[i];
                    if (att1.Name.ToString() != att2.Name.ToString())
                    {
                        return false;
                    }

                    if (att1.Value != att2.Value)
                    {
                        return false;
                    }
                }
            }

            var elems1 = elem1.Elements().ToList();
            var elems2 = elem2.Elements().ToList();

            if (elems1.Count != elems2.Count)
            {
                return false;
            }

            if (elems1.Count > 0)
            {
                for (int i = 0; i < elems1.Count; i++)
                {
                    if (!AreEqual(elems1[i], elems2[i]))
                    {
                        return false;
                    }                        
                }
            }

            return true;
        }

        public static bool AreEqual(string xml1, string xml2)
        {
            var xdoc1 = XDocument.Parse(xml1);
            var xdoc2 = XDocument.Parse(xml2);
            return AreEqual(xdoc1.Root, xdoc2.Root);
        }
    }
}
