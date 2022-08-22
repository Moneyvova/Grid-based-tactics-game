Shader "Unlit/UnlitCellShader"
{
    Properties
    { 
        [PerRendererData] _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}

        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.5
        _OutlineWidth ("Outline Width", Range (0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "LightMode" = "ForwardBase"
            "PassFlags" = "OnlyDirectional"
        }
        LOD 100

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
            };

            half4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ShadowStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float NdotL = dot(_WorldSpaceLightPos0, normal);
                float lightIntensity = NdotL > 0 ? 1 : _ShadowStrength;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                col *= lightIntensity*_Color;
                return col;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        Pass
        {
                // Hide polygons that facing the camera
                Cull Off

                Stencil
                {
                    Ref 1
                    Comp Greater
                }

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                };

                // Declare variables
                half _OutlineWidth;
                static const half4 OUTLINE_COLOR = half4(0,0,0,0);

                v2f vert(appdata v)
                {
                    // Convert vertex position and normal to the clip space
                    float4 clipPosition = UnityObjectToClipPos(v.vertex);
                    float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, v.normal));

                    // Calculating vertex offset.
                    // Taking into account "perspective division" and multiplying it with W component
                    // to keep constant offset
                    // independent from distance to the camera
                    float2 offset = normalize(clipNormal.xy) * _OutlineWidth * clipPosition.w;

                    // We also need take into account aspect ratio.
                    // _ScreenParams - built-in Unity variable
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    offset.y *= aspect;

                    // Applying offset
                    clipPosition.xy += offset;

                    v2f o;
                    o.vertex = clipPosition;

                    return o;
                }

                fixed4 frag() : SV_Target
                {
                    // All pixels of the outline have the same constant color
                    return OUTLINE_COLOR;
                }
                ENDCG
            }
    }
}
