namespace TermometroDaSala.Desktop;

public static class NivelCalorTexto
{
    /// <summary>Texto mostrado ao lado da cor, para o nível não depender só da cor.</summary>
    public static string Texto(this NivelCalor nivel) => nivel switch
    {
        NivelCalor.Normal => "Normal", NivelCalor.Atencao => "Atenção",
        NivelCalor.Cuidado => "Cuidado extremo", NivelCalor.Perigo => "Perigo", _ => "Nível inválido"
    };
}
