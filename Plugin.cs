using BepInEx;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KnifeHoldable
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }
        static AssetBundle assetBundle = null;
        void OnGameInitialized()
        {
            UnityEngine.Debug.Log("[KnifeHoldable] OnGameInitialized started.");
            if (assetBundle == null)
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KnifeHoldable.Resources.TKnife");
                assetBundle = AssetBundle.LoadFromStream(stream);
                stream.Close();
            }
            if (assetBundle == null) return;
            GameObject[] assets = assetBundle.LoadAllAssets<GameObject>();
            if (assets.Length > 0)
            {
                GameObject knife = Instantiate<GameObject>(assets[0]);
                knife.transform.SetParent(GorillaTagger.Instance.offlineVRRig.rightHandTransform, false);
                knife.transform.localPosition = Vector3.zero;
                knife.transform.localRotation = Quaternion.identity;
                knife.transform.localScale = Vector3.one;
                knife.layer = 0;
                foreach (Transform child in knife.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = 0;
                }
                knife.SetActive(true);
                foreach (Renderer r in knife.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = true;
                }
                Bounds bounds = new Bounds(knife.transform.position, Vector3.zero);
                bool hasBounds = false;
                foreach (Renderer r in knife.GetComponentsInChildren<Renderer>(true))
                {
                    if (hasBounds) bounds.Encapsulate(r.bounds);
                    else { bounds = r.bounds; hasBounds = true; }
                }
                if (hasBounds)
                {
                    float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                    if (maxDim > 0)
                    {
                        float scaleFactor = 0.45f / maxDim;
                        knife.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    }
                    hasBounds = false;
                    foreach (Renderer r in knife.GetComponentsInChildren<Renderer>(true))
                    {
                        if (hasBounds) bounds.Encapsulate(r.bounds);
                        else { bounds = r.bounds; hasBounds = true; }
                    }
                    Transform targetTransform = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform;
                    knife.transform.SetParent(targetTransform, false);
                    knife.transform.localPosition = Vector3.zero;
                    knife.transform.localRotation = Quaternion.Euler(0f, 270f, -450f);
                    knife.transform.localPosition += new Vector3(0.050f, 0.080f, 0.040f);
                    Shader opaqueShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (opaqueShader == null) opaqueShader = Shader.Find("Standard");
                    Material grayMat = new Material(opaqueShader);
                    grayMat.color = new Color(0.4f, 0.4f, 0.4f); 
                    if (grayMat.HasProperty("_BaseColor")) grayMat.SetColor("_BaseColor", new Color(0.4f, 0.4f, 0.4f));
                    if (grayMat.HasProperty("_Metallic")) grayMat.SetFloat("_Metallic", 0.8f);
                    if (grayMat.HasProperty("_Glossiness")) grayMat.SetFloat("_Glossiness", 0.6f);
                    if (grayMat.HasProperty("_Smoothness")) grayMat.SetFloat("_Smoothness", 0.6f);
                    foreach (Renderer r in knife.GetComponentsInChildren<Renderer>(true))
                    {
                        if (r.material != null && r.material.mainTexture != null)
                        {
                            Texture tex = r.material.mainTexture;
                            r.material = new Material(grayMat);
                            r.material.mainTexture = tex;
                            if (r.material.HasProperty("_BaseMap")) r.material.SetTexture("_BaseMap", tex);
                            r.material.color = Color.white;
                            if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", Color.white);
                        }
                        else
                        {
                            r.material = grayMat;
                        }
                    }
                }
            }
        }
    }
}
