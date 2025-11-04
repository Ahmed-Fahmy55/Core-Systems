using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class Zone8Helper
{
    /// <summary>
    /// Downloads an image from the given URL asynchronously and returns the resulting Sprite.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to download.</param>
    /// <returns>A Task that resolves to the downloaded Sprite, or null if failed.</returns>
    public static async Task<Sprite> DownloadImageAsSpriteAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.LogError("Emp URL provided.");
            return null;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var operation = request.SendWebRequest();

            // Await until the request is done
            while (!operation.isDone)
                await Task.Yield();

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
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    // Create a Sprite from the downloaded texture
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
    }

    public static async Task<Texture2D> DownloadImageAsTextureAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.LogError("Emp URL provided.");
            return null;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var operation = request.SendWebRequest();

            // Await until the request is done
            while (!operation.isDone)
                await Task.Yield();

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
            else
            {
                return DownloadHandlerTexture.GetContent(request);
            }
        }
    }

    public static async Task<bool> RunWithTimeout(Task task, int timeoutMs)
    {
        var delayTask = Task.Delay(timeoutMs);
        var finished = await Task.WhenAny(task, delayTask);

        if (finished == delayTask)
        {
            Logger.LogError($"Task timed out after {timeoutMs / 1000f} seconds.");
            return false;
        }

        return true;
    }

    public static async Task<bool> RunWithTimeout<T>(Task<T> task, int timeoutMs, Action<T> onComplete = null)
    {
        var delayTask = Task.Delay(timeoutMs);
        var finished = await Task.WhenAny(task, delayTask);

        if (finished == delayTask)
        {
            Logger.LogError($"Task timed out after {timeoutMs / 1000f} seconds.");
            return false;
        }

        if (task.IsCompletedSuccessfully)
            onComplete?.Invoke(task.Result);

        return true;
    }


    public static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;
        return Color.white; // fallback
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
