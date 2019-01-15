using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class import : MonoBehaviour
{
    [System.Serializable]
    public class ImportObjects
    {
        public ImportObject[] importObjects;
        
    }
    [System.Serializable]
    public class ImportObject
    {
        public string name = "l";
        public string objectPath;
        public string texturePath;
       
    }

    ImportObjects importObjects;
    string importObjectJSON = "https://github.com/saint66735/some-.objs/blob/master/object.json?raw=true";
    public Transform container;
    public GameObject objectCard;
    public string objectDataFolder;

    void Start()
    {
        objectDataFolder = Path.Combine( Application.dataPath, "Downloaded_objects");
        StartCoroutine(GetText());
    }

   
    // Update is called once per frame
    void Update()
    {
    
    }
    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get(importObjectJSON);//json
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) { Debug.Log(www.error); }
        else
        {
            //string json = File.ReadAllText(Application.dataPath + "\\object.json");
            importObjects = CreateFromJSON(www.downloadHandler.text);
            PopulateContainer();
        }
    }
    public IEnumerator DownloadObject(ImportObject importObject, Button button)
    {
        button.onClick.RemoveAllListeners();
        UnityWebRequest www = UnityWebRequest.Get(importObject.objectPath);//load mesh
        yield return www.SendWebRequest();
        
        if (www.isNetworkError || www.isHttpError) { Debug.Log(www.error); }
        else
        {
            string objectPath = Path.Combine(objectDataFolder, importObject.name + ".obj");
            File.WriteAllBytes(objectPath, www.downloadHandler.data);

            www = UnityWebRequestTexture.GetTexture(importObject.texturePath); //load texture
            yield return www.SendWebRequest();
        
            if (www.isNetworkError || www.isHttpError) { Debug.Log(www.error); }
            else
            {
                string texturePath = Path.Combine(objectDataFolder, importObject.name + "_texture.png");
                //Texture loadedObjectTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                File.WriteAllBytes(texturePath, www.downloadHandler.data);
                //loadedObject.GetComponentInChildren<MeshRenderer>().material.mainTexture = loadedObjectTexture;

                if (button != null)
                {
                    button.onClick.AddListener(()=>LoadObject(importObject));
                    ColorBlock cb = button.colors;
                    cb.normalColor = Color.green;
                    button.colors = cb;
                }
            }
        
        }
    }
    public void LoadObject(ImportObject importObject)
    {
        string objectPath = objectPath = Path.Combine(objectDataFolder, importObject.name + ".obj");
        GameObject loadedObject = OBJLoader.LoadOBJFile(objectPath);
        loadedObject.name = importObject.name;

        string texturePath = Path.Combine(objectDataFolder, importObject.name + "_texture.png");
        byte[] textureBytes = File.ReadAllBytes(texturePath);
        Texture2D loadedObjectTexture = new Texture2D(1,1);
        loadedObjectTexture.LoadImage(textureBytes);
        loadedObject.GetComponentInChildren<MeshRenderer>().material.mainTexture = loadedObjectTexture;
    }
    void PopulateContainer()
    {
        foreach (var import in importObjects.importObjects)
        {
            GameObject temp = Instantiate(objectCard, container);
            Button tempButton = temp.GetComponent<Button>();
            Text text = temp.GetComponentInChildren<Text>();
            text.text = import.name;
            tempButton.gameObject.GetComponent<ButtonData>().ImportObject = import;
            tempButton.onClick.AddListener(() => StartCoroutine( DownloadObject(tempButton.gameObject.GetComponent<ButtonData>().ImportObject, tempButton)));
        }
    }
    public ImportObjects CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ImportObjects>(jsonString);
    }
}
