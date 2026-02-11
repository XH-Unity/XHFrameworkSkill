Shader "FX/FX_LbeerURP"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_Cull_Mode("Cull_Mode", Float) = 0
        [Enum(Off,2,On,8)]_Depth("Depth", Float) = 2
        [Enum(Off,0,On,1)]_ZWrite0("ZWrite", Float) = 0
        [Enum(Add,1,Blend,10)]_AddBlend("Add/Blend", Float) = 10
        [Enum(Off,0,On,1)]_Custom("Custom", Float) = 0
        _Opacity_power("Opacity_power", Float) = 1
        _Opacity_scale("Opacity_scale", Float) = 1
        [KeywordEnum(Off,On)]_Use_Depth_fade("Use_Depth_fade", Float) = 0
        _Depth_fade("Depth_fade", Float) = 0
        [HDR]_Main_color("Main_color", Color) = (1,1,1,1)
        [NoScaleOffset]_Main_tex("Main_tex", 2D) = "white" {}
        _Main_Tex("Main_Tex", Vector) = (1,1,0,0)
        
        
        [HideInInspector]_MainTex ("占位Main_Tex(不生效)", 2D) = "white" {}
        
        _Main_Rotator("Main_Rotator", Float) = 0
        
        [KeywordEnum(R,G,B,A)] _Main_Split("Main_Split", Float) = 0
        [Enum(Off,0,On,1)]_Use_base_color("Use_base_color", Float) = 0
        [Enum(Off,0,On,1)]_Main_tex_clamp("Main_tex_clamp", Float) = 0
        [Enum(Off,0,On,1)]_Main_tex_radial("Main_tex_radial", Float) = 0
        [Enum(OFF,0,ON,1)]_Main_screen_UV("Main_screen_UV", Float) = 0
        _Main_speed_U("Main_speed_U", Float) = 0
        _Main_speed_V("Main_speed_V", Float) = 0
        
        [Space]
        [Color][Header(____________________AdditionalTex____________________)]
        [Space]
        [Toggle(_ADDITIONALTEXTOGGLE_ON)]_AdditionalTexToggle ("附加贴图开关", int) = 0.0
        [NoScaleOffset]_AdditionalTex("AdditionalTex", 2D) = "white" {}
        [HDR]_DegreeOfFusionColor("融合的颜色", Color) = (1,1,1,1)
        _DegreeOfFusion("融合程度", Range( 0 , 1)) = 0.0
        _Additional_Tex("Additional_Tex", Vector) = (1,1,0,0)
