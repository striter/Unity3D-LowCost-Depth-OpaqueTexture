using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class CE_MainCameraTexture : CameraEffectBase        //Depth Texture Replacement
{
    public override enum_CameraEffectQueue m_Sorting => enum_CameraEffectQueue.Main;
    public bool m_DepthTextureEnabled { get; private set; } = false;
    public bool m_OpaqueTextureEnabled { get; private set; } = false;
    readonly int ID_GlobalDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
    readonly int ID_GlobalOpaqueTexture = Shader.PropertyToID("_CameraOpaqueTexture");
    CommandBuffer m_DepthTextureBuffer, m_OpaqueTextureBuffer;
    RenderTexture m_ColorBuffer, m_DepthBuffer, m_DepthTexture, m_OpaqueTexture;
    public override bool m_DoGraphicBlitz => true;
    public override void InitEffect(CameraEffectManager _manager)
    {
        base.InitEffect(_manager);
        m_ColorBuffer = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 0, RenderTextureFormat.RGB111110Float);
        m_ColorBuffer.name = "Main Color Buffer";
        m_DepthBuffer = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 24, RenderTextureFormat.Depth);
        m_DepthBuffer.name = "Main Depth Buffer";
        m_Manager.m_Camera.SetTargetBuffers(m_ColorBuffer.colorBuffer, m_DepthBuffer.depthBuffer);

        m_DepthTextureBuffer = new CommandBuffer() { name = "Depth Texture Copy" };
        m_OpaqueTextureBuffer = new CommandBuffer() { name = "Opaque Texture Copy" };
        m_Manager.m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthTextureBuffer);
        m_Manager.m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_OpaqueTextureBuffer);
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(m_ColorBuffer, destination);
    }
    public CE_MainCameraTexture SetTextureEnable(bool depthTexture, bool opaqueTexture)
    {
        m_DepthTextureEnabled = depthTexture;
        m_DepthTextureBuffer.Clear();
        RenderTexture.ReleaseTemporary(m_DepthTexture);
        if (m_DepthTextureEnabled)
        {
            m_DepthTexture = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 0, RenderTextureFormat.RFloat);
            m_DepthTexture.name = "Opaque Depth Texture";

            m_DepthTextureBuffer.Blit(m_DepthBuffer.depthBuffer, m_DepthTexture.colorBuffer);
            m_DepthTextureBuffer.SetGlobalTexture(ID_GlobalDepthTexture, m_DepthTexture);
        }

        m_OpaqueTextureEnabled = opaqueTexture;
        m_OpaqueTextureBuffer.Clear();
        RenderTexture.ReleaseTemporary(m_OpaqueTexture);
        if (m_OpaqueTextureEnabled)
        {
            m_OpaqueTexture = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
            m_OpaqueTexture.name = "Opaque Texture";

            m_OpaqueTextureBuffer.Blit(m_ColorBuffer, m_OpaqueTexture);
            m_OpaqueTextureBuffer.SetGlobalTexture(ID_GlobalOpaqueTexture, m_OpaqueTexture);
        }
        return this;
    }


    public override void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(m_DepthTexture);
        RenderTexture.ReleaseTemporary(m_OpaqueTexture);
        RenderTexture.ReleaseTemporary(m_ColorBuffer);
        RenderTexture.ReleaseTemporary(m_DepthBuffer);
        m_Manager.m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthTextureBuffer);
        m_Manager.m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_OpaqueTextureBuffer);
        m_Manager.m_Camera.targetTexture = null;
        base.OnDestroy();
    }
}
public class PE_ViewDepth : PostEffectBase
{
}