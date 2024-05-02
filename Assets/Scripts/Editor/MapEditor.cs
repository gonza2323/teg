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
using System.IO;


public class MapEditor : EditorWindow
{
    // Referencias necesarias
    GameObject mapObject;
    string countriesDataSavePath = "Assets/Map/countries.json";
    string mapTextureSavePath = "Assets/Map/mapTexture.png";
    
    GameObject countryPrefab;
    TextAsset countriesDataJson;
    Texture2D countriesTexture;

    private Color32 NO_TEXTURE_COLOR = new Color32(255, 0, 255, 255);


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
        public string color;
        public List<string> neighbors= new List<string>();
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


    // Definir la interfaz
    public void OnGUI()
    {
        GUILayout.Label("Map generator", EditorStyles.boldLabel);

        GUILayout.Label("Saving Map", EditorStyles.boldLabel);
        // Mapa que se va a guardar
        mapObject = EditorGUILayout.ObjectField("Map Object", mapObject, typeof(GameObject), true) as GameObject;
        // Dirección donde guardar los datos de los países
        countriesDataSavePath = EditorGUILayout.TextField("Data save path", countriesDataSavePath);
        // Dirección donde guardar textura del mapa
        mapTextureSavePath = EditorGUILayout.TextField("Texture save path", mapTextureSavePath);

        // Botón para guardar mapa
        if (GUILayout.Button("Save Map"))
        {
            if (mapObject != null)
                SaveMap();
            else
                Debug.LogError("Map object not set. Did not save.");
        }

        GUILayout.Label("Loading map", EditorStyles.boldLabel);
        // Prefab para construir países al cargar mapa
        countryPrefab = EditorGUILayout.ObjectField("Country Prefab", countryPrefab, typeof(GameObject), false) as GameObject;
        // Dirección de dónde cargar los datos de los países
        countriesDataJson = EditorGUILayout.ObjectField("Countries JSON", countriesDataJson, typeof(TextAsset), false) as TextAsset;
        // Textura que contiene a los países
        countriesTexture = EditorGUILayout.ObjectField("Countries Texture", countriesTexture, typeof(Texture2D), false) as Texture2D;

        // Botón para cargar mapa
        if (GUILayout.Button("Load Map"))
        {
            if (countriesDataJson == null)
            {
                Debug.LogError("Countries json file not set. Did not load.");
                return;
            }
            if (countriesTexture == null)
            {
                Debug.LogError("Map texture not set. Did not load.");
                return;
            }
            if (countryPrefab == null)
            {
                Debug.LogError("Country prefab not set. Did not load.");
                return;
            }
            LoadMap();
        }
    }


    // Guarda el mapa actual a un .json
    // y sus sprites a una textura
    private void SaveMap()
    {
        // Estructura para guardar los datos
        CountriesSaveData countriesData = new CountriesSaveData();

        // Obtenemos los países del mapa
        Country[] countries = mapObject.GetComponentsInChildren<Country>();

        if (countries.Length == 0)
        {
            Debug.LogError("Map has no countries. Did not save.");
            return;
        }

        // Procesamos cada país
        foreach (Country country in countries)
        {
            CountrySaveData countrySaveData = new CountrySaveData {
                id = country.Id,
                name = country.CountryName,
            };

            // Si el país tiene sprite, guardamos el color
            SpriteRenderer renderer = country.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                countrySaveData.color = '#' + ColorUtility.ToHtmlStringRGB(renderer.color);
                // TODO: Actualizar textura
            }
            else
            {
                Debug.LogWarning($"Country ({country.Id}) \"{country.CountryName}\" has not sprite renderer or no sprite. It was saved without a texture.");
                countrySaveData.color = '#' + ColorUtility.ToHtmlStringRGB(NO_TEXTURE_COLOR);
            }
            
            // Guardamos sus vecinos
            foreach (Country neighbor in country.NeighboringCountries)
            {
                if (neighbor == country)
                {
                    Debug.LogWarning($"Country ({country.Id}) \"{country.CountryName}\" is connected to itself. Connection was not saved.");
                    continue;
                }

                if (!neighbor.NeighboringCountries.Contains(country))
                    Debug.LogWarning($"Country ({country.Id}) \"{country.CountryName}\" has a one-way connection to ({neighbor.Id}) \"{neighbor.CountryName}\".");

                countrySaveData.neighbors.Add(neighbor.Id);
            }

            if (countrySaveData.neighbors.Count == 0)
                Debug.LogWarning($"Country ({country.Id}) \"{country.CountryName}\" has no connections to other countries.");
            
            // Añadimos la data del país a la lista que será guardada
            countriesData.countries.Add(countrySaveData);
        }

        // Guardamos los datos de los países
        SaveCountriesData(countriesData);
    }


    private void SaveCountriesData(CountriesSaveData countriesData)
    {
        string directoryPath = Path.GetDirectoryName(countriesDataSavePath);
        string countriesDataJson = JsonUtility.ToJson(countriesData, true);

        // Crear directorio de guardado si no existe.
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        // Guardar json
        System.IO.File.WriteAllText(countriesDataSavePath, countriesDataJson);
        Debug.Log("Saved countries data at: " + countriesDataSavePath);
        AssetDatabase.Refresh();
    }


    // Carga el mapa desde una textura, generando sprites para cada país,
    // y un json con la información de cada país.
    private void LoadMap()
    {
        // Intentar parsear json
        CountriesSaveData countriesSaveData = JsonUtility.FromJson<CountriesSaveData>(countriesDataJson.text);
        if (countriesSaveData == null)
        {
            Debug.LogError("Could not parse countries json file. Did not load.");
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
        Color32[] mapPixels = countriesTexture.GetPixels32();
        int width = countriesTexture.width;
        int height = countriesTexture.height;

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
