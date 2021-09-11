using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

namespace Pigeon
{
    /// <summary>
    /// XML Serialize/Deserialize utility
    /// </summary>
    public static class SaveUtility
    {
        public static string GetPersistentDataPath(string fileName)
        {
            return Application.persistentDataPath + "/" + fileName;
        }

        public static void Serialize<T>(T instance, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer.BaseStream, instance);
            }
        }

        public static T Deserialize<T>(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return (T)(new XmlSerializer(typeof(T)).Deserialize(reader.BaseStream));
            }
        }

        static void ClearScene()
        {
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();

            for (int i = 0; i < sceneObjects.Length; i++)
            {
                GameObject.Destroy(sceneObjects[i]);
            }
        }
    }
}