using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleManager : MonoBehaviour {

    public bool m_DepthViewMode = true;
    public bool m_LowDCDepthTextureMode = true;

    bool m_LowDCDepthTextureModeEnabling;
    bool m_DepthViewEnabling;

    CameraEffectManager m_Effect;
    void Awake()
    {
        m_Effect = Camera.main.GetComponent<CameraEffectManager>();
    }


    void Start()
    {
        m_LowDCDepthTextureModeEnabling = m_LowDCDepthTextureMode;
        SetDepthTextureMode();

        m_DepthViewEnabling = m_DepthViewMode;
        SetViewDepth();
    }

    void Update()
    {
        if (m_LowDCDepthTextureModeEnabling != m_LowDCDepthTextureMode)
        {
            m_LowDCDepthTextureModeEnabling = m_LowDCDepthTextureMode;
            SetDepthTextureMode();
        }

        if(m_DepthViewEnabling!=m_DepthViewMode)
        {
            m_DepthViewEnabling = m_DepthViewMode;
            SetViewDepth();
        }
    }

    void SetDepthTextureMode()
    {
        m_Effect.RemoveCameraEffect<CE_MainCameraTexture>();
        m_Effect.m_Camera.depthTextureMode = DepthTextureMode.None;
        if (m_LowDCDepthTextureModeEnabling)
            m_Effect.GetOrAddCameraEffect<CE_MainCameraTexture>().SetTextureEnable(true, true);
        else
            m_Effect.m_Camera.depthTextureMode = DepthTextureMode.Depth;
    }

    void SetViewDepth()
    {
        m_Effect.RemoveCameraEffect<PE_ViewDepth>();
        if(m_DepthViewMode)
            m_Effect.GetOrAddCameraEffect<PE_ViewDepth>();
    }
}
