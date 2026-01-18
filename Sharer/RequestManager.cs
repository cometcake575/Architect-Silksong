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

    public static IEnumerator GetUserInfo(UserInfo info, Action<bool, string, string, string> callback)
    {
        var jsonBody = info.GetRequestJson();

        yield return null;
        callback(true, "cometcake575", "he/him\n\nThis is a test description to see how well it works. " +
                                       "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus ultrices ligula id erat imperdiet convallis. Nunc ex lorem, interdum vel tempor a, mollis sit amet ante. Fusce nec elit lobortis, efficitur augue tempus, pellentesque est. Praesent scelerisque felis eu libero iaculis tincidunt. Nulla in vehicula libero. Sed congue facilisis metus eu sodales. Donec libero elit, molestie id vulputate eu, vehicula sit amet metus. Quisque malesuada nibh et suscipit aliquet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc sem ipsum, posuere a justo in, tristique vehicula felis. Sed id tempor dolor. In eget tortor non massa fringilla porta.\n\nUt id accumsan purus. Vestibulum malesuada lectus eget purus pretium, eu bibendum odio iaculis. Phasellus vitae lacus libero. Pellentesque nec risus et diam egestas fermentum efficitur vitae ligula. Pellentesque convallis rutrum lacus, a cursus nulla facilisis eget. Nunc nec accumsan mauris, efficitur tincidunt erat. Nunc sit amet neque id odio congue imperdiet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam rhoncus lorem et odio aliquet, ut finibus lectus semper. Nullam scelerisque dolor est, sit amet porta enim rutrum at. Suspendisse potenti. Vivamus gravida fermentum tempor. Aliquam aliquam et orci ut pharetra. Sed tincidunt malesuada dolor maximus molestie. Nulla rutrum interdum libero. ", "file:///Users/arunkapila/Downloads/pfp.png");
    }
    
    [Serializable]
    private class AuthRequestData
    {
        public string username;
        public string password;
    }
}