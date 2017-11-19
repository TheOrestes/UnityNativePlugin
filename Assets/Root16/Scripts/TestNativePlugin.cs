using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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

    public string imagePath;
    private IntPtr sharedAPI = IntPtr.Zero;
    private IntPtr inData = IntPtr.Zero;
    private ImageData imgData;
    private Texture2D stbTexture;


    // Use this for initialization
    void Start ()
    {
        // Create instance of DLL class
        sharedAPI = CreateSharedAPI();

        // Load an image using stb_image from DLL
        inData = LoadImage(sharedAPI, imagePath);

        string path = Marshal.PtrToStringAnsi(getImagePath(sharedAPI));

        imgData = (ImageData)Marshal.PtrToStructure(inData, typeof(ImageData));
        Debug.Log("InputPath : " + path);
        
        Debug.Log("Image Width : " + imgData._width);
        Debug.Log("Image Height : " + imgData._height);
        Debug.Log("Image BPP : " + imgData._bpp);

        
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

        // Assign texture to renderer's material.
        GetComponent<Renderer>().material.mainTexture = stbTexture;
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
