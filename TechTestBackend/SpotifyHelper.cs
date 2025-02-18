using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace TechTestBackend;

public static class SpotifyHelper
{
    private static HttpClient client = new HttpClient();
    private static readonly string c_id = "996d0037680544c987287a9b0470fdbb";
    private static readonly string c_s = "5a3c92099a324b8f9e45d77e919fec13";

    public static async Task<Spotifysong[]> GetTracksAsync(string name)
    {
        var e = Encoding.ASCII.GetBytes($"{c_id}:{c_s}");
        var base64 = Convert.ToBase64String(e);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

        var password = await client.PostAsync(
            "https://accounts.spotify.com/api/token",
            new FormUrlEncodedContent(
                new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") }
            )
        );
        dynamic Password_content = JsonConvert.DeserializeObject(
            await password.Content.ReadAsStringAsync()
        );

        client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            Password_content.access_token.ToString()
        );

        var response = await client.GetAsync(
            "https://api.spotify.com/v1/search?q=" + name + "&type=track"
        );
        dynamic objects = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

        var songs = JsonConvert.DeserializeObject<Spotifysong[]>(objects.tracks.items.ToString());

        return songs;
    }

    public static async Task<Spotifysong> GetTrackAsync(string id)
    {
        var e = Encoding.ASCII.GetBytes($"{c_id}:{c_s}");
        var base64 = Convert.ToBase64String(e);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

        var password = await client.PostAsync(
            "https://accounts.spotify.com/api/token",
            new FormUrlEncodedContent(
                new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") }
            )
        );
        dynamic Password_content = JsonConvert.DeserializeObject(
            await password.Content.ReadAsStringAsync()
        );

        client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            Password_content.access_token.ToString()
        );

        var response = await client.GetAsync("https://api.spotify.com/v1/tracks/" + id + "/");
        dynamic objects = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

        var song = JsonConvert.DeserializeObject<Spotifysong>(objects.ToString());

        return song;
    }
}
