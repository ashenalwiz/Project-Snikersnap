//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;
//using Newtonsoft.Json;
//using System.Xml;

//public static class SaveSystem
//{
//    private static string folderPath = Path.Combine(Application.dataPath, "GameData"); // Assets/GameData
//    private static string filePath = Path.Combine(folderPath, "UserProgress.json");

//    public static void SaveData(SessionData data)
    
//        if (!Directory.Exists(folderPath))
//        {
//            Directory.CreateDirectory(folderPath); // Create GameData folder if not exists
//        }

//        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
//        File.WriteAllText(filePath, json);

//        Debug.Log("✅ Progress Saved: " + filePath);
//    }

//    public static SessionData LoadData()
//    {
//        if (File.Exists(filePath))
//        {
//            string json = File.ReadAllText(filePath);
//            return JsonConvert.DeserializeObject<SessionData>(json);
//        }
//        else
//        {
//            Debug.LogWarning("❌ No Save File Found! Creating new data...");
//            return new SessionData();
//        }
//    }
//}
