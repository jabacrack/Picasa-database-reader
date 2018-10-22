using System.Collections.ObjectModel;
using System.Data;
using System.Reactive;
using DynamicData;
using DynamicData.Binding;
using PicasaDatabaseReader.Core.Interfaces;
using ReactiveUI;

namespace PicasaDatabaseReader.UI.ViewModels
{
    public interface IMainWindowViewModel: IReactiveObject
    {
        string PathToDatabase { get; set; }
        ReactiveCommand<Unit, Unit> BrowseToDatabase { get; set; }
        ObservableCollection<string> TableNames { get; set; }
        IDatabaseReader DatabaseReader { get; }
        string SelectedTable { get; set; }
        DataView DataTable { get; }
    }
}