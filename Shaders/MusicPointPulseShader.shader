Shader "Custom/MusicPointPulseShader"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _StencilTex ("_StencilTex", 2D) = "white" {}

    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _StencilTex;
            float _alpha;
            fixed4 _Color;
            float _spectrum1;
            float _spectrum2;
            float _spectrum3;
            float _spectrum4;
            float _spectrum5;
            float _spectrum6;
            float _spectrum7;
            float _spectrum8;
            float _spectrum9;
            float _spectrum10;
            float _spectrum11;
            float _spectrum12;
            float _spectrum13;
            float _spectrum14;
            float _spectrum15;
            float _spectrum16;
            float _spectrum17;
            float _spectrum18;
            float _spectrum19;
            float _spectrum20;
            float _spectrum21;
            float _spectrum22;
            float _spectrum23;
            float _spectrum24;
            float _spectrum25;
            float _spectrum26;
            float _spectrum27;
            float _spectrum28;
            float _spectrum29;
            float _spectrum30;
            float _spectrum31;
            float _spectrum32;
            float _spectrum33;
            float _spectrum34;
            float _spectrum35;
            float _spectrum36;
            float _spectrum37;
            float _spectrum38;
            float _spectrum39;
            float _spectrum40;
            float _spectrum41;
            float _spectrum42;
            float _spectrum43;
            float _spectrum44;
            float _spectrum45;
            float _spectrum46;
            float _spectrum47;
            float _spectrum48;
            float _spectrum49;
            float _spectrum50;


            float2 GetStencilUV( float2 uv ){

                float2 stencilUV = float2(
                    1-uv.y,
                    1-uv.x
                );

                float camTexWidth = 1920;
                float camTexHeight = 1440;
                float aspect = (camTexWidth/camTexHeight) / (_ScreenParams.y/_ScreenParams.x);
                stencilUV.y = stencilUV.y * aspect + (1-aspect)/2;

                
                if(stencilUV.x <= 0.02)
                {
                    stencilUV.y += _spectrum1;
                }
                else if(stencilUV.x <= 0.04)
                {
                    stencilUV.y += _spectrum2;
                }
                else if(stencilUV.x <= 0.06)
                {
                    stencilUV.y += _spectrum3;
                }
                else if(stencilUV.x <= 0.08)
                {
                    stencilUV.y += _spectrum4;
                }
                else if(stencilUV.x <= 0.1)
                {
                    stencilUV.y += _spectrum5;
                }
                else if(stencilUV.x <= 0.12)
                {
                    stencilUV.y += _spectrum6;
                }
                else if(stencilUV.x <= 0.14)
                {
                    stencilUV.y += _spectrum7;
                }
                else if(stencilUV.x <= 0.16)
                {
                    stencilUV.y += _spectrum8;
                }
                else if(stencilUV.x <= 0.18)
                {
                    stencilUV.y += _spectrum9;
                }
                else if(stencilUV.x <= 0.2)
                {
                    stencilUV.y += _spectrum10;
                }
                else if(stencilUV.x <= 0.22)
                {
                    stencilUV.y += _spectrum11;
                }
                else if(stencilUV.x <= 0.24)
                {
                    stencilUV.y += _spectrum12;
                }
                else if(stencilUV.x <= 0.26)
                {
                    stencilUV.y += _spectrum13;
                }
                else if(stencilUV.x <= 0.28)
                {
                    stencilUV.y += _spectrum14;
                }
                else if(stencilUV.x <= 0.3)
                {
                    stencilUV.y += _spectrum15;
                }
                else if(stencilUV.x <= 0.32)
                {
                    stencilUV.y += _spectrum16;
                }
                else if(stencilUV.x <= 0.34)
                {
                    stencilUV.y += _spectrum17;
                }
                else if(stencilUV.x <= 0.36)
                {
                    stencilUV.y += _spectrum18;
                }
                else if(stencilUV.x <= 0.38)
                {
                    stencilUV.y += _spectrum19;
                }
                else if(stencilUV.x <= 0.4)
                {
                    stencilUV.y += _spectrum20;
                }
                else if(stencilUV.x <= 0.42)
                {
                    stencilUV.y += _spectrum21;
                }
                else if(stencilUV.x <= 0.44)
                {
                    stencilUV.y += _spectrum22;
                }
                else if(stencilUV.x <= 0.46)
                {
                    stencilUV.y += _spectrum23;
                }
                else if(stencilUV.x <= 0.48)
                {
                    stencilUV.y += _spectrum24;
                }
                else if(stencilUV.x <= 0.5)
                {
                    stencilUV.y += _spectrum25;
                }
                else if(stencilUV.x <= 0.52)
                {
                    stencilUV.y += _spectrum26;
                }
                else if(stencilUV.x <= 0.54)
                {
                    stencilUV.y += _spectrum27;
                }
                else if(stencilUV.x <= 0.56)
                {
                    stencilUV.y += _spectrum28;
                }
                else if(stencilUV.x <= 0.58)
                {
                    stencilUV.y += _spectrum29;
                }
                else if(stencilUV.x <= 0.6)
                {
                    stencilUV.y += _spectrum30;
                }
                else if(stencilUV.x <= 0.62)
                {
                    stencilUV.y += _spectrum31;
                }
                else if(stencilUV.x <= 0.64)
                {
                    stencilUV.y += _spectrum32;
                }
                else if(stencilUV.x <= 0.66)
                {
                    stencilUV.y += _spectrum33;
                }
                else if(stencilUV.x <= 0.68)
                {
                    stencilUV.y += _spectrum34;
                }
                else if(stencilUV.x <= 0.7)
                {
                    stencilUV.y += _spectrum35;
                }
                else if(stencilUV.x <= 0.72)
                {
                    stencilUV.y += _spectrum36;
                }
                else if(stencilUV.x <= 0.74)
                {
                    stencilUV.y += _spectrum37;
                }
                else if(stencilUV.x <= 0.76)
                {
                    stencilUV.y += _spectrum38;
                }
                else if(stencilUV.x <= 0.78)
                {
                    stencilUV.y += _spectrum39;
                }
                else if(stencilUV.x <= 0.8)
                {
                    stencilUV.y += _spectrum40;
                }
                else if(stencilUV.x <= 0.82)
                {
                    stencilUV.y += _spectrum41;
                }
                else if(stencilUV.x <= 0.84)
                {
                    stencilUV.y += _spectrum42;
                }
                else if(stencilUV.x <= 0.86)
                {
                    stencilUV.y += _spectrum43;
                }
                else if(stencilUV.x <= 0.88)
                {
                    stencilUV.y += _spectrum44;
                }
                else if(stencilUV.x <= 0.9)
                {
                    stencilUV.y += _spectrum45;
                }
                else if(stencilUV.x <= 0.92)
                {
                    stencilUV.y += _spectrum46;
                }
                else if(stencilUV.x <= 0.94)
                {
                    stencilUV.y += _spectrum47;
                }
                else if(stencilUV.x <= 0.96)
                {
                    stencilUV.y += _spectrum48;
                }
                else if(stencilUV.x <= 0.98)
                {
                    stencilUV.y += _spectrum49;
                }
                else if(stencilUV.x <= 1.0)
                {
                    stencilUV.y += _spectrum50;
                }
                
                return stencilUV;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 stencil = tex2D(_StencilTex, GetStencilUV(i.uv));//UVを補正
                fixed4 color = lerp(col, _Color, _alpha);
                return lerp(col, color, stencil.r);
            }
            ENDCG
        }
    }
}
