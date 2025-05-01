using System;
using System.IO;
using UnityEngine;

public class PersistentDataManager : KSingleton<PersistentDataManager>
{
    private static readonly string PersistentDataPath = Application.persistentDataPath;

    /// <summary>
    /// 保存数据到JSON文件
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="fileName">文件名（无需路径）</param>
    /// <param name="data">要保存的数据对象</param>
    /// <param name="prettyPrint">是否格式化JSON（默认true）</param>
    public static void SaveData<T>(string fileName, T data, bool prettyPrint = true)
    {
        try
        {
            string fullPath = Path.Combine(PersistentDataPath, fileName);
            string jsonData = JsonUtility.ToJson(data, prettyPrint);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(jsonData);
            }
            Debug.Log($"数据已保存到：{fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存数据失败：{e.Message}");
        }
    }

    /// <summary>
    /// 从JSON文件加载数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="fileName">文件名（无需路径）</param>
    /// <returns>数据对象或默认值</returns>
    public static T LoadData<T>(string fileName) where T : new()
    {
        try
        {
            string fullPath = Path.Combine(PersistentDataPath, fileName);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"文件不存在，返回默认数据：{fullPath}");
                return new T();
            }

            using (FileStream stream = new FileStream(fullPath, FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonData = reader.ReadToEnd();
                return JsonUtility.FromJson<T>(jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"加载数据失败：{e.Message}");
            return new T();
        }
    }

    /// <summary>
    /// 删除指定数据文件
    /// </summary>
    public static void DeleteData(string fileName)
    {
        string fullPath = Path.Combine(PersistentDataPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log($"已删除文件：{fullPath}");
        }
    }
}