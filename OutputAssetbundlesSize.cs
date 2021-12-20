/*  ===================================================================================================
 *  
 *  利用 Assetbundle Manifest 來輸出紀錄所有 Assetbundle 檔案大小的紀錄檔案(json格式)
 *  需要 Newtonsoft.Json 套件。
 *  需要 Editor Coroutines 套件( Package Manager 中下載)。
 *  沒有 Package Manager 的話可以使用以下 Script 
 *  @ref https://gist.github.com/benblo/10732554
 *  
 *  Use assetbundle manifest to output the json file that recored all assetbundles size.
 *  dependencies: 
 *      Newtonsoft.Json
 *      Editor Coroutines package (download from package manager)
 *  note:       
 *      if you use old unity version that dont have package manager, then use script on below link
 *  @ref https://gist.github.com/benblo/10732554
 *  
 *  ===================================================================================================*/

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace L2
{
    public class OutputAssetbundlesSize
    {
        [MenuItem("Tools/Output Assetbundles Size File")]
        public static void OutputAssetbundleSizeFile()
        {
            var folderPath = EditorUtility.OpenFolderPanel("select assetbundle folder", "", "");

            var manifestPath = EditorUtility.OpenFilePanel("select manifest file", "", "");

            EditorCoroutineUtility.StartCoroutineOwnerless(Load(folderPath, manifestPath));
        }

        #region Output Assetbundle size file
        static IEnumerator Load(string _assetbundlesPath, string _manifestPath)
        {
            string assetbundlePath = _assetbundlesPath;
            string manifestPath = Path.Combine(@"file://" + _manifestPath);

            var rq = UnityWebRequestAssetBundle.GetAssetBundle(manifestPath);
            yield return rq.SendWebRequest();
            if (string.IsNullOrEmpty(rq.error))
            {
                var ab = DownloadHandlerAssetBundle.GetContent(rq);
                if (ab != null)
                {
                    var abm = ab.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                    WriteManifestLog(abm, assetbundlePath);
                    ab.Unload(true);
                }
            }
            else Debug.LogError("load assetbundle error: " + rq.error);
        }

        static void WriteManifestLog(AssetBundleManifest manifest, string outputPath)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            Dictionary<string, long> sizeDict = new Dictionary<string, long>();
            foreach (string assetBundleName in manifest.GetAllAssetBundles())
            {
                string filePath = Path.Combine(outputPath, assetBundleName);
                long size = 0;
                if (File.Exists(filePath))
                {
                    FileInfo fi = new FileInfo(filePath);
                    size = fi.Length;
                }
                sizeDict.Add(assetBundleName, size);
            }

            string jsFilePath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(outputPath));
            jsFilePath += ".size.json";
            File.WriteAllText(jsFilePath, JsonConvert.SerializeObject(sizeDict, Formatting.Indented));

            Debug.Log("out put size recored file done");
        }
        #endregion
    }
}
