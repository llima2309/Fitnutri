namespace AppFitNutri.Models;

public class TableRow
{
    public string Exercise { get; set; } = string.Empty;
    public int Advanced { get; set; }
    public int Intermediate { get; set; }
    public int Beginner { get; set; }
    public string? Note { get; set; }
}


