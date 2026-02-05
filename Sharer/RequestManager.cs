using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Architect.Objects.Categories;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Sharer.Info;
using Architect.Sharer.States;
using Architect.Storage;
using Architect.Workshop;
using BepInEx;
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

        using var request = new UnityWebRequest(URL + (signup ? "/create" : "/login"), "POST");
        
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

        using var request = new UnityWebRequest(URL + "/status", "POST");
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

        using var request = new UnityWebRequest(URL + "/update-info", "POST");
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
                bosses = tags.Contains(LevelInfo.LevelTag.Bosses),
                minigames = tags.Contains(LevelInfo.LevelTag.Minigames)
            };
            form.AddField("tags", JsonUtility.ToJson(tagData));
        }

        // Add level files
        if (!overwriting || overwriteMode == 1)
        {
            var jsonData = StorageManager.SerializeAllScenes();
            var jsonBytes = Encoding.UTF8.GetBytes(jsonData);
            form.AddBinaryData("level", jsonBytes, "level.json", "application/json");
            
            var wJsonData = JsonConvert.SerializeObject(WorkshopManager.WorkshopData);
            var wJsonBytes = Encoding.UTF8.GetBytes(wJsonData);
            form.AddBinaryData("workshop", wJsonBytes, "workshop.json", "application/json");
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

        using var request = UnityWebRequest.Post(URL + "/upload", form);

        var operation = request.SendWebRequest();
        yield return operation;

        Dictionary<string, string> response;
        try
        {
            response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        }
        catch (JsonReaderException)
        {
            response = null;
        }

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
        public bool minigames;
    }

    private static List<LevelInfo> LoadLevels(List<Dictionary<string, string>> levelData)
    {
        List<LevelInfo> levelInfo = [];
        foreach (var level in levelData)
        {
            var info = new LevelInfo
            {
                LevelId = level["level_id"],
                LevelName = level["level_name"],
                LevelDesc = level["level_desc"],
                IconURL = level["icon_url"],
                HasSave = bool.Parse(level["has_save"]),
                CreatorName = level["username"],
                CreatorId = level["user_id"],
                DownloadCount = int.Parse(level["downloads"]),
                LikeCount = int.Parse(level["likes"]),
                Liked = bool.Parse(level["liked"])
            };
            if (level.TryGetValue("difficulty", out var difficulty))
            {
                info.Difficulty = difficulty switch
                {
                    "0" => LevelInfo.LevelDifficulty.None,
                    "1" => LevelInfo.LevelDifficulty.Easy,
                    "2" => LevelInfo.LevelDifficulty.Medium,
                    "3" => LevelInfo.LevelDifficulty.Hard,
                    "4" => LevelInfo.LevelDifficulty.ExtraHard,
                    _ => throw new ArgumentOutOfRangeException()
                };
                info.Duration = level["duration"] switch
                {
                    "0" => LevelInfo.LevelDuration.Tiny,
                    "1" => LevelInfo.LevelDuration.Short,
                    "2" => LevelInfo.LevelDuration.Medium,
                    "3" => LevelInfo.LevelDuration.Long,
                    "4" => LevelInfo.LevelDuration.None,
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (level["platforming"] == "1") info.Tags.Add(LevelInfo.LevelTag.Platforming);
                if (level["minigames"] == "1") info.Tags.Add(LevelInfo.LevelTag.Minigames);
                if (level["multiplayer"] == "1") info.Tags.Add(LevelInfo.LevelTag.Multiplayer);
                if (level["gauntlets"] == "1") info.Tags.Add(LevelInfo.LevelTag.Gauntlets);
                if (level["areas"] == "1") info.Tags.Add(LevelInfo.LevelTag.Areas);
                if (level["troll"] == "1") info.Tags.Add(LevelInfo.LevelTag.Troll);
                if (level["bosses"] == "1") info.Tags.Add(LevelInfo.LevelTag.Bosses);
            }
            levelInfo.Add(info);
        }

        return levelInfo;
    }

    /**
     * <param name="filterInfo">How levels should be filtered</param>
     * <param name="resultCount">The number of results to retrieve</param>
     * <param name="offset">The first (Offset * Results) levels are ignored</param>
     * <param name="callback">Runs when the request completes</param>
     */
    public static IEnumerator SearchLevels(
        FilterInfo filterInfo,
        int resultCount,
        int offset,
        Action<bool, List<LevelInfo>, int> callback)
    {
        var current = Time.time;
        var form = new WWWForm();
        
        form.AddField("result_count", resultCount);
        form.AddField("offset", offset);
        form.AddField("game", LEVEL_TYPE);
        
        form.AddField("key_filter", filterInfo.KeyMode.ToString());
        if (filterInfo.KeyFilter != null) form.AddField("key", filterInfo.KeyFilter);
        
        form.AddField("sorting_rule", filterInfo.SortingRule.ToString());
        
        form.AddField("incl_diff", JsonConvert.SerializeObject(filterInfo.IncludedDifficulty));
        form.AddField("excl_diff", JsonConvert.SerializeObject(filterInfo.ExcludedDifficulty));
        
        form.AddField("incl_dur", JsonConvert.SerializeObject(filterInfo.IncludedDurations));
        form.AddField("excl_dur", JsonConvert.SerializeObject(filterInfo.ExcludedDurations));
        
        form.AddField("incl_tags", JsonConvert.SerializeObject(filterInfo.IncludedTags));
        form.AddField("excl_tags", JsonConvert.SerializeObject(filterInfo.ExcludedTags));
        
        if (!filterInfo.UsernameFilter.IsNullOrWhiteSpace()) form.AddField("username", filterInfo.UsernameFilter);
        if (!filterInfo.SearchQuery.IsNullOrWhiteSpace()) form.AddField("contents", filterInfo.SearchQuery);
        
        ArchitectPlugin.Logger.LogInfo($"Prepared fields {Time.time - current}");
        current = Time.time;
        
        using var request = UnityWebRequest.Post(URL + "/search-new", form);

        var operation = request.SendWebRequest();
        ArchitectPlugin.Logger.LogInfo($"Sending request {Time.time - current}");
        current = Time.time;
        yield return operation;
        ArchitectPlugin.Logger.LogInfo($"Received response {Time.time - current}");
        current = Time.time;
        
        List<Dictionary<string, string>> response;
        try
        { 
            response = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(request.downloadHandler.text);
        }
        catch (JsonReaderException) { response = null; }
        
        if (response == null || response.IsNullOrEmpty() || response[0].ContainsKey("error"))
        {
            callback(false, null, 0);
            yield break;
        }
        ArchitectPlugin.Logger.LogInfo($"Confirmed response {Time.time - current}");
        current = Time.time;
        
        var meta = response[0];
        response.RemoveAt(0);
        var levelInfo = LoadLevels(response);
        ArchitectPlugin.Logger.LogInfo($"Finished parsing response {Time.time - current}");
        
        callback(true, levelInfo, int.Parse(meta["pages"]));
    }

    public class FilterInfo
    {
        // Used for Manage area, only return this user's levels
        public string KeyFilter;
        
        public string UsernameFilter;
        public string SearchQuery;
        
        public List<LevelInfo.LevelDifficulty> ExcludedDifficulty = [];
        public List<LevelInfo.LevelDifficulty> IncludedDifficulty = [];
        
        public List<LevelInfo.LevelDuration> ExcludedDurations = [];
        public List<LevelInfo.LevelDuration> IncludedDurations = [];
        
        public List<LevelInfo.LevelTag> ExcludedTags = [];
        public List<LevelInfo.LevelTag> IncludedTags = [];

        public Browse.SortingRule SortingRule = Browse.SortingRule.Featured;

        public bool Active;
        
        public bool KeyMode;
    }

    public static IEnumerator SendDeleteRequest(
        string name,
        Action<bool> callback)
    {
        var jsonBody = JsonUtility.ToJson(new DeleteRequestData
        {
            key = SharerKey,
            name = name
        });

        using var request = new UnityWebRequest(URL + "/delete", "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        yield return operation;

        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        callback(response != null && !response.ContainsKey("error"));
    }

    public static IEnumerator LikeLevel(string id)
    {
        var form = new WWWForm();
        
        form.AddField("level_id", id);
        form.AddField("key", SharerKey);
        
        using var request = UnityWebRequest.Post(URL + "/like", form);

        var operation = request.SendWebRequest();
        yield return operation;
    }
    
    public static IEnumerator DownloadSave(string levelId, Text status)
    {
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
        yield return operation;
        if (request.responseCode != 200)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.GetValueOrDefault("error", "Error occured when downloading");
            status.text = msg;
            yield break;
        }

        var data = request.downloadHandler.data;
        var done = new TaskCompletionSource<bool>();

        Platform.Current.WriteSaveSlot(Settings.SaveSlot.Value, data, _ => done.SetResult(true));
        UIManager.instance.slotFour.Prepare(GameManager.instance, true, false);
        
        yield return Task.WhenAll(done.Task, Task.Delay(1000));

        status.text = "Download Complete";
    }
    
    internal static IEnumerator DownloadLevel(string levelId, Text status)
    {
        status.text = "Downloading Level...";

        var jsonBody = JsonUtility.ToJson(new DownloadRequestData
        {
            level_id = levelId
        });
        
        var request = new UnityWebRequest(URL + "/download_level", "POST");

        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        yield return operation;
        if (request.responseCode != 200)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.GetValueOrDefault("error", "Error occured when downloading");
            status.text = msg;
            yield break;
        }
        
        var json = request.downloadHandler.text;

        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        
        var prefabs = JsonConvert.DeserializeObject<Dictionary<string, LevelData>>(result["level"])
            .Where(o => o.Key.StartsWith("Prefab_"));
        PrefabsCategory.Prefabs = prefabs.Select(o => 
            new PrefabObject(o.Key.Replace("Prefab_", ""))).ToList();
        
        var data = JsonConvert.DeserializeObject<Dictionary<string, LevelData>>(result["level"]);
        var wData = JsonConvert.DeserializeObject<WorkshopData>(result["workshop"]);
        
        yield return StorageManager.LoadLevelData(data, wData, status);

        PlacementManager.InvalidateScene();
    }

    [Serializable]
    public class DownloadRequestData
    {
        public string level_id;
    }

    [Serializable]
    private class DeleteRequestData
    {
        public string key;
        public string name;
    }
}