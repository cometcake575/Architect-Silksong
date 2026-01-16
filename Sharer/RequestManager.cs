using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Architect.Storage;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Architect.Sharer;

public static class RequestManager
{
    public const string URL = "https://cometcake575.pythonanywhere.com";
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

    [Serializable]
    private class AuthRequestData
    {
        public string username;
        public string password;
    }
}