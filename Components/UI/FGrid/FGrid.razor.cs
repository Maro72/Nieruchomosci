using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.UI
{
    public partial class FGrid<TItem> : ComponentBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Parameter] public string Title { get; set; } = "Lista";
        [Parameter] public IEnumerable<TItem> Items { get; set; } = Array.Empty<TItem>();
        [Parameter] public TItem? SelectedItem { get; set; }
        [Parameter] public EventCallback<TItem?> SelectedItemChanged { get; set; }
        [Parameter] public int GridKey { get; set; } = 0;
        [Parameter] public string SearchText { get; set; } = "";
        [Parameter] public EventCallback<string> SearchTextChanged { get; set; }

        // Wstrzykiwane fragmenty drzewa renderowania (RenderFragment)
        [Parameter] public RenderFragment? GridColumns { get; set; }
        [Parameter] public RenderFragment? ToolbarLeftContent { get; set; }
        [Parameter] public RenderFragment? ExtraActions { get; set; }
        [Parameter] public EventCallback OnExportToExcel { get; set; }
        // Delegaty powiadomień dla akcji CRUD
        [Parameter] public EventCallback OnAdd { get; set; }
        [Parameter] public EventCallback<TItem> OnEdit { get; set; }
        [Parameter] public EventCallback<TItem> OnDelete { get; set; }
        [Parameter] public bool ShowToolbar { get; set; } = true;
        [Parameter] public bool ShowPrint { get; set; } = true;
        [Parameter] public bool ShowSearch { get; set; } = true;
        protected async Task HandleSearch(ChangeEventArgs e)
        {
            SearchText = e.Value?.ToString() ?? "";
            await SearchTextChanged.InvokeAsync(SearchText);
        }

        protected string OnRowClassFunc(TItem item, int rowNumber)
        {
            if (SelectedItem != null && SelectedItem.Equals(item))
            {
                return "selected-row-highlight";
            }
            return "";
        }
        private async Task ExportToExcelInternal()
        {
            if (OnExportToExcel.HasDelegate)
            {
                // Wywołujemy metodę zdefiniowaną na konkretnej stronie (np. w FBudynki)
                await OnExportToExcel.InvokeAsync();
            }

        }
        protected async Task WykonajEksportExcel()
        {
            if (OnExportToExcel.HasDelegate)
            {
                // Wymuszamy asynchroniczne, bezpieczne dla wątków Blazora uruchomienie metody EksportujDoExcela()
                await OnExportToExcel.InvokeAsync(null);
            }
        }
        protected async Task DrukujWidok()
        {
            // Wywołuje natywne okno drukowania przeglądarki (Ctrl + P)
            await JSRuntime.InvokeVoidAsync("window.print");
        }
        private async Task HandleRowClick(DataGridRowClickEventArgs<TItem> args)
        {
            SelectedItem = args.Item;
            // Wymuszamy przerysowanie żeby przycisk Edytuj się odblokował
            StateHasChanged();
        }
    }
}