//        _Additional_Rotator("Additional_Rotator", Float) = 0
//        [Enum(Off,0,On,1)]_Additional_tex_clamp("Additional_tex_clamp", Float) = 0
//        [Enum(Off,0,On,1)]_Additional_tex_radial("Additional_tex_radial", Float) = 0
        _Additional_speed_U("Additional_speed_U", Float) = 0
        _Additional_speed_V("Additional_speed_V", Float) = 0
        
        
        
        [Space]
        [Color][Header(____________________Mask____________________)]
        [Space]
        [Toggle(_MASKTOGGLE_ON)]_MaskToggle ("Mask开关", int) = 1.0
        [NoScaleOffset]_Mask_tex("Mask_tex", 2D) = "white" {}
        [KeywordEnum(R,G,B,A)] _Mask_Split("Mask_Split", Float) = 0
        _Mask_Tex("Mask_Tex", Vector) = (1,1,0,0)
        _Mask_Rotator("Mask_Rotator", Float) = 0
        [Enum(Off,0,On,1)]_Mask_tex_clamp("Mask_tex_clamp", Float) = 0
        [Enum(Off,0,On,1)]_Mask_tex_radial("Mask_tex_radial", Float) = 0
        _Mask_speed_U("Mask_speed_U", Float) = 0
        _Mask_speed_V("Mask_speed_V", Float) = 0
        
        [Header(____________________Noise____________________)]
        [Space]
        [Toggle(_NOISETOGGLE_ON)]_NoiseToggle ("Noise开关", int) = 1.0
        [NoScaleOffset]_Noise_tex("Noise_tex", 2D) = "white" {}
        _Noise_Tex("Noise_Tex", Vector) = (1,1,0,0)
        _Noise_Rotator("Noise_Rotator", Float) = 0
        _Noise_scale("Noise_scale", Float) = 0
        [Enum(Off,0,On,1)]_Noise_tex_radial("Noise_tex_radial", Float) = 0
        _Noise_speed_U("Noise_speed_U", Float) = 0
        _Noise_speed_V("Noise_speed_V", Float) = 0
        
        [Header(____________________Dissolve____________________)]
        [Space]
        [Toggle(_DISSOLVETOGGLE_ON)]_DissolveToggle ("Dissolve开关", int) = 1.0
        [NoScaleOffset]_Dissolove_tex("Dissolove_tex", 2D) = "white" {}
        _Dissolve_Tex("Dissolve_Tex", Vector) = (1,1,0,0)
        _Dissolve_Rotator("Dissolve_Rotator", Float) = 0
        [Enum(Off,0,On,1)]_Dissove_tex_clamp("Dissove_tex_clamp", Float) = 0
        [Enum(Off,0,On,1)]_Dissolve_tex_radial("Dissolve_tex_radial", Float) = 0
        _Dissolve_speed_U("Dissolve_speed_U", Float) = 0
        _Dissolve_speed_V("Dissolve_speed_V", Float) = 0
        _Dissolve("Dissolve", Float) = 0
        _Dissolve_soft("Dissolve_soft", Range( 0 , 1)) = 0.5
        _Dissolve_edge("Dissolve_edge", Range( 0 , 1)) = 0
        [HDR]_Dissolve_color("Dissolve_color", Color) = (0,0,0,0)
        [Enum(Off,0,On,1)]_Use_dissolve_gradent("Use_dissolve_gradent", Float) = 0
        
        [NoScaleOffset]_Dissolvegradent("Dissolve-gradent", 2D) = "white" {}
        _Dissolve_gradent_Tex("Dissolve_gradent_Tex", Vector) = (1,1,0,0)
        _FixedPower1("Dissolve_gradent_width", Range( 0 , 1)) = 0.5
        _Dissolve_gradent_Rotator("Dissolve_gradent_Rotator", Float) = 0
        
        [Header(____________________GradedTex____________________)]
        [Space]
        [Toggle(_GRADEDTEXTOGGLE_ON)]GradedTexToggle ("Graded_texe开关", int) = 1.0
        [NoScaleOffset]_Gradent_tex("Gradent_tex", 2D) = "white" {}
        _GradedColor("Graded_color", Color) = (1,1,1,1)
        _Gradent_Tex("Gradent_Tex", Vector) = (1,1,0,0)
        [Enum(Off,0,On,1)]_Gradent_tex_radial("Gradent_tex_radial", Float) = 0
        _Gradent_Rotator("Gradent_Rotator", Float) = 0
        _Gradent_speed_U("Gradent_speed_U", Float) = 0
        _Gradent_speed_V("Gradent_speed_V", Float) = 0
        _Desatruate("Desatruate", Float) = 0
        
        
        [Header(____________________Vertex____________________)]
        [Space]
        [Toggle(_VERTEXTOGGLE_ON)]_VertexToggle ("Vertex开关", int) = 1.0
        [NoScaleOffset]_Vertex_tex("Vertex_tex", 2D) = "white" {}
        _Vertex_Tex("Vertex_Tex", Vector) = (1,1,0,0)
        _Vertex_Rotator("Vertex_Rotator", Float) = 0
        [Enum(Off,0,On,1)]_Vertex_tex_radial("Vertex_tex_radial", Float) = 0
        _Vertex_scale("Vertex_scale", Float) = 0
        _Vertex_speed_U("Vertex_speed_U", Float) = 0
        _Vertex_speed_V("Vertex_speed_V", Float) = 0
        
        [Header(____________________Fresnel____________________)]
        [Space]
        [Toggle(_FRESNELTOGGLE_ON)]_FresnelToggle ("Fresnel开关", int) = 1.0
        [Enum(Off,0,On,1)]_Fresnel_one_minus("Fresnel_one_minus", Float) = 0
        [Enum(Off,0,On,1)]_Use_fresnel_color("Use_fresnel_color", Float) = 0
        [Enum(Off,0,On,1)]_Use_fresnel_opacity("Use_fresnel_opacity", Float) = 0
        _Fresnelpower("Fresnel+power", Float) = 1
        [HDR]_Fresnel_color("Fresnel_color", Color) = (0,0,0,0)
        [HideInInspector] _texcoord( "", 2D ) = "white" {}
        [HideInInspector] _tex4coord2( "", 2D ) = "white" {}
        [HideInInspector] _tex4coord3( "", 2D ) = "white" {}
        [HideInInspector] __dirty( "", Int ) = 1
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

//        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Custom" "Queue" = "Transparent+0" "IsEmissive" = "true" "RenderPipeline" = "UniversalPipeline"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
        Cull [_Cull_Mode]
        ZWrite [_ZWrite0]
