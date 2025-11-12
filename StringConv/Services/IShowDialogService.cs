using Avalonia.Controls;
using StringConv.ViewModels;
using System.Threading.Tasks;

namespace StringConv.Services;

internal interface IShowDialogService
{
    Task ShowDialogAsync<TView, TViewModel>(Window parent) where TView : Window where TViewModel : ViewModelBase;
}
