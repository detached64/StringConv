using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using StringConv.ViewModels;
using System;
using System.Threading.Tasks;

namespace StringConv.Services;

internal sealed class ShowDialogService(IServiceProvider serviceProvider) : IShowDialogService
{
    public async Task ShowDialogAsync<TView, TViewModel>(Window parent) where TView : Window where TViewModel : ViewModelBase
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = serviceProvider.GetService<TViewModel>();
        await view.ShowDialog(parent);
    }
}
