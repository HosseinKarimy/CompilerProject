namespace CompilerProject.Tokens;

public class TokenModel
{
    public string Name { get; set; }
    public TokenType Type { get; set; }
    public TokenStatus Status { get; set; }
    public string Description { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int StartPosition { get; set; }
}
