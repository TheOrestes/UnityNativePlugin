using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestNativePlugin : MonoBehaviour
{
    [DllImport("SharedAPI")]
    private static extern int Add(int a, int b);

    [DllImport("SharedAPI")]
    private static extern IntPtr CreateSharedAPI();
    
    [DllImport("SharedAPI")]
    private static extern int getImageWidth(IntPtr api);

    [DllImport("SharedAPI")]
    private static extern int getImageHeight(IntPtr api);

    [DllImport("SharedAPI")]
    private static extern int getImageBPP(IntPtr api);

    [DllImport("SharedAPI")]
    private static extern IntPtr getImageData(IntPtr api);

    [DllImport("SharedAPI")]
    private static extern IntPtr getImagePath(IntPtr api);

    [DllImport("SharedAPI")]
    private static extern void LoadImage(IntPtr api, string _path);

    public string imagePath;
    private IntPtr sharedAPI;
    private IntPtr imageData;
    private Texture2D stbTexture;
    
    // Use this for initialization
    void Start ()
    {
        // Create instance of DLL class
        sharedAPI = CreateSharedAPI();

        // Load an image using stb_image from DLL
        LoadImage(sharedAPI, imagePath);

        string path = Marshal.PtrToStringAnsi(getImagePath(sharedAPI));
        Debug.Log("InputPath : " + path);

        int width = getImageWidth(sharedAPI);
        int height = getImageHeight(sharedAPI);
        int bpp = getImageBPP(sharedAPI);

        Debug.Log("Image Width : " + width);
        Debug.Log("Image Height : " + height);
        Debug.Log("Image BPP : " + bpp);

        // get image data from DLL in char* format
        imageData = getImageData(sharedAPI);

        // assign memory for storing the raw data
        int byteLength = width * height * bpp;
        byte[] rawImageData = new byte[byteLength];

        // Memcpy raw data
        Marshal.Copy(imageData, rawImageData, 0, byteLength);

        // create unity's Texture2D object
        stbTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Load raw texture byte data into the texture
        stbTexture.LoadRawTextureData(rawImageData);
        stbTexture.Apply();

        // Assign texture to renderer's material.
        GetComponent<Renderer>().material.mainTexture = stbTexture;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnDestroy()
    {
        
    }
}
