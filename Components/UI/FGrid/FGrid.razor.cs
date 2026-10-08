using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        [Parameter] public bool ShowCrudActions { get; set; } = true;
        [Parameter] public bool ShowToolbar { get; set; } = true;
        [Parameter] public bool ShowPrint { get; set; } = true;
        [Parameter] public bool ShowSearch { get; set; } = true;
        [Parameter] public bool ShowPager { get; set; } = true;
        private string _searchText = "";
        private string _lastParameterSearchText = "";

        protected override void OnParametersSet()
        {
            if (!string.Equals(SearchText, _lastParameterSearchText, StringComparison.Ordinal))
            {
                _lastParameterSearchText = SearchText;
                _searchText = SearchText;
            }
        }

        protected bool MatchesSearch(TItem item)
        {
            return string.IsNullOrWhiteSpace(_searchText) ||
                   ContainsSearchValue(item, _searchText, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        private static bool ContainsSearchValue(object? value, string searchText, int depth, HashSet<object> visited)
        {
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            if (IsSearchableScalar(type))
            {
                return Convert.ToString(value, CultureInfo.CurrentCulture)?
                    .Contains(searchText, StringComparison.OrdinalIgnoreCase) == true;
            }

            if (depth >= 2)
            {
                return false;
            }

            if (!type.IsValueType && !visited.Add(value))
            {
                return false;
            }

            if (value is IEnumerable values)
            {
                foreach (var item in values)
                {
                    if (ContainsSearchValue(item, searchText, depth + 1, visited))
                    {
                        return true;
                    }
                }

                return false;
            }

            return type.GetProperties()
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .Any(property => ContainsSearchValue(property.GetValue(value), searchText, depth + 1, visited));
        }

        private static bool IsSearchableScalar(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        protected async Task HandleSearch(ChangeEventArgs e)
        {
            _searchText = e.Value?.ToString() ?? "";
            SearchText = _searchText;
            await SearchTextChanged.InvokeAsync(_searchText);
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