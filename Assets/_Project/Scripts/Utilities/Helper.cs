using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class Helper
{
    public static string GetQueryParam(string url, string key)
    {
        if (string.IsNullOrEmpty(url)) return null;

        var uri = new System.Uri(url);
        string query = uri.Query;

        if (string.IsNullOrEmpty(query) || query.Length < 1)
        {
            return "";
        }

        // Safely remove the '?'
        query = query.Substring(1);

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var kv = pair.Split('=');
            if (kv.Length == 2 && kv[0] == key)
            {
                return UnityEngine.Networking.UnityWebRequest.UnEscapeURL(kv[1]);
            }
        }
        return null;
    }


    /// <summary>
    /// Downloads an image from the given URL asynchronously and returns the resulting Sprite.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to download.</param>
    /// <returns>A Task that resolves to the downloaded Sprite, or null if failed.</returns>
    public static async Awaitable<Sprite> DownloadImageAsSpriteAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.LogWarning("Emp URL provided.");
            return null;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError($"Failed to download image: {imageUrl}: {request.error}");

                if (request.downloadHandler != null)
                {
                    byte[] data = request.downloadHandler.data;
                    Logger.LogError($"failed to donwload Bytes: {data?.Length}");
                }
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                texture.name = "Downloaded_Sprite_Tex";
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            else
            {
                Logger.LogError("Downloaded texture is null.");
                return null;
            }

        }
    }

    /// <summary>
    /// A WebGL-safe delay that uses Unity's internal clock.
    /// Works on all platforms without hanging.
    /// </summary>
    /// <param name="seconds">Duration in seconds.</param>
    public static async Awaitable Delay(float seconds, CancellationToken token = default)
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(seconds, token);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation if needed, or let it bubble up
        }
    }

    public static async Awaitable<Texture2D> DownloadImageAsTextureAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.LogError("Emp URL provided.");
            return null;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError($"Failed to download image: {imageUrl}: {request.error}");
                Debug.Log($"Status Code: {request.responseCode}");
                Debug.Log($"Error: {request.error}");

                if (request.downloadHandler != null)
                {
                    byte[] data = request.downloadHandler.data;
                    Debug.Log($"Downloaded Bytes: {data?.Length}");
                }
                return null;
            }

            return DownloadHandlerTexture.GetContent(request);
        }
    }


    public static async Awaitable<(bool completed, T result)> RunWithTimeout<T>(
        Awaitable<T> task, int timeoutMs, Action<T> onComplete = null, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(timeoutMs);

        try
        {
            // Awaiting the task with the linked token
            // Note: The task itself must support the CancellationToken to actually stop
            T result = await task;

            onComplete?.Invoke(result);
            return (true, result);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning($"Task timed out or was cancelled after {timeoutMs}ms");
            return (false, default);
        }
        catch (Exception e)
        {
            Logger.LogError($"Task faulted: {e.Message}");
            return (false, default);
        }
    }


    public static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;
        return Color.clear; // fallback
    }

    public static void Shuffle<T>(this List<T> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Shuffle<T>(this T[] array)
    {
        System.Random rng = new();

        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }
}
