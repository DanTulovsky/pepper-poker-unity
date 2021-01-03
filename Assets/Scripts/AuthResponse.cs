using Newtonsoft.Json; 
// {
//   "access_token": "<foo>",
//   "expires_in": 1800,
//   "refresh_expires_in": 1800,
//   "refresh_token": "<bar>",
//   "token_type": "bearer",
//   "not-before-policy": 0,
//   "session_state": "b084902e-7c66-47ca-8bbd-3b60b7b8903d",
//   "scope": "profile poker email"
// }

public class AuthResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonProperty("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }
    
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
    
    [JsonProperty("token_type")]
    public string TokenType { get; set; }
    
    [JsonProperty("not-before-policy")]
    public int NotBeforePolicy { get; set; }
    
    [JsonProperty("session_state")]
    public string SessionState { get; set; }
    
    [JsonProperty("scope")]
    public string Scope { get; set; }
}