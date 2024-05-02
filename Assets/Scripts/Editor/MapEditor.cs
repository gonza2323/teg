using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Xml;
using Codice.Client.BaseCommands;
using System.Linq;
using Unity.Properties;
using UnityEngine.Rendering;
using NUnit.Framework;


public class MapEditor : EditorWindow
{
    GameObject mapObject;
    GameObject countryPrefab;


    // Estructura que se serializa a json para guardar varios países
    [System.Serializable]
    class CountriesSaveData
    {
        public List<CountrySaveData> countries = new List<CountrySaveData>();
    }


    // Estructura que se serializa a json para guardar un país
    [System.Serializable]
    public class CountrySaveData
    {
        public string id;
        public string name;
        public List<string> neighbors= new List<string>();
        public string color;
    }


    // Representación intermedia de un país
    class CountryData
    {
        public string id = "invalid";
        public string name = "invalid";
        public List<string> neighbors = new List<string>();
        public Vector2 position = Vector2.zero;
        public Vector2 indicatorPos = Vector2.zero;
        public Sprite sprite;
    }


    // No sé cómo funciona esto, pero es para
    // poder crear la ventana del editor
    [MenuItem("Tools/Map Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MapEditor));
    }


    // Interfaz. Tampoco entiendo mucho pero funciona
    public void OnGUI()
    {
        GUILayout.Label("Map generator", EditorStyles.boldLabel);

        // Referencias
        mapObject = EditorGUILayout.ObjectField("Map Object", mapObject, typeof(GameObject), true) as GameObject;
        countryPrefab = EditorGUILayout.ObjectField("Country Prefab", countryPrefab, typeof(GameObject), false) as GameObject;

        // Botones
        if (GUILayout.Button("Save Map"))
            if (mapObject != null)
                SaveMap();
            else
                Debug.LogError("Map object not set");

        if (GUILayout.Button("Load Map"))
        {
            if (countryPrefab != null)
                LoadMap();
            else
                Debug.LogError("Country prefab not set");
        }
    }


    // Guarda el mapa actual. Los sprites en una textura
    // y los datos en .json.
    private void SaveMap()
    {
        CountriesSaveData countriesData = new CountriesSaveData();


        // A cada objecto hijo del mapa (país), lo guardamos en countriesData
        foreach (Transform child in mapObject.transform)
        {
            if (child.TryGetComponent<Country>(out Country country))
            {
                CountrySaveData countrySaveData = new CountrySaveData {
                    id = country.Id,
                    name = country.CountryName,
                };

                var renderer = child.GetComponentInChildren<SpriteRenderer>();
                if (renderer != null)
                    countrySaveData.color = '#' + ColorUtility.ToHtmlStringRGB(renderer.color);
                
                foreach (Country neighbor in country.NeighboringCountries)
                    countrySaveData.neighbors.Add(neighbor.Id);

                countriesData.countries.Add(countrySaveData);
            }
        }

        // Pasamos countriesData a JSON
        string json = JsonUtility.ToJson(countriesData, true);

        // Actualizamos los assets
        UpdateAsset(json);
    }


    private void UpdateAsset(string json)
    {
        string folderPath = "Assets/Map";
        string filePath = folderPath + "/countries.json";

        // Check if the folder exists, if not, create it
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Map");
        }

        // Check if the file already exists
        if (AssetDatabase.LoadAssetAtPath<TextAsset>(filePath) == null)
        {
            System.IO.File.WriteAllText(Application.dataPath + "/Map/countries.json", json); // Create an empty JSON file
            AssetDatabase.Refresh(); // Refresh the asset database to reflect changes
            AssetDatabase.SaveAssets(); // Save the assets to disk

            Debug.Log("Created new countries.json");
        } else
        {
            // If the file already exists, update it
            System.IO.File.WriteAllText(Application.dataPath + "/Map/countries.json", json); // Overwrite existing file with new data
            AssetDatabase.Refresh(); // Refresh the asset database to reflect changes
            AssetDatabase.SaveAssets(); // Save the assets to disk

            Debug.Log("Updated countries.json");
        }
    }


