using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class ResourceLoader : MonoBehaviour
{ 
    [SerializeField] private Button loadModelButton;
    [SerializeField] private Button loadImageButton;
    [SerializeField] private Button loadAudioButton;
    [SerializeField] private Button unloadButton;
    [SerializeField] private Transform modelSpawnPoint;
    [SerializeField] private SpriteRenderer displayImage;
    [SerializeField] private AudioSource audioSource;

    private Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();
    private GameObject instantiatedModel;

    void Start()
    {
        loadModelButton.onClick.AddListener(() => LoadResource<GameObject>("Model"));
        loadImageButton.onClick.AddListener(() => LoadResource<Sprite>("Image"));
        loadAudioButton.onClick.AddListener(() => LoadResource<AudioClip>("Audio"));
        unloadButton.onClick.AddListener(UnloadAllResources);
    }

    void LoadResource<T>(string key) where T : Object
    {
        if (loadedAssets.ContainsKey(key))
        {
            Debug.Log($"Resource {key} is already loaded.");
            return;
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                loadedAssets[key] = op;
                ApplyResource(key, op.Result);
            }
            else
            {
                Debug.LogError($"Failed to load {key}");
            }
        };
    }

    void ApplyResource(string key, Object resource)
    {
        if (resource is GameObject modelPrefab)
        {
            if (instantiatedModel != null)
            {
                Destroy(instantiatedModel);
            }
            instantiatedModel = Instantiate(modelPrefab, modelSpawnPoint.position, Quaternion.identity);
        }
        else if (resource is Sprite sprite)
        {
            displayImage.sprite = sprite;
        }
        else if (resource is AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void UnloadAllResources()
    {
        foreach (var entry in loadedAssets)
        {
            Addressables.Release(entry.Value);
        }
        loadedAssets.Clear();

        if (instantiatedModel != null)
        {
            Destroy(instantiatedModel);
            instantiatedModel = null;
        }
        displayImage.sprite = null;
        audioSource.Stop();
        audioSource.clip = null;
    }
}
