using System;
using System.Collections;
using Newtonsoft.Json;

namespace Architect.Sharer.Info;

public class UserInfo(string userId, bool usingToken)
{
    public string UserID = userId;

    public bool UsingToken = usingToken;
    
    public string Username;
    
    public string Description;

    public string PfpUrl;

    public bool IsSetup;

    public string GetRequestJson()
    {
        return UsingToken
            ? JsonConvert.SerializeObject(new Token
            {
                token = UserID
            })
            : JsonConvert.SerializeObject(new Key
            {
                key = UserID
            });
    }

    [Serializable]
    private class Token
    {
        public string token;
    }
    
    [Serializable]
    private class Key
    {
        public string key;
    }

    public IEnumerator Setup()
    {
        if (IsSetup) yield break;
        yield return RequestManager.GetUserInfo(this, (user, desc, pfp) =>
        {
            Username = user;
            Description = desc;
            PfpUrl = pfp;
            IsSetup = true;
        });
    }
}