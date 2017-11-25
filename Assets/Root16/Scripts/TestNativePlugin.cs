using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class TestNativePlugin : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ImageData
    {
        public int _width;
        public int _height;
        public int _bpp;
        public IntPtr _data;
    }

    [DllImport("ImageLoader", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr CreateSharedAPI();

    [DllImport("ImageLoader", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getImagePath(IntPtr api);

    [DllImport("ImageLoader", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr LoadImage(IntPtr api, string _path);

    [DllImport("ImageLoader", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ReleaseMemory(IntPtr api);

    [DllImport("ImageLoader", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetDebugFunction(IntPtr fp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(string str);

    public string imagePath;
    private IntPtr sharedAPI = IntPtr.Zero;
    private IntPtr inData = IntPtr.Zero;
    private ImageData imgData;
    private Texture2D stbTexture;

    private WWW www_path;
    public Text debugText;

    static void DebugCallbackFunction(string str)
    {
        Debug.Log("FROM NATIVE ========>> " + str);
    }

    // Use this for initialization
    void Start ()
    {
        DebugDelegate callback_delegate = new DebugDelegate(DebugCallbackFunction);
        IntPtr intptr_delegate = Marshal.GetFunctionPointerForDelegate(callback_delegate);
        SetDebugFunction(intptr_delegate);

        // Create instance of DLL class
        sharedAPI = CreateSharedAPI();

        // Load an image using stb_image from DLL
        string fullPath = string.Empty;
        if(Application.platform == RuntimePlatform.WindowsEditor)
        {
            //fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, imagePath);
            LoadImage();
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            LoadImage();
        }

        //debugText.text = fullPath + debugText.text;
        
    }

    void LoadImage()
    {        
        Debug.Log("dataPath : " + Application.dataPath);
        Debug.Log("persistentDataPath : " + Application.persistentDataPath);
        Debug.Log("streamingAssetsPath : " + Application.streamingAssetsPath);

        inData = LoadImage(sharedAPI, "/storage/emulated/0/videos/uv_map_reference.jpg");
        
        imgData = (ImageData)Marshal.PtrToStructure(inData, typeof(ImageData));
        
        if(imgData._width == -1 || imgData._height == -1 || imgData._bpp == -1)
        {
            Debug.Log("Error loading image");
        }
        else
        {
            // assign memory for storing the raw data
            int byteLength = imgData._width * imgData._height * imgData._bpp;
            byte[] rawImageData = new byte[byteLength];

            // Memcpy raw data
            Marshal.Copy(imgData._data, rawImageData, 0, byteLength);

            // create unity's Texture2D object
            stbTexture = new Texture2D(imgData._width, imgData._height, TextureFormat.RGB24, false);

            // Load raw texture byte data into the texture
            stbTexture.LoadRawTextureData(rawImageData);
            stbTexture.Apply();

            Debug.Log("Image Width : " + imgData._width);
            Debug.Log("Image Height : " + imgData._height);
            Debug.Log("Image BPP : " + imgData._bpp);

            // Assign texture to renderer's material.
            GetComponent<Renderer>().material.mainTexture = stbTexture;
        }
    }

	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnDestroy()
    {
        ReleaseMemory(sharedAPI);
    }
}
