Shader "Custom/Stencil Mask"
{
    Properties
    {
       
        _StencilID ("Stencil ID" , Range(0,255)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" 
            "Queue" = "Geometry" 
                "RenderPipeline" = "UniversalPipeline"
            }
        pass{
            
            Blend Zero One
            ZWrite Off
            
            Stencil{
                Ref [_StencilID]
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
                }
        }
        
       
    }
   
}
