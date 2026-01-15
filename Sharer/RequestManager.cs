using System.Collections;
using Architect.Storage;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Sharer;

public static class RequestManager
{
    // TODO Set this
    public const string URL = "";
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
        // TODO Add login process
        yield return new WaitForSeconds(0.5f);
        result.text = "Sample Text";
    }
}