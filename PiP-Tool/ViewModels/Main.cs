﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Helpers.Native;
using PiP_Tool.DataModel;
using PiP_Tool.Interfaces;
using PiP_Tool.Views;

namespace PiP_Tool.ViewModels
{
    public class Main : ViewModelBase, ICloseable
    {

        #region public

        public event EventHandler<EventArgs> RequestClose;

        public ICommand StartPipCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand ClosingCommand { get; }

        public WindowInfo SelectedWindowInfo
        {
            get => _selectedWindowInfo;
            set
            {
                if (_selectedWindowInfo == value)
                    return;
                _selectedWindowInfo = value;
                ShowSelector();
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<WindowInfo> WindowsList
        {
            get => _windowsList;
            set
            {
                _windowsList = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region private

        private ObservableCollection<WindowInfo> _windowsList;
        private SelectorWindow _selectorWindow;
        private WindowInfo _selectedWindowInfo;
        private readonly ProcessList _processList;

        #endregion

        public Main()
        {
            StartPipCommand = new RelayCommand(StartPipCommandExecute);
            LoadedCommand = new RelayCommand(LoadedCommandExecute);
            ClosingCommand = new RelayCommand(ClosingCommandExecute);

            WindowsList = new ObservableCollection<WindowInfo>();

            _processList = new ProcessList();
            _processList.OpenWindowsChanged += OpenWindowsChanged;
            UpdateWindowsList();
        }

        private void UpdateWindowsList()
        {
            var openWindows = _processList.OpenWindows;

            var toAdd = openWindows.Where(x => WindowsList.All(y => x.Handle != y.Handle));
            var toRemove = WindowsList.Where(x => openWindows.All(y => x.Handle != y.Handle));

            foreach (var e in toAdd)
            {
                WindowsList.Add(e);
            }
            foreach (var e in toRemove)
            {
                WindowsList.Remove(e);
            }
        }

        private void ShowSelector()
        {
            _selectorWindow?.Close();
            _selectorWindow = new SelectorWindow();
            MessengerInstance.Send(SelectedWindowInfo);
            _selectorWindow.Show();
            _selectorWindow.Activate();
        }

        private void StartPip(NativeStructs.Rect selectedRegion)
        {
            var pip = new PictureInPictureWindow();
            MessengerInstance.Send(new SelectedWindow(SelectedWindowInfo, selectedRegion));
            pip.Show();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void OpenWindowsChanged(object sender, EventArgs e)
        {
            UpdateWindowsList();
        }

        #region commands

        private void StartPipCommandExecute()
        {
            MessengerInstance.Send<Action<NativeStructs.Rect>>(StartPip);
        }

        private void LoadedCommandExecute()
        {
            
            var thisHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            //_processList.ExcludedProcesses.Add(thisHandle);
        }

        private void ClosingCommandExecute()
        {
            _processList.Dispose();
            _selectorWindow?.Close();
        }

        #endregion

    }
}
