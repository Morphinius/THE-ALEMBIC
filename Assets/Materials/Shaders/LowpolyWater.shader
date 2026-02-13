Shader "Custom/LowpolyWaterTriangles" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.0, 0.3, 0.6, 0.8)
        _ColorVariation ("Color Variation", Range(0, 0.3)) = 0.1
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveHeight ("Wave Height", Float) = 0.2
        _WaveFrequency ("Wave Frequency", Float) = 1.0
        _FoamIntensity ("Foam Intensity", Float) = 0.3
    }
    
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint vertexId : SV_VertexID;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float triangleHash : TEXCOORD2;
                float waveOffset : TEXCOORD3;
            };
            
            fixed4 _BaseColor;
            float _ColorVariation;
            float _WaveSpeed;
            float _WaveHeight;
            float _WaveFrequency;
            float _FoamIntensity;
            
            // Хэш-функция для генерации псевдослучайных чисел
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // Генерация уникального идентификатора для треугольника
            float getTriangleHash(uint vertexId) {
                // Группируем вершины по треугольникам (3 вершины на треугольник)
                uint triangleId = vertexId / 3;
                return rand(float2(triangleId, triangleId));
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // Получаем хэш треугольника для этого vertexId
                o.triangleHash = getTriangleHash(v.vertexId);
                
                // Создаем волны с разными фазами для разных треугольников
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float phase = o.triangleHash * 3.14159 * 2.0; // Разная фаза для каждого треугольника
                
                float wave = sin(worldPos.x * _WaveFrequency + _Time.y * _WaveSpeed + phase) * 
                           sin(worldPos.z * _WaveFrequency + _Time.y * _WaveSpeed + phase) * 
                           _WaveHeight;
                
                worldPos.y += wave;
                o.waveOffset = wave;
                
                o.vertex = UnityWorldToClipPos(worldPos);
                o.uv = v.uv;
                o.worldPos = worldPos;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // Базовый цвет с вариацией на основе хэша треугольника
                float colorVariation = (i.triangleHash - 0.5) * _ColorVariation;
                fixed4 triangleColor = _BaseColor + fixed4(colorVariation, colorVariation * 0.5, -colorVariation * 0.2, 0);
                
                // Добавляем пену на гребнях волн
                float foam = saturate(i.waveOffset * _FoamIntensity * 10.0);
                foam *= i.triangleHash; // Разная интенсивность пены для разных треугольников
                
                triangleColor.rgb += foam * 0.3;
                
                return triangleColor;
            }
            ENDCG
        }
    }
}