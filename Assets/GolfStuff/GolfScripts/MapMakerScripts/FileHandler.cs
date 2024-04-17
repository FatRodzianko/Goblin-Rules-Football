using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class FileHandler
{

    public static void SaveToJSONFile<T>(List<T> toSave, string filename)
    {
        Debug.Log("SaveToJSONFile (list): " + GetPath(filename));
        string content = JsonHelper.ToJson<T>(toSave.ToArray());
        WriteFile(GetPath(filename), content);
    }
    public static string SaveToJSON<T>(List<T> toSave)
    {
        Debug.Log("SaveToJSON (list): " );
        string content = JsonHelper.ToJson<T>(toSave.ToArray());
        return content;
    }

    public static void SaveToJSONFile<T>(T toSave, string filename)
    {
        Debug.Log("SaveToJSONFile (single): " + GetPath(filename));
        string content = JsonUtility.ToJson(toSave);
        WriteFile(GetPath(filename), content);
    }
    public static string SaveToJSON<T>(T toSave)
    {
        Debug.Log("SaveToJSON (single): ");
        string content = JsonUtility.ToJson(toSave);
        return content;
    }

    public static List<T> ReadListFromJSON<T>(string filename)
    {
        string content = ReadFile(GetPath(filename));

        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return new List<T>();
        }

        List<T> res = JsonHelper.FromJson<T>(content).ToList();

        return res;

    }

    public static T ReadFromJSONFile<T>(string filename, bool getPath = true)
    {
        string content = "";
        if (getPath)
            content = ReadFile(GetPath(filename));
        else
            content = ReadFile(filename);

        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return default(T);
        }

        T res = JsonUtility.FromJson<T>(content);

        return res;
    }

    private static string GetPath(string filename)
    {
        return Application.persistentDataPath + "/" + filename;
    }

    private static void WriteFile(string path, string content)
    {
        FileStream fileStream = new FileStream(path, FileMode.Create);

        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(content);
        }
    }

    private static string ReadFile(string path)
    {
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();
                return content;
            }
        }
        return "";
    }
    public static List<string> FindAllCustomCourses()
    {
        string[] customCourses = Directory.GetFiles(Application.persistentDataPath, "course_*");
        return customCourses.ToList();
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}