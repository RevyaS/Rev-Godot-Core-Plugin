using Godot;

[Tool]
public partial class BloomScreen : ColorRect
{
    ShaderMaterial Shader => (ShaderMaterial)Material;

    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float BloomStrength
    {
        get => (float)Shader.GetShaderParameter("edge_strength");
        set => Shader.SetShaderParameter("edge_strength", value);
    } 

    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float InnertBloomStrength
    {
        get => (float)Shader.GetShaderParameter("inner_strength");
        set => Shader.SetShaderParameter("inner_strength", value);
    } 

    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Radius
    {
        get => (float)Shader.GetShaderParameter("radius");
        set => Shader.SetShaderParameter("radius", value);
    } 

    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Feather
    {
        get => (float)Shader.GetShaderParameter("feather");
        set => Shader.SetShaderParameter("feather", value);
    } 
}