//        ZWrite On
        ZTest [_Depth]
        Blend SrcAlpha [_AddBlend]
        
        

        // ---- forward rendering base pass:
        Pass
        {
            //			Name "FORWARD"
            //			Tags { "LightMode" = "ForwardBase" }

            HLSLPROGRAM
            // compile directives
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature_local _MAIN_SPLIT_R _MAIN_SPLIT_G _MAIN_SPLIT_B _MAIN_SPLIT_A
            // #pragma shader_feature _MAIN_SPLIT_R
            #pragma shader_feature_local _MASK_SPLIT_R _MASK_SPLIT_G _MASK_SPLIT_B _MASK_SPLIT_A
            // #pragma shader_feature _MASK_SPLIT_R
            // #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
            
            #pragma shader_feature_local _ _MASKTOGGLE_ON  // 噪波贴图开关
            #pragma shader_feature_local _ _NOISETOGGLE_ON  // 噪波贴图开关
            #pragma shader_feature_local _ _DISSOLVETOGGLE_ON  // 溶解效果开关
            #pragma shader_feature_local _ _GRADEDTEXTOGGLE_ON  // 梯度遮罩开关
            #pragma shader_feature_local _ _ADDITIONALTEXTOGGLE_ON  // 梯度alpha开关
            #pragma shader_feature_local _ _VERTEXTOGGLE_ON  // 顶点动画开关
            #pragma shader_feature_local _ _FRESNELTOGGLE_ON  // 菲尼尔效果开关
            #pragma shader_feature_local   _USE_DEPTH_FADE_OFF _USE_DEPTH_FADE_ON  // 深度渐变开关

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            #undef TRANSFORM_TEX

            // 用于在着色器中快速转换纹理坐标的宏，可以提高代码的可读性和开发效率。
            #define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
            #define UNITY_PI 3.14159265359
            // struct Input
            // {
            // 	float2 uv_texcoord;
            // 	float4 uv2_tex4coord2;
            // 	float4 vertexColor : COLOR;
            // 	float3 worldNormal;
            // 	float3 viewDir;
            // 	float4 uv3_tex4coord3;
            // 	float4 screenPos;
            // };

            // 顶点动画的采样函数
            // #if defined(_VERTEXTOGGLE_ON)
            TEXTURE2D(_Vertex_tex);
            SAMPLER(sampler_Vertex_tex);
            // #endif
            
            // 深度获取
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_Noise_tex);
            SAMPLER(sampler_Noise_tex);
            TEXTURE2D(_Main_tex);
            SAMPLER(sampler_Main_tex);
            TEXTURE2D(_Gradent_tex);
            SAMPLER(sampler_Gradent_tex);
            TEXTURE2D(_Dissolove_tex);
            SAMPLER(sampler_Dissolove_tex);
            TEXTURE2D(_Dissolvegradent);
            SAMPLER(sampler_Dissolvegradent);
            TEXTURE2D(_Mask_tex);
            SAMPLER(sampler_Mask_tex);

            TEXTURE2D(_AdditionalTex);
            SAMPLER(sampler_AdditionalTex);

            CBUFFER_START(UnityPerMaterial)
            half _Depth;
            half _AddBlend;
            half _Cull_Mode;
            

            // #if defined(_DISSOLVETOGGLE_ON)
                half4 _Dissolve_Tex;
                float4 _Dissolove_tex_ST;
                half _Dissolve_Rotator;
                half _Dissove_tex_clamp;
                half _Dissolve_tex_radial;
                half _Dissolve_speed_U;
                half _Dissolve_speed_V;
                half _Dissolve;
                half _Dissolve_soft;
                half _Dissolve_edge;
                half4 _Dissolve_color;
                half _Use_dissolve_gradent;

                half4 _Dissolve_gradent_Tex;
                half _FixedPower1;
                half _Dissolve_gradent_Rotator;
            // #endif

            // 梯度遮罩
            // #if defined(_GRADEDTEXTOGGLE_ON)
                float4 _Gradent_Tex;
                float _Gradent_tex_radial;
                float4 _Gradent_tex_ST;
                float _Gradent_Rotator;
                float _Gradent_speed_U;
                float _Gradent_speed_V;
                float _Desatruate;
                half4 _GradedColor;
            // #endif
            
            // 顶点动画的参数
            // #if defined(_VERTEXTOGGLE_ON)
                half4 _Vertex_Tex;
                half _Vertex_Rotator;
                half _Vertex_tex_radial;
                half _Vertex_scale;
                half _Vertex_speed_U;
                half _Vertex_speed_V;
                float4 _Vertex_tex_ST;
            // #endif

            // 菲尼尔
            // #if defined(_FRESNELTOGGLE_ON)
                half _Fresnel_one_minus;
                half _Use_fresnel_color;
                half _Use_fresnel_opacity;
                half _Fresnelpower;
                half4 _Fresnel_color;
                
            // #endif
            
            float4 _Noise_tex_ST;
            
            
            
            
            
            half _Custom;

            half _Main_speed_U;
            half _Main_speed_V;
            float4 _Main_tex_ST;
            half _Main_Rotator;
            half4 _Main_Tex;
            half _Main_tex_radial;
            half _Main_tex_clamp;
            half _Noise_speed_U;
            half _Noise_speed_V;
            half _Noise_Rotator;
            half4 _Noise_Tex;
            half _Noise_tex_radial;
            half _Noise_scale;
            half _Use_base_color;
            half _DegreeOfFusion;
            half4 _Main_color;
            half4 _DegreeOfFusionColor;
            // sampler2D _Gradent_tex;

            
            
            // sampler2D _Mask_tex;
            half _Additional_speed_U;
            half _Additional_speed_V;
            float4 _AdditionalTex_ST;
            half4 _Additional_Tex;

            half _Mask_speed_U;
            half _Mask_speed_V;
            float4 _Mask_tex_ST;
            half _Mask_Rotator;
            half4 _Mask_Tex;
            half _Mask_tex_radial;
            half _Mask_tex_clamp;
            
            half _Opacity_power;
            half _Opacity_scale;
            half4 _CameraDepthTexture_TexelSize;
            half _Depth_fade;
            half _Use_Depth_fade;
            float4 _texcoord_ST;
            float4 _tex4coord2_ST;
            float4 _tex4coord3_ST;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                half4 color : COLOR;
            };

            struct Varyings
            {
                // UNITY_POSITION(pos);
                float4 positionCS : SV_POSITION;
                float2 pack0 : TEXCOORD0; // _texcoord
                float4 pack1 : TEXCOORD1; // _tex4coord2
                float4 pack2 : TEXCOORD2; // _tex4coord3
                float3 worldNormal : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                half4 color : COLOR0;
            };

            

            // ----------色彩空间转换函数----------
            half3 GammaToLinearSpace(half3 sRGB)
            {
                // return  sRGB;
                return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
            }

            half GammaToLinearSpaceH1(half value)
            {
                // return value;
                return value * (value * (value * 0.305306011h + 0.682171111h) + 0.012522878h);
            }

            half3 LinearToGammaSpace(half3 linRGB)
            {
                linRGB = max(linRGB, half3(0.h, 0.h, 0.h));
                return max(1.055h * pow(linRGB, 0.416666667h) - 0.055h, 0.h);
            }

            half LinearToGammaSpaceH1(half value)
            {
                return max(1.055h * pow(value, 0.416666667h) - 0.055h, 0.h);
            }

            // 将重复的代码片段封装成函数，有助于减少代码冗余且使代码更易于管理和阅读。
            float2 TransformUV(float2 uv, float rotator)
            {
                float rad = rotator * UNITY_PI / 180.0;
                float cosVal = cos(rad);
                float sinVal = sin(rad);
                return mul(uv - float2(0.5, 0.5), float2x2(cosVal, -sinVal, sinVal, cosVal)) + float2(0.5, 0.5);
            }


            // 顶点 shader
            Varyings vert(Attributes v)
            {
                Varyings o;

                // 顶点动画
                // #if defined(_VERTEXTOGGLE_ON)
                // // Varyings o;
                // ///
                // // 更合理的数据包装与传递,如果某些变量如_Vertex_scale和Custom2_z315经常一起使用，考虑将它们预先计算或封装到一起，以减少在渲染管道中的传递和处理次数。
                // // 将结果添加到 float2 类型的变量中
                // float2 appendResult392 = float2(_Vertex_speed_U, _Vertex_speed_V);
                // // 计算噪音纹理的uv坐标
                // float2 uv_Vertex_tex = v.texcoord.xy * _Vertex_tex_ST.xy + _Vertex_tex_ST.zw;
                //
                // // 根据旋转角度对uv坐标进行旋转变换
                // // float2 rotator384 = mul(uv_Noise_tex - float2(0.5, 0.5), float2x2(cos384, -sin384, sin384, cos384)) + float2(0.5, 0.5);
                // float2 rotator384 = TransformUV(uv_Vertex_tex, _Vertex_Rotator);
                // // 对uv坐标进行缩放和偏移
                // float2 uv_TexCoord30_g36 = v.texcoord.xy * float2(2, 2);
                // // 对临时输出变量进行处理
                // float2 temp_output_25_0_g36 = uv_TexCoord30_g36 - float2(1, 1);
                // // 将结果添加到 float2 类型的变量中
                // float2 appendResult21_g36 = (float2(
                //     frac((atan2((temp_output_25_0_g36).x, (temp_output_25_0_g36).y) / 6.28318548202515)),
                //     length(temp_output_25_0_g36)));
                // // 对临时输出变量进行处理
                // float4 temp_output_40_0_g36 = _Vertex_Tex;
                // // 对两个结果进行插值
                // float2 lerpResult393 = lerp(((rotator384 * (_Vertex_Tex).xy) + (_Vertex_Tex).zw),
                //                             ((appendResult21_g36 * (temp_output_40_0_g36).xy) + (temp_output_40_0_g36).
                //                                 zw), _Vertex_tex_radial);
                // // 根据时间、速度和插值结果对变量进行平移
                // float2 panner396 = (_Time.y * appendResult392 + lerpResult393);
                // // 获取顶点的自定义信息
                // float Custom2_z315 = v.texcoord2.z;
                // // 获取自定义值
                // float Use_custom327 = _Custom;
                // // 对两个值进行插值
                // float lerpResult400 = lerp(_Vertex_scale, (_Vertex_scale + Custom2_z315), Use_custom327);
                // // 获取顶点法线信息
                // float3 ase_vertexNormal = v.normal.xyz;
                // // 计算顶点偏移值
                // // 尝试减少对tex2Dlod等重资源调用的使用，或者合并可以一起处理的纹理采样操作。
                // // float3 Vertexoffset403 = tex2Dlod( _Vertex_tex, float4( panner396, 0, 0.0) ).r * lerpResult400 * ase_vertexNormal;
                // float3 Vertexoffset403 = _Vertex_tex.SampleLevel(sampler_Vertex_tex, panner396, 0.0).r * lerpResult400 *
                //     ase_vertexNormal;
                // // Vertexoffset403 = LinearToGammaSpace(Vertexoffset403);
                // // Vertexoffset403.rgb = GammaToLinearSpace(Vertexoffset403.rgb);
                // // 更新顶点位置
                // v.positionOS.xyz += Vertexoffset403;
                // // 更新顶点w值
                // v.positionOS.w = 1;
                // o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                // // #else
                // //
                // // o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                // #endif

                ///
                // 将顶点位置转换为裁剪空间中的位置
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                // 对第一个纹理坐标进行转换并打包到o.pack0的xy分量中
                o.pack0.xy = TRANSFORM_TEX(v.texcoord, _texcoord).xy;
                // 对第二个纹理坐标进行转换并打包到o.pack1的xyzw分量中
                o.pack1.xyzw = TRANSFORM_TEX(v.texcoord1, _tex4coord2);
                // 对第三个纹理坐标进行转换并打包到o.pack2的xyzw分量中
                o.pack2.xyzw = TRANSFORM_TEX(v.texcoord2, _tex4coord3);
                // 将顶点位置从局部空间转换到世界空间，并提取出xyz分量
                float3 worldPos = mul(unity_ObjectToWorld, v.positionOS).xyz;
                // 将顶点法线从局部空间转换到世界空间
                float3 worldNormal = TransformObjectToWorldNormal(v.normal);


                o.worldPos.xyz = worldPos;
                o.worldNormal = worldNormal;
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.color = v.color;
                return o;
            }

            // 片元 shader
            half4 frag(Varyings i) : SV_Target
            {
                // 您多次使用atan2和length计算，考虑是否有可能调整算法来减少这类重复的代价高昂的数学运算。
                // 若某些变量仅在创建后的几行代码中使用然后就不再使用，可以考虑内联这些变量以减少总变量数和提高代码的紧凑性。
                // 如多次访问相同纹理或者使用相同的参数调用同一个函数，可以将结果存储在一个变量中以避免重复调用。
                // 对于GPU着色器代码，考虑数据的并行计算性，尽量避免长串的依赖链，优化线程的利用。
                // 对于片元着色器，考虑使用预计算的常量，减少计算量，提高性能。
                // 减少自定义的操作，比如atan2和length的运算可以结合现有的GPU优化来实现。
                // 避免在每个像素上执行昂贵的操作,例如，角度到弧度的转换或常数计算应尽可能地在顶点着色器中完成或事先计算，而不是像素着色器中。
                // 计算视图方向
                
                // 计算主纹理坐标
                float2 appendResult15 = (float2(_Main_speed_U, _Main_speed_V));
                float2 uv_Main_tex = i.pack0.xy * _Main_tex_ST.xy + _Main_tex_ST.zw;

                // 计算旋转角度对应的余弦和正弦值
                // float radPerDeg = UNITY_PI / 180.0;
                // float cos256 = cos(_Main_Rotator * radPerDeg);
                // float sin256 = sin(_Main_Rotator * radPerDeg);

                // uv_Main_tex - float2( 0.5,0.5 )多次使用
                // float2 rotator256 = mul(uv_Main_tex - float2(0.5, 0.5), float2x2(cos256, -sin256, sin256, cos256)) + float2(0.5, 0.5);
                float2 rotator256 = TransformUV(uv_Main_tex, _Main_Rotator);

                // 计算其他变量
                half2 temp_output_59_0 = _Main_Tex.xy;
                half2 temp_output_61_0 = _Main_Tex.zw;
                float2 Custom1_xy311 = i.pack1.xy;
                half Use_custom327 = _Custom;
                float2 lerpResult324 = lerp(temp_output_61_0, temp_output_61_0 + Custom1_xy311, Use_custom327);
                float2 uv_TexCoord30_g25 = i.pack0.xy * float2(2, 2);
                float2 temp_output_25_0_g25 = (uv_TexCoord30_g25 - float2(1, 1));

                // 计算其他变量
                float2 appendResult21_g25 = float2(
                    frac(atan2(temp_output_25_0_g25.x, temp_output_25_0_g25.y) / 6.28318548202515),
                    length(temp_output_25_0_g25));
                half4 appendResult329 = (half4(temp_output_59_0, lerpResult324));
                half4 temp_output_40_0_g25 = appendResult329;
                float2 lerpResult69 = lerp(rotator256 * temp_output_59_0 + lerpResult324,
                                           appendResult21_g25 * temp_output_40_0_g25.xy + temp_output_40_0_g25.
                                               zw, _Main_tex_radial);
                float2 panner68 = 1.0 * _Time.y * appendResult15 + lerpResult69;
                float2 lerpResult85 = lerp(panner68, saturate(panner68), _Main_tex_clamp);

                // 计算噪声纹理坐标
                float Noise43 = 0.0;
                #if defined(_NOISETOGGLE_ON)
                    float2 appendResult113 = float2(_Noise_speed_U, _Noise_speed_V);
                    float2 uv_Noise_tex = i.pack0.xy * _Noise_tex_ST.xy + _Noise_tex_ST.zw;

                    float2 rotator265 = TransformUV(uv_Noise_tex, _Noise_Rotator);
                    
                    float2 uv_TexCoord30_g22 = i.pack0.xy * float2(2, 2);
                    float2 temp_output_25_0_g22 = uv_TexCoord30_g22 - float2(1, 1);
                    float2 appendResult21_g22 = float2(
                        frac(atan2(temp_output_25_0_g22.x, temp_output_25_0_g22.y) / 6.28318548202515),
                        length(temp_output_25_0_g22));
                    float4 temp_output_40_0_g22 = _Noise_Tex;
                    float2 lerpResult114 = lerp(rotator265 * _Noise_Tex.xy + _Noise_Tex.zw,
                                                appendResult21_g22 * temp_output_40_0_g22.xy + temp_output_40_0_g22.
                                                    zw, _Noise_tex_radial);
                    float2 panner115 = 1.0 * _Time.y * appendResult113 + lerpResult114;

                    // 计算噪声纹理的缩放值
                    float Custom1_w313 = i.pack1.xyzw.w;
                    float lerpResult336 = lerp(_Noise_scale, (_Noise_scale + Custom1_w313), Use_custom327);

                    // 计算噪声值
                    half NoiseTex = SAMPLE_TEXTURE2D(_Noise_tex, sampler_Noise_tex, panner115).r;
                    NoiseTex = GammaToLinearSpaceH1(NoiseTex);
                     Noise43 = (NoiseTex -0.5) * lerpResult336;
                #endif

                
                    
                
                
                // 应用纹理采样
                half4 tex2DNode1 = SAMPLE_TEXTURE2D(_Main_tex, sampler_Main_tex,  lerpResult85 + Noise43);
                
                // tex2DNode1.rgb = LinearToGammaSpace(tex2DNode1.rgb);
                // tex2DNode1.rgb = GammaToLinearSpace(tex2DNode1.rgb);
                tex2DNode1.a = GammaToLinearSpaceH1(tex2DNode1.a);

                #if defined(_MAIN_SPLIT_R)
                float staticSwitch169 = tex2DNode1.r;
                #elif defined(_MAIN_SPLIT_G)
					float staticSwitch169 = tex2DNode1.g;
                #elif defined(_MAIN_SPLIT_B)
					float staticSwitch169 = tex2DNode1.b;
                #elif defined(_MAIN_SPLIT_A)
					float staticSwitch169 = tex2DNode1.a;
                #endif
                half4 temp_cast_0 = staticSwitch169.xxxx;
                half4 lerpResult170 = lerp(temp_cast_0, tex2DNode1, _Use_base_color);
                
                // 附加贴图融合
                #if defined(_ADDITIONALTEXTOGGLE_ON)
                    float2 UVTime = float2(_Additional_speed_U, _Additional_speed_V);

                    // 计算uv_Mask_tex
                    float2 uv_Additional = i.pack0.xy * _AdditionalTex_ST.xy + _AdditionalTex_ST.zw;

                    float2 additionalTexST = uv_Additional * _Additional_Tex.xy + _Additional_Tex.zw;

                    // 计算panner131
                    float2 additionalTexUV =1.0 * _Time.y * UVTime + additionalTexST;

                    // 对panner131进行插值运算，并使用saturate函数进行饱和处理
                    // float2 additionalTexUV = lerp(panner131, saturate(panner131), _Mask_tex_clamp);

                    // 对lerpResult144进行采样
                    half4 additionalTex = SAMPLE_TEXTURE2D(_AdditionalTex, sampler_AdditionalTex, additionalTexUV) * _DegreeOfFusionColor;
                
                    additionalTex = lerp(lerpResult170, additionalTex, additionalTex.a);
                    lerpResult170 = lerp(lerpResult170, additionalTex,_DegreeOfFusion);
                #else
                
                    lerpResult170 = lerpResult170;
                #endif
                    

                
                // 顶点色与主颜色混合
                // 计算渐变速度
                // float3 mainMultiplyVertexColor = i.color.rgb * (_Main_color.rgb);
                half3 mainMultiplyVertexColor = i.color.rgb * LinearToGammaSpace(_Main_color.rgb);

                
                // 计算梯度遮罩
                half3 gradient = half3(1, 1, 1);
                half4 desaturateInitialColor253 = half4(1, 1, 1, 1);
                #if defined(_GRADEDTEXTOGGLE_ON)
                    // 计算经过纹理平铺和偏移后的uv坐标
                    float2 uv_Gradent_tex = i.pack0.xy * _Gradent_tex_ST.xy + _Gradent_tex_ST.zw;
                    float2 rotator272 = TransformUV(uv_Gradent_tex, _Gradent_Rotator);
                    // 计算纹理缩放后的uv坐标
                    float2 uv_TexCoord30_g27 = i.pack0.xy * float2(2, 2);
                    // 计算渐变纹理uv偏移
                    float2 temp_output_25_0_g27 = (uv_TexCoord30_g27 - float2(1, 1));
                    // 计算渐变纹理uv偏移的余弦和正弦值
                    float2 appendResult21_g27 = (float2(
                        frac((atan2((temp_output_25_0_g27).x, (temp_output_25_0_g27).y) / 6.28318548202515)),
                        length(temp_output_25_0_g27)));

                    // 计算渐变纹理uv偏移的uv坐标
                    // 在两种纹理坐标之间进行插值
                    float2 lerpResult244 = lerp(rotator272 * _Gradent_Tex.xy + _Gradent_Tex.zw,
                                                appendResult21_g27 * _Gradent_Tex.xy + _Gradent_Tex.zw, _Gradent_tex_radial);
                    float2 appendResult245 = float2(_Gradent_speed_U, _Gradent_speed_V);
                    float2 panner246 = 1.0 * _Time.y * appendResult245 + lerpResult244;
                     desaturateInitialColor253 = SAMPLE_TEXTURE2D(_Gradent_tex, sampler_Gradent_tex, panner246) * _GradedColor;
                // GammaToLinearSpace(desaturateInitialColor253);
                    // 计算颜色灰度值
                    const float redWeight = 0.299;
                    const float greenWeight = 0.587;
                    const float blueWeight = 0.114;
                    float desaturateDot253 = dot(desaturateInitialColor253.rgb, float3(redWeight, greenWeight, blueWeight));

                    // 渐变，去掉饱和度
                     gradient = lerp(desaturateInitialColor253.rgb, desaturateDot253.xxx, _Desatruate);
                #endif
                // float NdotV = dot(i.worldNormal, viewDir);
                
                
                
                // Fresnel计算
                half4 lerpResult300 = float4(0, 0, 0, 0);
                float lerpResult297 = 1; 
                #if defined(_FRESNELTOGGLE_ON)
                    half3 worldViewDir = normalize(GetCameraPositionWS() - i.worldPos.xyz);
                    half NdotV = saturate(dot(i.worldNormal, worldViewDir));
                    // 使用插值函数计算结果
                    half lerpResult282 = lerp((1.0 - NdotV), NdotV, _Fresnel_one_minus);
                    // 对插值结果进行幂运算并确保结果在0到1之间
                    half temp_output_286_0 = saturate(pow(saturate(lerpResult282), _Fresnelpower));
                    // 计算反射颜色
                    half4 Fresnel_color293 = temp_output_286_0 * half4(LinearToGammaSpace(_Fresnel_color.rgb), _Fresnel_color.a);
                    // half4 Fresnel_color293 = temp_output_286_0 * _Fresnel_color;
                    // lerp Fresnel菲涅耳
                     lerpResult300 = lerp(float4(0, 0, 0, 0), Fresnel_color293, _Use_fresnel_color);
                     lerpResult297 = lerp(1.0, temp_output_286_0, _Use_fresnel_opacity);
                #endif
                

                
                // 溶解效果
                half temp_output_183_0 = 1.0;
                half3 dissolveEdgeColor = half3(0, 0, 0);
                
                #if defined(_DISSOLVETOGGLE_ON)
                    // 计算溶解速度
                    float2 appendResult158 = (float2(_Dissolve_speed_U, _Dissolve_speed_V));
                    // 计算溶解纹理UV坐标
                    float2 uv_Dissolove_tex = i.pack0.xy * _Dissolove_tex_ST.xy + _Dissolove_tex_ST.zw;

                    float2 rotator268 = TransformUV(uv_Dissolove_tex, _Dissolve_Rotator);
                    // 计算溶解后的uv坐标
                    float2 temp_output_148_0 = _Dissolve_Tex.xy;
                    float2 temp_output_150_0 = _Dissolve_Tex.zw;
                    // 获取自定义坐标
                    float2 Custom2_xy314 = i.pack2.xy;
                    // 对给定的输入执行线性插值
                    float2 lerpResult373 = lerp(temp_output_150_0, (temp_output_150_0 + Custom2_xy314), Use_custom327);
                    // 计算纹理坐标乘以常量向量 (2, 2)
                    float2 uv_TexCoord30_g21 = i.pack0.xy * float2(2, 2);
                    // 使用常量向量 (1, 1) 减去纹理坐标
                    float2 temp_output_25_0_g21 = (uv_TexCoord30_g21 - float2(1, 1));
                    // 计算新的浮点数向量，包括 atan2 计算和向量长度计算结果
                    float2 appendResult21_g21 = (float2(
                        frac((atan2((temp_output_25_0_g21).x, (temp_output_25_0_g21).y) / 6.28318548202515)),
                        length(temp_output_25_0_g21)));
                    // 创建新的浮点数向量，包括 temp_output_148_0 和 lerpResult373 的值
                    float4 appendResult374 = (float4(temp_output_148_0, lerpResult373));
                    // 复制 appendResult374 到 temp_output_40_0_g21
                    float4 temp_output_40_0_g21 = appendResult374;
                    // 对给定的输入执行线性插值
                    float2 lerpResult157 = lerp(((saturate(rotator268) * temp_output_148_0) + lerpResult373),
                                                ((appendResult21_g21 * (temp_output_40_0_g21).xy) + (temp_output_40_0_g21).
                                                    zw), _Dissolve_tex_radial);
                    // 执行平移变换乘法并添加 lerpResult157
                    float2 panner159 = (1.0 * _Time.y * appendResult158 + lerpResult157);
                    // 对给定的输入执行线性插值
                    float2 lerpResult162 = lerp(panner159, saturate(panner159), _Dissove_tex_clamp);
                    // 返回纹理的颜色值，纹理坐标由 lerpResult162 给出
                
                    half4 tex2DNode163 = SAMPLE_TEXTURE2D(_Dissolove_tex, sampler_Dissolove_tex, lerpResult162);
                    // tex2DNode163.rgb = LinearToGammaSpace(tex2DNode163);
                    tex2DNode163.rgb = GammaToLinearSpace(tex2DNode163.rgb);
                    float2 rotator363 = TransformUV(uv_Dissolove_tex, _Dissolve_gradent_Rotator);
                    
                    // 根据条件进行插值
                    half Dissolvegradent = SAMPLE_TEXTURE2D( _Dissolvegradent, sampler_Dissolvegradent, (  saturate( rotator363 ) * _Dissolve_gradent_Tex.xy  + _Dissolve_gradent_Tex.zw ) ).r;
                // LinearToGammaSpaceH1(Dissolvegradent);
                // GammaToLinearSpace(Dissolvegradent);
                    half lerpResult228 = lerp( tex2DNode163.r , ( tex2DNode163.r - ( 1.0 -  Dissolvegradent / saturate( ( 1.0 - _FixedPower1 ) )  ) ) , _Use_dissolve_gradent);

                    // 获取特定值
                    half Custom1_z312 = i.pack1.xyzw.z;
                    // 根据条件进行插值
                    half lerpResult331 = lerp(_Dissolve, (_Dissolve + Custom1_z312), Use_custom327);
                    // 计算临时结果
                     temp_output_183_0 = saturate(
                        ((((lerpResult228 + 1.0) - (lerpResult331 * (1.0 + (1.0 - _Dissolve_soft)))) -
                            _Dissolve_soft) / (1.0 - _Dissolve_soft)));

                    // 溶解的边缘颜色
                     dissolveEdgeColor = ((temp_output_183_0 - saturate(
                            ((((lerpResult228 + 1.0) - ((lerpResult331 + _Dissolve_edge) * (1.0 + (1.0 -
                                _Dissolve_soft)))) - _Dissolve_soft) / (1.0 - _Dissolve_soft)))) *
                        _Dissolve_color).rgb;

                #endif
               
                
                 
                

                // mask 效果
                half staticSwitch304 = 1.0;
                #if defined(_MASKTOGGLE_ON)
                    // 将_Mask_speed_U和_Mask_speed_V组成float2类型的变量appendResult129
                    float2 appendResult129 = float2(_Mask_speed_U, _Mask_speed_V);

                    // 计算uv_Mask_tex
                    float2 uv_Mask_tex = i.pack0.xy * _Mask_tex_ST.xy + _Mask_tex_ST.zw;


                    float2 rotator263 = TransformUV(uv_Mask_tex, _Mask_Rotator);
                    // 计算uv_TexCoord30_g26
                    float2 uv_TexCoord30_g26 = i.pack0.xy * float2(2, 2);

                    // 计算temp_output_25_0_g26
                    float2 temp_output_25_0_g26 = (uv_TexCoord30_g26 - float2(1, 1));

                    // 计算appendResult21_g26
                    float2 appendResult21_g26 = (float2(
                        frac((atan2((temp_output_25_0_g26).x, (temp_output_25_0_g26).y) / 6.28318548202515)),
                        length(temp_output_25_0_g26)));

                    // 获取_Mask_Tex的值
                    float4 temp_output_40_0_g26 = _Mask_Tex;

                    // 使用lerp函数对(rotator263 * (_Mask_Tex).xy + (_Mask_Tex).zw)和(appendResult21_g26 * (temp_output_40_0_g26).xy + (temp_output_40_0_g26).zw)进行插值
                    float2 lerpResult130 = lerp(((rotator263 * (_Mask_Tex).xy) + (_Mask_Tex).zw),
                                                ((appendResult21_g26 * (temp_output_40_0_g26).xy) + (temp_output_40_0_g26).
                                                    zw), _Mask_tex_radial);

                    // 计算panner131
                    float2 panner131 = (1.0 * _Time.y * appendResult129 + lerpResult130);

                    // 对panner131进行插值运算，并使用saturate函数进行饱和处理
                    float2 lerpResult144 = lerp(panner131, saturate(panner131), _Mask_tex_clamp);

                    // 对lerpResult144进行采样
                    half4 tex2DNode133 = SAMPLE_TEXTURE2D(_Mask_tex, sampler_Mask_tex, lerpResult144);
                // GammaToLinearSpace(tex2DNode133);

                    //优化条件编译指令，多次使用的条件编译指令可以通过预处理宏来设置默认值，减少条件判断的复杂度。
                    // #ifndef _MASK_SPLIT_R
                    // #define _MASK_SPLIT_R
                    // #endif

                    #if defined(_MASK_SPLIT_R)
                     staticSwitch304 = tex2DNode133.r;
                    #elif defined(_MASK_SPLIT_G)
					     staticSwitch304 = tex2DNode133.g;
                    #elif defined(_MASK_SPLIT_B)
					     staticSwitch304 = tex2DNode133.b;
                    #elif defined(_MASK_SPLIT_A)
					     staticSwitch304 = tex2DNode133.a;
                    #endif
                
                #endif
               
                // 计算着色器中的混合透明度
                half Dissovle164 = temp_output_183_0;
                half Opacity179 = i.color.a * _Main_color.a;
                
                
                // 计算屏幕位置及深度信息
                half4 ase_screenPos = float4(i.screenPos.xyz, i.screenPos.w + 0.00000000001);
                half4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
                ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0)
                                          ? ase_screenPosNorm.z
                                          : ase_screenPosNorm.z * 0.5 + 0.5;
                // float screenDepth351 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, ase_screenPosNorm.xy));
                // half screenDepth351 = LinearEyeDepth(
                //     SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, ase_screenPosNorm.x).r,
                //     _ZBufferParams);
                // float distanceDepth351 = saturate(abs((screenDepth351 - LinearEyeDepth(ase_screenPosNorm.z)) / (_Depth_fade)));
                // half distanceDepth351 = saturate(
                //     abs((screenDepth351 - LinearEyeDepth(ase_screenPosNorm.z, _ZBufferParams)) / (_Depth_fade)));
                // half lerpResult355 = lerp(1.0, distanceDepth351, _Use_Depth_fade);
                // half Depth_fade354 = saturate(lerpResult355);

                #if defined(_USE_DEPTH_FADE_ON)
                half Alpha = pow(saturate(staticSwitch304 * Dissovle164 * Opacity179 * lerpResult297 * staticSwitch169),
                                 _Opacity_power) * _Opacity_scale;
                #else
                half Alpha = pow(saturate(staticSwitch304 * Dissovle164 * Opacity179 * lerpResult297 * staticSwitch169),
                                 _Opacity_power) * _Opacity_scale;
                #endif
                // 计算最终的混合透明度

                // 最终颜色输出
                half3 Emission = lerpResult170.rgb * mainMultiplyVertexColor * gradient + lerpResult300.rgb + dissolveEdgeColor;
                // half staticSwitch = lerp(staticSwitch304, 1.0, _DegreeOfFusion);
                // half alpha = lerp(Alpha, 1.0, _DegreeOfFusion);
                // half3 Emission2 = lerp(lerpResult170.rgb * mainMultiplyVertexColor * gradient + lerpResult300.rgb + dissolveEdgeColor, staticSwitch * _DegreeOfFusionColor + Alpha * 0.8, alpha);

                //
                // UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz)
                half4 c = 0;

                
                
                c += half4(0, 0, 0, Alpha);
                c.rgb += Emission;
                

                // c.rgb = GammaToLinearSpace(c.rgb);
                // GammaToLinearSpace(c.rgb);
                // return additionalTex;
                return c;
            }
            ENDHLSL
        }
    }
    //	CustomEditor "ASEMaterialInspector"
}