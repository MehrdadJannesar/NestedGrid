namespace Riton.Context.Blaziton.Components.Grids.Models;

public class GridResult<TItem>
{
    public IReadOnlyList<TItem> Data { get; set; } = [];
    public int TotalCount { get; set; }
}