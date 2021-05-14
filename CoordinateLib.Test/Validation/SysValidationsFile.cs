using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Tests.Validation
{
    class SysValidationsFile
    {
        string filename;
        SysValidations systems;

        public SysValidationsFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("List file not found: " + path);
            }
            this.filename = path;

            using (var tr = new StreamReader(this.filename))
            {
                XmlSerializer xmlTestPlanSerializer = new XmlSerializer(typeof(SysValidations));
                systems = (SysValidations)xmlTestPlanSerializer.Deserialize(tr);
            }
        }

        public List<SysValidation> Systems
        {
            get { return this.systems.Systems; }
        }
    }
}
