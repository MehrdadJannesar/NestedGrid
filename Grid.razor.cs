using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Riton.Context.Blaziton.Components.Grids.Events;
using Riton.Context.Blaziton.Components.Grids.Models;
using Riton.Context.Blaziton.Components.Paginations;
using Zamin.Extensions.Translations.Abstractions;

namespace Riton.Context.Blaziton.Components.Grids;

public partial class Grid<TItem>
{
    #region Properties
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public ITranslator Translator { get; set; } = default!;
    [Parameter] public bool DisableTranslator { get; set; }

    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public Action? ParrentStateHasChanged { get; set; }

    [Parameter] public Func<TItem, string>? RowClass { get; set; }
    [Parameter] public bool DisableRowNumber { get; set; }
    [Parameter] public EventCallback<GridRowEventArgs<TItem>> OnRowClick { get; set; }
    [Parameter] public EventCallback<GridRowEventArgs<TItem>> OnRowDoubleClick { get; set; }
    [Parameter] public EventCallback<GridRowEventArgs<TItem>> OnRowContextMenuClick { get; set; }
    private List<GridColumn<TItem>> Columns { get; set; } = [];
    private GridOperationColumn<TItem>? OperationColumn { get; set; }

    [Parameter] public List<TItem>? Data { get; set; } = [];
    [Parameter] public Func<GridQuery<TItem>, Task<GridResult<TItem>>>? DataProvider { get; set; } = null;
    [Parameter] public bool FetchDataOnInitialized { get; set; } = true;
    [Parameter] public string? FetchingDataText { get; set; }
    [Parameter] public string? EmptyDataText { get; set; }
    public bool FetchingData { get; private set; } = false;
    public List<TItem> CurrentData { get; private set; } = [];

    [Parameter] public int PageSize { get; set; } = 10;
    [Parameter] public bool DisablePagination { get; set; }
    private Pagination Pagination { get; set; } = default!;
    public int CurrentPageNumber { get; private set; } = 1;
    public int CurrentPageSize { get; private set; }
    public int CurrentTotalCount { get; private set; }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        CurrentData = Data ?? [];
        CurrentTotalCount = Data?.Count ?? 0;
        CurrentPageSize = PageSize;

        if (FetchDataOnInitialized)
        {
            await FetchDataAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JSRuntime.InvokeVoidAsync("window.initializeFlowbite");

        if (firstRender)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    #endregion

    #region Pagination Methods
    public void SetPageNumber(int pageNumber)
    {
        CurrentPageNumber = pageNumber;
        if (CurrentPageNumber < 1)
        {
            throw new ArgumentOutOfRangeException("PageNumber cound not bet less than 1");
        }
    }

    public void SetPageSize(int pageSize)
    {
        CurrentPageSize = pageSize;
        if (CurrentPageSize < 1)
        {
            throw new ArgumentOutOfRangeException("PageSize cound not bet less than 1");
        }
    }

    public void ResetPagination()
    {
        CurrentPageNumber = 1;
        CurrentPageSize = PageSize;
    }

    public void UpdatePageNumberAfterDelete(int deletedCount = 1)
    {
        if (deletedCount < 1)
        {
            throw new ArgumentOutOfRangeException("DeletedCount cound not bet less than 1");
        }

        double totalCountAfterDelete = CurrentTotalCount - deletedCount;

        if (CurrentPageNumber == Pagination.LastPageNumber)
        {
            if (totalCountAfterDelete > 0)
            {
                CurrentPageNumber = Convert.ToInt32(Math.Ceiling(totalCountAfterDelete / CurrentPageSize));

            }
            else if (totalCountAfterDelete == 0)
            {
                CurrentPageNumber = 1;
            }
            else
            {
                throw new ArgumentOutOfRangeException("invalid deleted Items Count");
            }
        }
    }

    public void UpdatePageNumberAfterCreate(int createdCount = 1)
    {
        if (createdCount < 1)
        {
            throw new ArgumentOutOfRangeException("CreatedCount cound not bet less than 1");
        }

        double totalCountAfterCreate = CurrentTotalCount + createdCount;

        CurrentPageNumber = Convert.ToInt32(Math.Ceiling(totalCountAfterCreate / CurrentPageSize));
    }

    private async Task PageNumberChangedHandler(int pageNumber)
    {
        CurrentPageNumber = pageNumber;

        await FetchDataAsync();
    }
    #endregion

    #region Content Method
    public async Task FetchDataAsync()
    {
        if (DataProvider is not null)
        {
            FetchingData = true;

            ParrentStateHasChanged?.Invoke();

            var query = new GridQuery<TItem>()
            {
                PageNumber = CurrentPageNumber,
                PageSize = CurrentPageSize
            };

            var queryResult = await DataProvider.Invoke(query);

            CurrentData = [.. queryResult.Data];
            CurrentTotalCount = queryResult.TotalCount;

            FetchingData = false;

            ParrentStateHasChanged?.Invoke();
        }
    }

    public void EmptyData()
    {
        CurrentData = [];
        CurrentTotalCount = 0;

        ResetPagination();
    }

    public void AddColumn(GridColumn<TItem> column)
    {
        Columns.Add(column);
    }

    public void AddOperationColumn(GridOperationColumn<TItem> column)
    {
        OperationColumn = column;
    }

    private async Task OnRowClickHandler(TItem item, MouseEventArgs args)
    {
        if (OnRowClick.HasDelegate)
        {
            await OnRowClick.InvokeAsync(new GridRowEventArgs<TItem>(item, args));
        }
    }

    private async Task OnRowDoubleClickHandler(TItem item, MouseEventArgs args)
    {
        if (OnRowDoubleClick.HasDelegate)
        {
            await OnRowDoubleClick.InvokeAsync(new GridRowEventArgs<TItem>(item, args));
        }
    }

    private async Task OnRowContextMenuClickHandler(TItem item, MouseEventArgs args)
    {
        if (OnRowContextMenuClick.HasDelegate)
        {
            await OnRowContextMenuClick.InvokeAsync(new GridRowEventArgs<TItem>(item, args));
        }
    }
    #endregion
}