    // TODO:
    // Carga el mapa desde una textura, generando sprites
    // y los datos desde un .json.
    private void LoadMap()
    {
        // Cargar json y textura
        TextAsset countriesJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Map/countries.json");
        Texture2D mapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Map/mapTexture.png");

        if (countriesJson == null)
        {
            Debug.LogError("countries.json was not found. Make sure it's called countries.json and it's placed inside Map/");
            // return;
        }

        if (mapTexture == null)
        {
            Debug.LogError("Map texture was not found. Make sure it's called mapTexture.png and it's placed inside Map/");
            return;
        }


        // Procear json
        CountriesSaveData countriesSaveData = JsonUtility.FromJson<CountriesSaveData>(countriesJson.text);

        if (countriesSaveData == null)
        {
            Debug.LogError("Could not parse countries.json");
            return;
        }

        var colorsToCountries = new Dictionary<Color, CountryData>();

        foreach (CountrySaveData countrySaveData in countriesSaveData.countries)
        {
            if (ColorUtility.TryParseHtmlString(countrySaveData.color, out Color color))
            {
                if (!colorsToCountries.TryAdd(color, new CountryData
                {
                    id = countrySaveData.id,
                    name = countrySaveData.name,
                    neighbors = countrySaveData.neighbors,
                }))
                {
                    string name = colorsToCountries[color].name;
                    Debug.LogError($"Countries {name} and {countrySaveData.name} have the same color assigned in the json file!");
                }
            }
            else
                Debug.LogError("Could not parse country '" + countrySaveData.id + "' color");
        }


        // Procesar textura
        // Código asqueroso y lento, pero funciona
        Color32[] mapPixels = mapTexture.GetPixels32();
        int width = mapTexture.width;
        int height = mapTexture.height;

        string folderPath = "Assets/Map/CountrySprites/";
        bool alphaErrorShowned = false;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 color = mapPixels[y * width + x];
                switch (color.a)
                {
                    case 255:
                        Texture2D countryTexture = CreateCountryTexture(mapPixels, x, y, width, height, out Vector2 pos, out Vector2 avgPos);
                        CountryData countryData;
                        string fileName;

                        if (colorsToCountries.TryGetValue((Color)color, out countryData))
                        {
                            fileName = countryData.id;
                        } else 
                        {
                            Debug.LogError($"No country was found in the json file for the country texture with color {ColorUtility.ToHtmlStringRGB(color)}");
                            countryData = new CountryData();
                            colorsToCountries[(Color)color] = countryData;
                            fileName = ColorUtility.ToHtmlStringRGB(color);
                        }
                        countryData.position = pos;
                        countryData.indicatorPos = avgPos;

                        // Guardar la imagen del país
                        string path = folderPath + fileName + ".png";
                        System.IO.File.WriteAllBytes(path, countryTexture.EncodeToPNG());
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                        // Configurar la importación de la imagen como sprite
                        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                        TextureImporterSettings settings = new TextureImporterSettings();
                        importer.ReadTextureSettings(settings);

                        settings.spriteMode = (int)SpriteImportMode.Single;
                        settings.textureType = TextureImporterType.Sprite;
                        settings.spritePixelsPerUnit = 1;
                        settings.spriteAlignment = (int)SpriteAlignment.BottomLeft;
                        settings.filterMode = UnityEngine.FilterMode.Point;

                        importer.SetTextureSettings(settings);

                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();

                        // Cargar el sprite asset
                        countryData.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        break;

                    case 0:
                        continue;

                    default:
                        if (!alphaErrorShowned)
                        {
                            Debug.LogError($"Some pixels ({x}, {y}) have alpha values that are not 0 or 1");
                            alphaErrorShowned = true;
                        }
                        return;
                }
            }
        }


        // Crear mapa con países
        GameObject map = new GameObject("Map");
        List<Country> countries = new List<Country>();
        var countriesToCountryData = new Dictionary<Country, CountryData>();

        foreach (KeyValuePair<Color, CountryData> pair in colorsToCountries)
        {
            Color color = pair.Key;
            CountryData countryData = pair.Value;
            
            GameObject country = PrefabUtility.InstantiatePrefab(countryPrefab, map.transform) as GameObject;
            country.name = countryData.name;
            country.transform.position = countryData.position;

            if (country.TryGetComponent<Country>(out Country countryScript))
            {
                countryScript.Init(countryData.id, countryData.name);
                countriesToCountryData.Add(countryScript, countryData);
                countries.Add(countryScript);
            }
            else
                Debug.LogError("Country prefab has no country script");

            SpriteRenderer spriteRenderer = country.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (countryData.sprite != null)
                {
                    spriteRenderer.color = color;
                    spriteRenderer.sprite = countryData.sprite;
                } else
                {
                    Debug.LogError($"Country {countryData.id} has no texture");
                }
            } else
                Debug.LogError("Country prefab has no sprite renderers");

            Canvas troopIndicator = country.GetComponentInChildren<Canvas>();
            if (troopIndicator != null)
                troopIndicator.transform.localPosition = countryData.indicatorPos;
            else
                Debug.LogError("Country prfab has no troop number indicator UI");
        }

        // Cargar conecciones entre países
        foreach (KeyValuePair<Country, CountryData> pair in countriesToCountryData)
        {
            Country country = pair.Key;
            List<string> neighborIds = pair.Value.neighbors;

            List<Country> neighbors = countries.Where(c => neighborIds.Contains(c.Id)).ToList();
            country.SetNeighbors(neighbors);

            if (neighbors.Count == 0)
                Debug.LogWarning($"Country {country.CountryName} has no neighbors");
            else if (neighbors.Any(neighbor => !countriesToCountryData[neighbor].neighbors.Contains(country.Id)))
                Debug.LogWarning($"Country {country.CountryName} has one way connections to other countries");
        }

        // Mark the scene as dirty so the changes are saved
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private Texture2D CreateCountryTexture(Color32[] mapPixels, int pixelX, int pixelY, int width, int height, out Vector2 pos, out Vector2 avgPos)
    {
        Color32 color = mapPixels[pixelY * width + pixelX];
        int maxY = pixelY;
        int minY = pixelY;
        int maxX = pixelX;
        int minX = pixelX;

        // Calcular tamaño del país
        for (int y = pixelY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mapPixels[y * width + x].Equals(color))
                {
                    maxY = Mathf.Max(maxY, y);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    minX = Mathf.Min(minX, x);
                }
            }
        }

        int newWidth = maxX - minX + 1;
        int newHeight = maxY - minY + 1;
        pos = new Vector2(minX, pixelY);
        avgPos = Vector2.zero;
        int pixelCount = 0;

        // Crear textura 
        Texture2D countryTexture = new Texture2D(newWidth, newHeight);
        Color32[] countryPixels = countryTexture.GetPixels32();

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                Color32 pixelColor = mapPixels[(minY + y) * width + (minX + x)];
                if (pixelColor.Equals(color))
                {
                    mapPixels[(minY + y) * width + (minX + x)] = new Color32(0, 0, 0, 0);
                    countryPixels[y * newWidth + x] = Color.white;
                    avgPos += new Vector2(x, y);
                    pixelCount++;
                }
                else
                    countryPixels[y * newWidth + x] = new Color32(0, 0, 0, 0);
            }
        }
        avgPos /= pixelCount;

        countryTexture.SetPixels32(countryPixels);
        countryTexture.Apply();
        
        return countryTexture;
    }
}
