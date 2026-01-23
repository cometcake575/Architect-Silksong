using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        if (response == null || !response.TryGetValue("key", out var value))
        {
            result.text = response?.GetValueOrDefault("error", "An unknown error occured")
                          ?? "An unknown error occured";
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

        if (operation.webRequest.result != UnityWebRequest.Result.Success) yield break;

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

    public static IEnumerator UploadLevel(
        string name,
        string desc,
        string iconUrl,
        int saveNumber,
        LevelInfo.LevelDifficulty difficulty,
        LevelInfo.LevelDuration duration,
        List<LevelInfo.LevelTag> tags,
        Action<bool, string> callback,
        [CanBeNull] string overwriteLevelId,
        int overwriteMode)
    {
        var overwriting = overwriteLevelId != null;
        
        var form = new WWWForm();

        // Add user key
        form.AddField("key", SharerKey);

        // Indicates this is the Silksong level sharer
        form.AddField("game", LEVEL_TYPE);
        
        // Overwriting existing level
        if (overwriting)
        {
            form.AddField("overwrite", overwriteLevelId);
            form.AddField("overwrite_mode", overwriteMode);
        }

        // Add level info
        if (!overwriting || overwriteMode == 0)
        {
            // Level info
            form.AddField("name", name);
            form.AddField("desc", desc);
            form.AddField("url", iconUrl);

            // Tags
            var tagData = new LevelTagData
            {
                difficulty = (int)difficulty,
                duration = (int)duration,
                platforming = tags.Contains(LevelInfo.LevelTag.Platforming),
                multiplayer = tags.Contains(LevelInfo.LevelTag.Multiplayer),
                gauntlets = tags.Contains(LevelInfo.LevelTag.Gauntlets),
                areas = tags.Contains(LevelInfo.LevelTag.Areas),
                troll = tags.Contains(LevelInfo.LevelTag.Troll),
                bosses = tags.Contains(LevelInfo.LevelTag.Bosses)
            };
            form.AddField("tags", JsonUtility.ToJson(tagData));
        }

        // Add level files
        if (!overwriting || overwriteMode == 1)
        {
            var jsonData = StorageManager.SerializeAllScenes();
            var jsonBytes = Encoding.UTF8.GetBytes(jsonData);
            form.AddBinaryData("level", jsonBytes, "level.json", "application/json");
        }

        // Check if save slot is valid, add if so
        var done = new TaskCompletionSource<bool>();
        if ((!overwriting || overwriteMode == 2) && Platform.IsSaveSlotIndexValid(saveNumber))
        {
            Platform.Current.ReadSaveSlot(saveNumber, bytes =>
            {
                if (bytes != null)
                {
                    form.AddBinaryData("save", bytes, "save.dat", "application/octet-stream");
                }

                done.SetResult(true);
            });
            yield return done.Task;
        }

        var request = UnityWebRequest.Post(URL + "/upload", form);

        var operation = request.SendWebRequest();
        yield return operation;

        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);

        string value = null;
        if (response == null || response.TryGetValue("error", out value))
        {
            callback(false, value ?? "An unknown error occured");
            yield break;
        }

        callback(true, response["message"]);
    }

    [Serializable]
    private class LevelTagData
    {
        public int difficulty;
        public int duration;
        public bool platforming;
        public bool multiplayer;
        public bool gauntlets;
        public bool areas;
        public bool troll;
        public bool bosses;
    }

    /**
     * <param name="filterInfo">How levels should be filtered</param>
     * <param name="results">The number of results to retrieve</param>
     * <param name="offset">The first (Offset * Results) levels are ignored</param>
     * <param name="callback">Runs when the request completes</param>
     */
    public static IEnumerator SearchLevels(
        FilterInfo filterInfo,
        int results,
        int offset,
        Action<bool, List<LevelInfo>> callback)
    {
        yield return null;

        callback(true, []);
    }

    public class FilterInfo
    {
        // Used for Manage area, only return this user's levels
        public string KeyFilter;
        
        public string UsernameFilter;
        public string SearchQuery;
        
        public List<LevelInfo.LevelDifficulty> ExcludedDifficulty;
        public List<LevelInfo.LevelDifficulty> IncludedDifficulty;
        
        public List<LevelInfo.LevelDuration> ExcludedDurations;
        public List<LevelInfo.LevelDuration> IncludedDurations;
        
        public List<LevelInfo.LevelTag> ExcludedTags;
        public List<LevelInfo.LevelTag> IncludedTags;
    }
}