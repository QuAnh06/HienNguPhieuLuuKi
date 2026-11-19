using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CaptureHelper
{
    [MenuItem("Tools/Capture Scene View")]
    public static void DoCapture()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        int w = 1280;
        int h = 720;
        RenderTexture rt = new RenderTexture(w, h, 24);
        RenderTexture prev = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        cam.targetTexture = prev;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(rt);

        byte[] bytes = tex.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(tex);

        string path = @"C:\Users\Laptop\.gemini\antigravity-cli\brain\cf9fd4ae-86b8-41e9-b8f7-020aeea169b5\map_interactive_verification.png";
        File.WriteAllBytes(path, bytes);
        Debug.Log("Saved screen capture to: " + path);
    }
}
