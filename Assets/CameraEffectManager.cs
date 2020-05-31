using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraEffectManager :MonoBehaviour
{
    List<CameraEffectBase> m_CameraEffects=new List<CameraEffectBase>();
    public Camera m_Camera { get; protected set; }
    public bool m_DoGraphicBlitz { get; private set; } = false;
    RenderTexture m_BlitzTempTexture1, m_BlitzTempTexture2;

    protected void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.depthTextureMode = DepthTextureMode.None;
        m_DoGraphicBlitz = false;
        m_BlitzTempTexture1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        m_BlitzTempTexture2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
    }
    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(!m_DoGraphicBlitz)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, m_BlitzTempTexture1);
        for (int i = 0; i < m_CameraEffects.Count; i++)
        {
            if (! m_CameraEffects[i].m_Enabled)
                continue;

            m_CameraEffects[i].OnRenderImage(m_BlitzTempTexture1,m_BlitzTempTexture2);
            Graphics.Blit(m_BlitzTempTexture2, m_BlitzTempTexture1);
        }
        Graphics.Blit(m_BlitzTempTexture1,destination);
    }
    private void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(m_BlitzTempTexture2);
        RenderTexture.ReleaseTemporary(m_BlitzTempTexture1);
        RemoveAllPostEffect();
    }

    #region Interact
    public T GetOrAddCameraEffect<T>() where T : CameraEffectBase, new()
    {
        T existingEffect = GetCameraEffect<T>();
        if (existingEffect != null)
            return existingEffect;

        T effectBase = new T();
        if (effectBase.m_Supported)
        {
            effectBase.InitEffect(this);
            m_CameraEffects.Add(effectBase);
            ResetCameraEffectParams();
            return effectBase;
        }
        return null;
    }

    public T GetCameraEffect<T>() where T : CameraEffectBase => m_CameraEffects.Find(p => p.GetType() == typeof(T)) as T;
    public void RemoveCameraEffect<T>() where T : CameraEffectBase, new()
    {
        T effect = GetCameraEffect<T>();
        if (effect == null)
            return;

        effect.OnDestroy();
        m_CameraEffects.Remove(effect);
        ResetCameraEffectParams();
    }
    public void RemoveAllPostEffect()
    {
        foreach(CameraEffectBase effect in m_CameraEffects)
        {
            effect.OnDestroy();
        }
        m_CameraEffects.Clear();
        ResetCameraEffectParams();
    }
    protected void ResetCameraEffectParams()
    {
        m_DoGraphicBlitz = false;
        m_CameraEffects.Sort((a, b) => a.m_Sorting - b.m_Sorting);
        foreach (CameraEffectBase effect in m_CameraEffects)
        {
            if (!effect.m_Enabled)
                return;

            m_DoGraphicBlitz |= effect.m_DoGraphicBlitz;
        }
    }
    #endregion
}

#region CameraEffectBase
public enum enum_CameraEffectQueue
{
    Invalid = -1,
    Main = 1,
    PostEffect = 2,
}
public class CameraEffectBase
{
    public virtual enum_CameraEffectQueue m_Sorting => enum_CameraEffectQueue.Invalid;
    public virtual bool m_DoGraphicBlitz => false;
    protected CameraEffectManager m_Manager { get; private set; }
    public bool m_Supported { get; private set; }
    public bool m_Enabled { get; protected set; }
    public CameraEffectBase()
    {
        m_Supported = Init();
    }
    protected virtual bool Init()
    {
        return true;
    }
    public virtual void InitEffect(CameraEffectManager _manager)
    {
        m_Manager = _manager;
        m_Enabled = true;
    }
    public virtual void SetEnable(bool enable) => m_Enabled = enable;
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
    }

    public virtual void OnDestroy()
    {
    }
}
public class PostEffectBase : CameraEffectBase
{
    public override enum_CameraEffectQueue m_Sorting => enum_CameraEffectQueue.PostEffect;
    const string S_ParentPath = "Hidden/PostEffect/";
    public Material m_Material { get; private set; }
    public override bool m_DoGraphicBlitz => true;
    protected override bool Init()
    {
        m_Material = CreateMaterial(this.GetType());
        return m_Material != null;
    }

    public static Material CreateMaterial(Type type)
    {
        try
        {
            Shader shader = Shader.Find(S_ParentPath + type.ToString());
            if (shader == null)
                throw new Exception("Shader:" + S_ParentPath + type.ToString() + " Not Found");
            if (!shader.isSupported)
                throw new Exception("Shader:" + S_ParentPath + type.ToString() + " Is Not Supported");

            return new Material(shader) { hideFlags = HideFlags.DontSave };
        }
        catch (Exception e)
        {
            Debug.LogError("Post Effect Error:" + e.Message);
            return null;
        }
    }

    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, m_Material);
    }
    public override void OnDestroy()
    {
        GameObject.Destroy(m_Material);
    }
}
#endregion