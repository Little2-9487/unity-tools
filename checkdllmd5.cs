/*  ==============================================================
 *  利用 Android 原生功能以及 AndroidJNI  來取得 Assembly-CSharp.dll
 *  用來對其做 Md5 檢查。
 *  ==============================================================*/
public void Check()
{
    var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
    var assetManager = activity.Call<AndroidJavaObject>("getAssets");
    var stream = assetManager.Call<AndroidJavaObject>("open", "bin/Data/Managed/Assembly-CSharp.dll");

    //獲取檔案長度
    var availableBytes = stream.Call<int>("available");
    //取得InputStream.read的MethodID
    var clsPtr = AndroidJNI.FindClass("java/io/InputStream");
    var METHOD_read = AndroidJNIHelper.GetMethodID(clsPtr, "read", "([B)I");
    //申請一個Java ByteArray物件控制代碼
    var byteArray = AndroidJNI.NewByteArray(availableBytes);
    //呼叫方法
    int readCount = AndroidJNI.CallIntMethod(stream.GetRawObject(), METHOD_read, new[] { new jvalue() { l = byteArray } });
    //從Java ByteArray中得到C# byte陣列
    var bytes = AndroidJNI.FromByteArray(byteArray);
    //刪除Java ByteArray物件控制代碼
    AndroidJNI.DeleteLocalRef(byteArray);
    //關閉檔案流
    stream.Call("close");
    stream.Dispose();

    string rs;
    using (var md5 = MD5.Create())
    {
        var hash = md5.ComputeHash(bytes);
        rs = System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}