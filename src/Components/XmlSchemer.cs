using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class XmlSchemer : IXmlSchemer {
        public string Create(Type t) {
            string xsd;
            using (var stream = new MemoryStream()) {
                using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                    var schemas = new XmlSchemas();
                    var exporter = new XmlSchemaExporter(schemas);
                    var mapping = new XmlReflectionImporter().ImportTypeMapping(t);
                    exporter.ExportTypeMapping(mapping);
                    foreach (XmlSchema schema in schemas) {
                        schema.Write(writer);
                    }

                    writer.Flush();
                    xsd = Encoding.UTF8.GetString(stream.ToArray());
                }
            }

            xsd = ManipulateXsdForScriptContent(xsd);
            return xsd;
        }

        private static string ManipulateXsdForScriptContent(string xsd) {
            const string tag = "name=\"script\"";
            const string startTag = "<xs:complexType mixed=\"true\">";
            const string endTag = "</xs:complexType>";

            var tagPos = xsd.IndexOf(tag, StringComparison.Ordinal);
            var startPos = tagPos > 0 ? xsd.Substring(tagPos).IndexOf(startTag, StringComparison.Ordinal) : 0;
            var endPos = startPos > 0 ? xsd.Substring(tagPos + startPos).IndexOf(endTag, StringComparison.Ordinal) : 0;

            if (tagPos <= 0 || startPos <= 0 || endPos <= 0) { return xsd; }

            xsd = xsd.Substring(0, tagPos + startPos) + xsd.Substring(tagPos + startPos + endPos + endTag.Length);
            return xsd;
        }

        public bool Valid(string secretGuid, string xml, Type t, IErrorsAndInfos errorsAndInfos) {
            return Valid(secretGuid, xml, Create(t), t, errorsAndInfos);
        }

        internal bool Valid(string secretGuid, string xml, string xsd, Type t, IErrorsAndInfos errorsAndInfos) {
            if (xml == "") {
                return false;
            }

            var schemaSet = new XmlSchemaSet();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xsd))) {
                schemaSet.Add("http://www.aspenlaub.net", XmlReader.Create(stream));
            }

            XmlSchema compiledXmlSchema = null;
            foreach (XmlSchema schema in schemaSet.Schemas()) {
                compiledXmlSchema = schema;
            }

            var schemaIsValid = compiledXmlSchema != null;

            var settings = new XmlReaderSettings();
            if (schemaIsValid) {
                settings.Schemas.Add(compiledXmlSchema);
            } else {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.InvalidSchema, $"{secretGuid}.xsd"));
            }
            settings.ValidationEventHandler += (_, args) => {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.InvalidXml, $"{secretGuid}.xml", $"{secretGuid}.xsd", args.Message));
                schemaIsValid = false;
            };
            settings.ValidationType = ValidationType.Schema;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
                var reader = XmlReader.Create(stream, settings);
                while (reader.Read()) {}

                reader.Close();
            }

            return schemaIsValid;
        }
    }
}
