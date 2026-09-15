Shader "Custom/FragmentAssemblyWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MaskTex ("Stage Mask", 2D) = "black" {}

        _StageAlpha ("Stage Start Alpha", Range(0,1)) = 1
        _EraseStrength ("Erase Strength", Range(0,1)) = 0.5

        // xy：Sprite 本地 bounds 最小点；zw：bounds 尺寸。
        _LocalBounds ("Local Bounds", Vector) = (0,0,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 maskUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;

            fixed4 _Color;
            float _StageAlpha;
            float _EraseStrength;
            float4 _LocalBounds;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.uv = v.uv;
                o.maskUV =
                    (v.vertex.xy - _LocalBounds.xy) /
                    _LocalBounds.zw;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv) * i.color;
                float mask = tex2D(_MaskTex, i.maskUV).r;

                color.a *= _StageAlpha *
                    (1.0 - saturate(mask) * _EraseStrength);

                return color;
            }

            ENDCG
        }
    }
}