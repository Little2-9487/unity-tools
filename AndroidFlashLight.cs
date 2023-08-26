using System;
using UnityEngine;

public class AndroidFlashLight : MonoBehaviour
{

    AndroidJavaObject cameraManager;

    private bool isFlashOn = false;

    void Start()
    {
        try
        {
            //懶得額外設定android manifest 權限，直接調用這個，unity就會自動幫你把 camera 權限加上。
            //需要的權限可見: https://developer.android.com/guide/topics/media/camera#manifest
            WebCamTexture b = new WebCamTexture(); 

            //安卓 7.0 後，要使用 camera2 的方式，才能成功控制 camera (2023/08/27) 
            //以下為取得 android 中 CameraManager 的方式。
            AndroidJavaClass contextClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = contextClass.GetStatic<AndroidJavaObject>("currentActivity");
            string cameraService = "camera";
            cameraManager = currentActivity.Call<AndroidJavaObject>("getSystemService", cameraService);

            //取得目前 手機擁有的 camera ID，通常0就是背面攝影機
            String[] cameraList = cameraManager.Call<String[]>("getCameraIdList");
            foreach (var id in cameraList)
            {
                Debug.LogError("camera id: " + id);
            }


        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message + ", " + e.StackTrace);
        }
        
    }


    public void ToggleFlashlight()
    {
        if (cameraManager != null)
        {
            try
            {
                //參數要注意型態，這邊的 id 是 string，不是 int。
                cameraManager.Call("setTorchMode", "0", !isFlashOn);
                isFlashOn = !isFlashOn;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Error toggling flashlight: " + e.Message);
            }
        }
    }
}
