#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace L2
{
    public class ChangeAllProjectFontWithGUID : EditorWindow
    {
        private bool inSearching = false;
        private bool isReplaceing = false;
        private List<string> needChangePrefab = new List<string>();

        //search gui
        [SerializeField]
        private List<string> searchGUID = new List<string>();
        private SerializedObject seriOBJ;
        private SerializedProperty oldGUIDSeriProp;

        //replace gui
        private string newGUID;

        //scroll view gui
        private Vector2 scrollPos;
        private string text = "";

        //progress gui
        private string progressText = "...";

        [MenuItem("Tools/Change All Project Font")]
        public static void Open()
        {
            // Get existing open window or if none, make a new one:
            var window = (ChangeAllProjectFontWithGUID)EditorWindow.GetWindow(typeof(ChangeAllProjectFontWithGUID));
            window.Show();
        }

        private void OnEnable()
        {
            seriOBJ = new SerializedObject(this);
            oldGUIDSeriProp = seriOBJ.FindProperty("searchGUID");
        }

        private void OnGUI()
        {


            GUILayout.Label("輸入要搜尋的字型的GUID:");

            seriOBJ.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(oldGUIDSeriProp, true);

            if (EditorGUI.EndChangeCheck())
            {
                seriOBJ.ApplyModifiedProperties();
            }

            GUILayout.Space(20);
            
            GUILayout.Label("輸入要取代字型的GUID");
            newGUID = EditorGUILayout.TextField("replace to: ", newGUID);

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("訊息:", GUILayout.Width(50), GUILayout.Height(15));
            if (GUILayout.Button("Clear", GUILayout.Width(50), GUILayout.Height(15)))
            {
                text = "";
            }
            EditorGUILayout.EndHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            
            GUILayout.Label(text);
            EditorGUILayout.EndScrollView();

            GUILayout.Label(progressText);

            if (inSearching == false)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("開始搜尋", GUILayout.Width(100), GUILayout.Height(30))) 
                {
                    StartSearch();
                }

                if (needChangePrefab.Count > 0 && isReplaceing == false)
                {
                    if (GUILayout.Button("開始取代", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        StartReplace();
                    }
                }
                
                GUILayout.EndHorizontal();
            }

        }

        private void StartSearch()
        {
            text = "";
            string path = Path.Combine(Application.dataPath);
            //EditorCoroutineUtility.StartCoroutine(DirSearchCor(path, true, () => 
            //{
            //    Debug.Log("done");
            //}), this);
            DirSearch(path);

            StringBuilder sb = new StringBuilder();
            foreach (var prefab in needChangePrefab)
            {
                sb.AppendLine(prefab);
            }

            File.WriteAllText(@"C:\Users\Ian.wang\Desktop\changeFontLog.txt", sb.ToString());
        }

        private void StartReplace()
        {
            isReplaceing = true;
            EditorCoroutineUtility.StartCoroutine(ReplaceCor(() => 
            {
                isReplaceing = false;
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }), this);

        }

        private void DirSearch(string _path)
        {
            if (_path.Contains("_workSpace")) return;

            //先處理當前資料夾的物件
            foreach (var file in Directory.GetFiles(_path, "*.prefab"))
            {
                var rawText = File.ReadAllText(file);
                foreach (var oldGUID in searchGUID)
                {
                    if (rawText.Contains(oldGUID))
                    {
                        text = text + "\n" + file;
                        bool isIn = needChangePrefab.Contains(file);
                        if(isIn == false)
                            needChangePrefab.Add(file);
                    }
                }
            }

            //處理子資料夾
            var folders = Directory.GetDirectories(_path);
            foreach (var d in folders)
            {
                DirSearch(d);
            }

        }

        private IEnumerator DirSearchCor(string _path, bool isFirstFolder, Action _onDone)
        {
            //先處理當前資料夾的物件
            foreach (var file in Directory.GetFiles(_path, "*.prefab"))
            {
                var rawText = File.ReadAllText(file);
                foreach (var oldGUID in searchGUID)
                {
                    if (rawText.Contains(oldGUID))
                    {
                        text = text + "\n" + file;
                        needChangePrefab.Add(file);
                    }
                }
            }

            //處理子資料夾
            var folders = Directory.GetDirectories(_path);

            foreach (var d in folders)
            {
                yield return DirSearchCor(d, false, null);
            }

        }

        private IEnumerator ReplaceCor(Action _onDone)
        {
            int count = 0;
            foreach (var prefab in needChangePrefab)
            {
                count++;
                var rawText = File.ReadAllText(prefab);
                foreach (var guid in searchGUID)
                {
                    rawText = rawText.Replace(guid, newGUID);
                    File.WriteAllText(prefab, rawText);
                }
                progressText = string.Format("{0}/{1}", count, needChangePrefab.Count);
                yield return null;

                if (count == needChangePrefab.Count)
                {
                    _onDone?.Invoke();
                }
            }

        }
    }
}
#endif
