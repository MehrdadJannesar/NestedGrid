using Microsoft.AspNetCore.Components.Web;

namespace Riton.Context.Blaziton.Components.Grids.Events;

public class GridRowEventArgs<TItem>(TItem item, MouseEventArgs args) : EventArgs
{
    public TItem Item { get; set; } = item;
    public MouseEventArgs Args { get; set; } = args;
}