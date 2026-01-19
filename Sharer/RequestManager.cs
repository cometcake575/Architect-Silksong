using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Architect.Sharer.Info;
using Architect.Storage;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Architect.Sharer;

public static class RequestManager
{
    // TODO Change
    public const string URL = "http://127.0.0.1:5000";
    public const string LEVEL_TYPE = "silksong";
    
    [CanBeNull] private static string _sharerKey = StorageManager.LoadSharerKey();

    [CanBeNull]
    public static string SharerKey
    {
        get => _sharerKey;
        set
        {
            StorageManager.SaveApiKey(value);
            _sharerKey = value;
        }
    }

    public static IEnumerator Login(bool signup, string user, string pw, Text result)
    {
        var jsonBody = JsonUtility.ToJson(new AuthRequestData
        {
            username = user,
            password = pw
        });

        var request = new UnityWebRequest(URL + (signup ? "/create" : "/login"), "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        yield return operation;

        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        if (!response.TryGetValue("key", out var value))
        {
            result.text = response.GetValueOrDefault("error", "An unknown error occured");
            yield break;
        }

        SharerKey = value;
        result.text = signup ? "Account Created" : "Logged In";
    }

    public static IEnumerator GetUserInfo(UserInfo info, Action<string, string, string> callback)
    {
        var jsonBody = info.GetRequestJson();

        var request = new UnityWebRequest(URL + "/status", "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        yield return operation;

        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        if (response.ContainsKey("error")) yield break;

        callback(response["username"], response["description"], response["pfp"]);
    }
    
    [Serializable]
    private class AuthRequestData
    {
        public string username;
        public string password;
    }

    public static IEnumerator SendChangeRequest(string changeType, string newValue, Action<bool> callback)
    {
        var jsonBody = JsonConvert.SerializeObject(new ChangeRequestData
        {
            key = SharerKey,
            changeType = changeType,
            newValue = newValue
        });

        var request = new UnityWebRequest(URL + "/update-info", "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        yield return operation;

        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        
        callback(!response.ContainsKey("error"));
    }
    
    [Serializable]
    private class ChangeRequestData
    {
        public string key;
        public string changeType;
        public string newValue;
    }
}