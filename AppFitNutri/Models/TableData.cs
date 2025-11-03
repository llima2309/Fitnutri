using System.Collections.ObjectModel;

namespace AppFitNutri.Models;

public class TableData
{
    public ObservableCollection<string> Headers { get; set; } = new();
    public ObservableCollection<TableRow> Rows { get; set; } = new();
}