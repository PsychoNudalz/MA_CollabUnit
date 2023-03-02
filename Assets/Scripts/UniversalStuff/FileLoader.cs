using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileLoader
{

    public static void SaveToFile<T>(string path, string filename, T objectToBeSaved)
    {
        string saveString = JsonUtility.ToJson(objectToBeSaved);
        // Debug.Log("Saving all cards");
        Debug.Log(saveString);
        if (CreateFile(path, filename, saveString))
        {
            Debug.Log($"Save {filename} complete");
        }
        else
        {
            Debug.LogError($"Save {filename} Failed");
        }
    }

    /// <summary>
    /// create file at path
    /// false if failed
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="saveString"></param>
    /// <returns></returns>
    public static bool CreateFile(string path, string filename, string saveString)
    {

        try
        {
            File.WriteAllText(path + filename, saveString);
            return true;
        }
        catch (DirectoryNotFoundException e)
        {
            Directory.CreateDirectory(path);
            File.WriteAllText(path + filename, saveString);
            return false;

        }
    }

    public static bool BackupFile(string path, string filename, string backupPath)
    {
        string loadString = "";
        string[] filenameSplit = filename.Split('.');
        string backupFileName = filenameSplit[0] + "_Backup_" + DateTime.Now.Ticks + "." + filenameSplit[1];
        Debug.Log($"Backing up File: {backupFileName}");
        try
        {
            loadString = File.ReadAllText(path + filename);
            if (CreateFile(backupPath, backupFileName, loadString))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (FileNotFoundException e)
        {
            Debug.LogWarning($"Failed to find {path + filename}");

            return false;
        }
    }

    public static T LoadFromFile<T>(string pathWithName)
    {
        string loadString = "";
        try
        {
            loadString = File.ReadAllText(pathWithName);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogWarning("Failed to find save file, loading default save");

            return default(T);
        }
        return JsonUtility.FromJson<T>(loadString);
    }




    public static int[] StringSplitToInt(string stringArray, char c)
    {
        string[] tempStringArray = stringArray.Split(c);
        List<int> returnInt = new List<int>();
        string temp;
        foreach (string s in tempStringArray)
        {
            temp = s.Replace(" ", "");
            try
            {
                if (!temp.Equals(""))
                {

                    returnInt.Add(int.Parse(temp));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        return returnInt.ToArray();
    }

    


    public static object EmptyDataCheck(string s, object defaultValue)
    {
        if (s.Equals(""))
        {
            return defaultValue;
        }
        else
        {
            return s;
        }

    }
    public static string EmptyDataCheckString(string s, string i)
    {
        if (s.Equals(""))
        {
            return i;
        }
        else
        {
            return s;
        }

    }

    /// <summary>
    /// Get and loads file of a certain type from Resources
    /// </summary>
    /// <typeparam name="T"> type of return component</typeparam>
    /// <param name="path"> path to file</param>
    /// <param name="fileType">type of file, in regular expressions, eg *.prefab</param>
    /// <returns> list of components specified </returns>
    public static List<T> GetAllFilesFromResources<T>(string path, string fileType = "*.prefab", bool displayDebug = true)
    {
        List<T> fileList = new List<T>();

        string[] filePaths = Directory.GetFiles(Application.dataPath + "/Resources/" + path, fileType, SearchOption.AllDirectories);
        GameObject loadGO;
        string temp;
        foreach (string filePath in filePaths)
        {
            temp = filePath;
            temp = temp.Replace(Application.dataPath + "/Resources/", "");
            temp = temp.Replace(fileType.Substring(1), "");
            loadGO = Resources.Load<GameObject>(temp) as GameObject;

            if (!loadGO)
            {
                Debug.LogError($"Fail to load from resource {temp}");
            }
            else if (loadGO.TryGetComponent<T>(out T component))
            {
                fileList.Add(component);
            }
        }

        if (displayDebug)
        {
            Debug.Log($"Found all files of type {fileType}, containing: {fileList.Count} files return");
        }
        return fileList;
    }

    public static string ConvertToCSVSafe(string input)
    {
        input = input.Replace(",", "<COMMA>");
        input = input.Replace("\n", "<NEWLINE>").Replace("\r", " ");
        input = input.Replace("?", "<QUESTION>");
        input = input.Replace("!", "<EXMARK>");
        return input;
    }

    public static string ConvertFromCSVSafe(string input)
    {
        input = input.Replace("<COMMA>", ",");
        input = input.Replace("<NEWLINE>","\n");
        input = input.Replace("<QUESTION>","?");
        input = input.Replace("<EXMARK>", "!");
        return input;
    }
}
