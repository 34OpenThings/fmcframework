using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class FMCSaveSystem
{
    public const string FileName = "/savegame.fmc";

    SaveFile currentSaveFile;

    public FMCSaveSystem()
    {
        currentSaveFile = LoadFromDisk() ?? new SaveFile();
    }

    public void Write(string key, object value)
    {
        if (currentSaveFile != null)
        {
            bool found = false;
            foreach (SaveDataRow row in currentSaveFile)
            {
                if (row.key == key)
                {
                    row.value = value;
                    found = true;
                    break;
                }
            }

            if(!found)
                currentSaveFile.Add(new SaveDataRow(key, value));
        }

        SaveToDisk(currentSaveFile);
    }

    /// <summary>
    /// Use this to read a property.
    /// Specify in the second parameter what is the default value for this property.
    /// </summary>
    public T Read<T>(string key, T defaultValue)
    {
        if (currentSaveFile != null)
        {
            foreach (SaveDataRow sr in currentSaveFile)
            {
                if (key == sr.key && sr.value is T)
                    return (T)sr.value;
            }
        }

        return defaultValue;
    }

    void SaveToDisk(SaveFile saveFileToWrite)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
            FileStream file = File.Create(Application.persistentDataPath + FileName); //you can call it anything you want
            bf.Serialize(file, saveFileToWrite);
            file.Close();
        }
        catch
        {
            //TODO log error message
        }
    }

    SaveFile LoadFromDisk()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + FileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + FileName, FileMode.Open);
                SaveFile newSaveFile = (SaveFile)bf.Deserialize(file);
                file.Close();
                return newSaveFile;
            }
            else return null;
        }
        catch
        {
            //TODO log error message
            return null;
        }
    }

}

[System.Serializable]
public class SaveFile : List<SaveDataRow> {}

/// <summary>
/// Defines a setting, which is composed of
/// - a KEY, which uniquely identifies the setting
/// - an OBJECT, which is the setting itself
/// </summary>
[System.Serializable]
public class SaveDataRow
{
    public string key;
    public object value;

    public SaveDataRow(string key, object value)
    {
        this.key = key;
        this.value = value;
    }
}
