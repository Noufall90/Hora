using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveDataJson : MonoBehaviour
{
    // private PlayerData _playerData;
    // private PlayerWeapons _weaponsPlayerData;

    // void Start()
    // {
    //     PlayerData = PlayerData.Instance;
    // }

    // public void StartData()
    // {
    //     string json = JsonUtility.ToJson(_playerData);
    //     Debug.Log(json);

    //     using(StreamWriter writer = new StreamWriter(Application.persistentDataPath + Path.AltDirectorySeparatorChar +"player.json"))
    //     {
    //         writer.Write(json);
    //     }
    // }

    // public void LoadData()
    // {
    //     string json = string.Empty;

    //     using(StreamReader reader = new StreamReader(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "player.json"))
    //     {
    //         json = reader.ReadToEnd();
    //     }

    //     PlayerData data = PlayerData.Instance;
    // }
}