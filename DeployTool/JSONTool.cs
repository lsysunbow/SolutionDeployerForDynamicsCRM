using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DeployTool
{
    public class JSONTool
    {
        public readonly string configPath = Environment.CurrentDirectory + "\\config.js";
        public void Serialize<T>(T obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream1 = new MemoryStream())
            {
                ser.WriteObject(stream1, obj);
                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                Console.WriteLine("JSON form of {0} object",typeof(T));
                string result = sr.ReadToEnd(); 
                Console.WriteLine("JSON has been serialized to {0}",this.configPath); 
                File.WriteAllText(configPath, result); 
            }
        }
        public T Deserialize<T>(string json) where T :  class
        {
       
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            T deserializedObj = ser.ReadObject(ms) as T;
            ms.Close();
            return deserializedObj; 
        }
         
    }
}
