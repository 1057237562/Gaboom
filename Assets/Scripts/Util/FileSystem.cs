using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace Gaboom.Util
{
    public class FileSystem
    {
        public static string ReadFile(string filename)
        {
            if (File.Exists(filename))
            {
                StreamReader file = new StreamReader(filename);
                string content = file.ReadToEnd();
                file.Close();
                return content;
            }
            else
            {
                return "";
            }
        }

        public static void WriteFile(string filename,string content)
        {
            StreamWriter writer = new StreamWriter(filename);
            writer.Write(content);
            writer.Flush();
            writer.Close();
        }

        public static List<Reflection> ReadConfigFile(string configname)
        {
            return DeserializeFromFile<List<Reflection>>(Environment.CurrentDirectory + "/" + configname + ".cfg");
        }

        public static void WriteConfigFile(string configname,List<Reflection> content)
        {
            SerializeToFile(new Values(content,Environment.CurrentDirectory + "/" + configname + ".cfg"));
        }

        public struct Values
        {
            public object values;
            public string filename;

            public Values(object vs, string fn)
            {
                values = vs;
                filename = fn;
            }
        }

        public static void SerializeToFile(object values)
        {
            Values val = (Values)values;

            if (!Directory.Exists(val.filename.Substring(0, val.filename.LastIndexOf("/"))))
            {
                Directory.CreateDirectory(val.filename.Substring(0, val.filename.LastIndexOf("/")));
            }
            bool block = true;
            while (block)
            {
                try
                {

                    using (FileStream stream = new FileStream(val.filename, FileMode.OpenOrCreate))
                    {
                        try
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            using (var zipStream = new GZipStream(stream, CompressionMode.Compress))
                            {
                                try
                                {
                                    formatter.Serialize(zipStream, val.values);
                                    block = false;
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                catch
                {

                }
            }
        }

        public static TResult DeserializeFromFile<TResult>(string filename) where TResult : class
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                //MemoryStream ms = new MemoryStream();
                using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    TResult result = formatter.Deserialize(zipStream) as TResult;

                    return result;
                }
            }
        }
    }
}
