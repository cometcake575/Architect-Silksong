using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Architect.Placements;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Architect.Storage.Sharer;

public static class SharerRequests
{
    private const string ID = "silksong";
    private const string URL = "https://cometcake575.pythonanywhere.com";
    
    internal static async Task SendAuthRequest(string username, string password, string path, Text errorMessage)
    {
        var jsonBody = JsonUtility.ToJson(new AuthRequestData
        {
            username = username,
            password = password
        });

        var request = new UnityWebRequest(URL + path, "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        if (!response.TryGetValue("key", out var value))
        {
            errorMessage.text = response.GetValueOrDefault("error", "An unknown error occured");
            return;
        }

        LevelSharerUI.APIKey = value;
        errorMessage.text = "";
        
        LevelSharerUI.RefreshActiveOptions();
    }

    internal static async Task UploadLevel(string name, string desc, string iconUrl, int saveNumber, Text status)
    {
        var form = new WWWForm();

        form.AddField("key", LevelSharerUI.APIKey);
        form.AddField("name", name);
        form.AddField("desc", desc);
        form.AddField("url", iconUrl);
        form.AddField("game", ID);

        var jsonData = StorageManager.SerializeAllScenes();

        var jsonBytes = Encoding.UTF8.GetBytes(jsonData);
        form.AddBinaryData("level", jsonBytes, "level.json", "application/json");

        var done = new TaskCompletionSource<bool>();
        if (Platform.IsSaveSlotIndexValid(saveNumber))
        {
            Platform.Current.ReadSaveSlot(saveNumber, bytes =>
            {
                if (bytes != null)
                {
                    form.AddBinaryData("save", bytes, "save.dat", "application/octet-stream");
                }
                done.SetResult(true);
            });
        }

        await done.Task;

        var request = UnityWebRequest.Post(URL + "/upload", form);

        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();
        if (request.responseCode != 201)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.GetValueOrDefault("error", "Error occured when uploading");
            status.text = msg;
            return;
        }

        await LevelSharerUI.PerformSearch();
        status.text = "Uploaded";
    }

    internal static async Task DeleteLevel(string name, Text status)
    {
        var jsonBody = JsonUtility.ToJson(new DeleteRequestData
        {
            key = LevelSharerUI.APIKey,
            name = name,
            game = ID
        });

        var request = new UnityWebRequest(URL + "/delete", "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();
        if (request.responseCode != 201)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.GetValueOrDefault("error", "Error occured when deleting");
            status.text = msg;
            return;
        }

        await LevelSharerUI.PerformSearch();
        status.text = "Deleted";
    }
    
    internal static async Task<string> SendSearchRequest(string description, string creator)
    {
        var jsonBody = JsonUtility.ToJson(new SearchRequestData
        {
            desc = description,
            creator = creator,
            game = ID
        });

        var request = new UnityWebRequest(URL + "/search", "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        return request.downloadHandler.text;
    }
    
    internal static async Task DownloadLevel(string levelId, Text status)
    {
        LevelSharerUI.CurrentlyDownloading = true;
        LevelSharerUI.RefreshActiveOptions();
        
        status.text = "Downloading Level...";

        var jsonBody = JsonUtility.ToJson(new DownloadRequestData
        {
            level_id = levelId
        });
        
        var request = new UnityWebRequest(URL + "/download", "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        await operation;
        if (request.responseCode != 200)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.GetValueOrDefault("error", "Error occured when downloading");
            status.text = msg;
            LevelSharerUI.CurrentlyDownloading = false;
            LevelSharerUI.RefreshActiveOptions();
            return;
        }
        
        var json = request.downloadHandler.text;
        
        var data = JsonConvert.DeserializeObject<Dictionary<string, LevelData>>(json);
        ArchitectPlugin.Instance.StartCoroutine(StorageManager.LoadLevelData(data, levelId, status));

        PlacementManager.InvalidateScene();
    }
    
    internal static async Task DownloadSave(string levelId, Text status)
    {
        LevelSharerUI.CurrentlyDownloading = true;
        LevelSharerUI.RefreshActiveOptions();
        
        status.text = "Downloading Save...";

        var jsonBody = JsonUtility.ToJson(new DownloadRequestData
        {
            level_id = levelId
        });
        
        var request = new UnityWebRequest(URL + "/download_save", "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        await operation;
        if (request.responseCode != 200)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.GetValueOrDefault("error", "Error occured when downloading");
            status.text = msg;
            LevelSharerUI.CurrentlyDownloading = false;
            LevelSharerUI.RefreshActiveOptions();
            return;
        }

        var data = request.downloadHandler.data;
        var done = new TaskCompletionSource<bool>();

        Platform.Current.WriteSaveSlot(Settings.SaveSlot.Value, data, _ => done.SetResult(true));
        UIManager.instance.slotFour.Prepare(GameManager.instance, true, false);
        
        await Task.WhenAll(done.Task, Task.Delay(1000));

        status.text = "Download Complete";
        
        LevelSharerUI.CurrentlyDownloading = false;
        LevelSharerUI.RefreshActiveOptions();
    }
    
    [Serializable]
    public class SearchRequestData
    {
        public string desc;
        public string creator;
        public string game;
    }

    [Serializable]
    public class DownloadRequestData
    {
        public string level_id;
    }

    [Serializable]
    public class AuthRequestData
    {
        public string username;

        public string password;
    }

    [Serializable]
    public class DeleteRequestData
    {
        public string key;

        public string name;

        public string game;
    }
}