﻿using System;
using System.Diagnostics;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    // Keep this class first in the file. Otherwise it seems that the script type is not registered properly.
    public abstract class AtmosphericScattering : VolumeComponent
    {
        static readonly int m_TypeParam = Shader.PropertyToID("_AtmosphericScatteringType");
        // Fog Color
        static readonly int m_ColorModeParam = Shader.PropertyToID("_FogColorMode");
        static readonly int m_FogColorDensityParam = Shader.PropertyToID("_FogColorDensity");
        static readonly int m_MipFogParam = Shader.PropertyToID("_MipFogParameters");

        // Fog Color
        public FogColorParameter       colorMode = new FogColorParameter(FogColorMode.SkyColor);
        [Tooltip("Constant Fog Color")]
        public ColorParameter          color = new ColorParameter(Color.grey);
        public ClampedFloatParameter   density = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);
        [Tooltip("Maximum mip map used for mip fog (0 being lowest and 1 highest mip).")]
        public ClampedFloatParameter   mipFogMaxMip = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);
        [Tooltip("Distance at which minimum mip of blurred sky texture is used as fog color.")]
        public MinFloatParameter       mipFogNear = new MinFloatParameter(0.0f, 0.0f);
        [Tooltip("Distance at which maximum mip of blurred sky texture is used as fog color.")]
        public MinFloatParameter       mipFogFar = new MinFloatParameter(1000.0f, 0.0f);

        public abstract void PushShaderParameters(CommandBuffer cmd, FrameSettings frameSettings);

        public static void PushNeutralShaderParameters(CommandBuffer cmd)
        {
            cmd.SetGlobalFloat(m_TypeParam, (float)FogType.None);
        }

        public void PushShaderParametersCommon(CommandBuffer cmd, FogType type, FrameSettings frameSettings)
        {
            if (frameSettings.enableAtmosphericScattering)
                cmd.SetGlobalFloat(m_TypeParam, (float)type);
            else
                cmd.SetGlobalFloat(m_TypeParam, (float)FogType.None);

            // Fog Color
            cmd.SetGlobalFloat(m_ColorModeParam, (float)colorMode.value);
            cmd.SetGlobalColor(m_FogColorDensityParam, new Color(color.value.r, color.value.g, color.value.b, density));
            cmd.SetGlobalVector(m_MipFogParam, new Vector4(mipFogNear, mipFogFar, mipFogMaxMip, 0.0f));
        }
    }

    [GenerateHLSL]
    public enum FogType
    {
        None,
        Linear,
        Exponential
    }

    [GenerateHLSL]
    public enum FogColorMode
    {
        ConstantColor,
        SkyColor,
    }

    [Serializable, DebuggerDisplay(k_DebuggerDisplay)]
    public sealed class FogTypeParameter : VolumeParameter<FogType>
    {
        public FogTypeParameter(FogType value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    [Serializable, DebuggerDisplay(k_DebuggerDisplay)]
    public sealed class FogColorParameter : VolumeParameter<FogColorMode>
    {
        public FogColorParameter(FogColorMode value, bool overrideState = false)
            : base(value, overrideState) { }
    }
}
