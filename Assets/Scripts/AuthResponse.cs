using Newtonsoft.Json; 
// {
//   "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICIwU1JBcjFRZVEzSU9oSExfb0tfdFFDaHFWNkxDYzI2eVllaTdRSGNWZHJrIn0.eyJleHAiOjE2MDk2OTIyMzksImlhdCI6MTYwOTY5MDQzOSwianRpIjoiODU4YTc2M2ItMDRjNy00YjQzLTg1NzQtYmJjNTg2NmFjOTE2IiwiaXNzIjoiaHR0cHM6Ly9sb2dpbi53ZXRzbm93LmNvbS9hdXRoL3JlYWxtcy93ZXRzbm93IiwiYXVkIjoicGVwcGVyLXBva2VyLWdycGMiLCJzdWIiOiJiMjYyMzU0ZC0xZTMxLTQ1YWEtYTMxNy1lZTJhOGM2YmJmMjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJwZXBwZXItcG9rZXItZ3JwYy53ZXRzbm93LmNvbSIsInNlc3Npb25fc3RhdGUiOiJiMDg0OTAyZS03YzY2LTQ3Y2EtOGJiZC0zYjYwYjdiODkwM2QiLCJhY3IiOiIxIiwicmVzb3VyY2VfYWNjZXNzIjp7InBlcHBlci1wb2tlci1ncnBjLndldHNub3cuY29tIjp7InJvbGVzIjpbInVzZXIiXX19LCJzY29wZSI6InByb2ZpbGUgcG9rZXIgZW1haWwiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6Ik1yIFdldHNub3ciLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJtcndldHNub3ciLCJnaXZlbl9uYW1lIjoiTXIiLCJmYW1pbHlfbmFtZSI6IldldHNub3ciLCJlbWFpbCI6Im1yd2V0c25vd0B3ZXRzbm93LmNvbSJ9.F1Y1XlntjPWsWuDj85CyH1gAPZZz0UpBtmxhrmm7is0-23UaOtEDhX2_pyjKNp3748v4mjIUodu-rshY3tfvb0r2WvF-9AYt4GWU2CQqeBqTr4aktTWmlg8wB6MOPKTopyUn1vaYE9aJ9jTqibdx2-XqNuw8kf7IrbvrE36epdlUIOKiIKO06LUNlxOkuJPvZ2enSJQHimAAhotBF0-ZJlemhXvQ4MZv74dAeZf3inkiJOitHoVQfCsH2YL0NIQwP9Z9hhphY2UlRD_XY82IFuE5OJT2abvpyfTi9U0_O5rvOkZMi2waF5KpiNDANEj2QHiGbdTx0bh6GR_IDinvcg",
//   "expires_in": 1800,
//   "refresh_expires_in": 1800,
//   "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJiNTQ1MGRmOS1mNjM2LTQwMGUtOTBkZS00OGRhOTQ3YjAwZWMifQ.eyJleHAiOjE2MDk2OTIyMzksImlhdCI6MTYwOTY5MDQzOSwianRpIjoiNjFkMzM4ZGEtMGZiYy00NTZmLWE5MzktZTczNjVjZTllYjRhIiwiaXNzIjoiaHR0cHM6Ly9sb2dpbi53ZXRzbm93LmNvbS9hdXRoL3JlYWxtcy93ZXRzbm93IiwiYXVkIjoiaHR0cHM6Ly9sb2dpbi53ZXRzbm93LmNvbS9hdXRoL3JlYWxtcy93ZXRzbm93Iiwic3ViIjoiYjI2MjM1NGQtMWUzMS00NWFhLWEzMTctZWUyYThjNmJiZjI2IiwidHlwIjoiUmVmcmVzaCIsImF6cCI6InBlcHBlci1wb2tlci1ncnBjLndldHNub3cuY29tIiwic2Vzc2lvbl9zdGF0ZSI6ImIwODQ5MDJlLTdjNjYtNDdjYS04YmJkLTNiNjBiN2I4OTAzZCIsInNjb3BlIjoicHJvZmlsZSBwb2tlciBlbWFpbCJ9.fhe6R_wdLkZZEP3BVDxnrqCMOHwo6EGyETbDKSftA5M",
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