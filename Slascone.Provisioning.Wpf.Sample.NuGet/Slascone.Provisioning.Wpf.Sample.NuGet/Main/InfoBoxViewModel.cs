using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Main
{
    public class InfoBoxViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Inline> _infoLines;
        private string _title;
        private readonly bool _isNewerShipmentAvailable;
        private readonly string _latestShipmentVersionNumber;
        private readonly string _latestShipmentDownloadLink;

        public InfoBoxViewModel()
        {
            _infoLines = new ObservableCollection<Inline>();
            _title = "Information";
        }

        // Modified constructor to receive only the properties we need
        public InfoBoxViewModel(bool isNewerShipmentAvailable, string latestShipmentVersionNumber, string latestShipmentDownloadLink) : this()
        {
            _isNewerShipmentAvailable = isNewerShipmentAvailable;
            _latestShipmentVersionNumber = latestShipmentVersionNumber;
            _latestShipmentDownloadLink = latestShipmentDownloadLink;
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Inline> InfoLines
        {
            get => _infoLines;
            set
            {
                _infoLines = value;
                OnPropertyChanged();
            }
        }

        public void LoadAboutInformation()
        {
            Title = "About";
            
            var infoLines = new List<Inline>
            {
                // Add product information
                new Run("SLASCONE Demo Client Application") { FontWeight = FontWeights.Bold },
                new LineBreak(),
                new Run($"Version {Assembly.GetAssembly(typeof(MainWindow)).GetName().Version}"),
                new LineBreak()
            };

            // Add update information if available
            if (_isNewerShipmentAvailable)
            {
                infoLines.Add(new LineBreak());
                infoLines.Add(new Run($"Update available: {_latestShipmentVersionNumber}"));
                
                if (!string.IsNullOrEmpty(_latestShipmentDownloadLink))
                {
                    var hyperlink = new Hyperlink(new Run(_latestShipmentDownloadLink))
                    {
                        NavigateUri = new Uri(_latestShipmentDownloadLink)
                    };

                    hyperlink.RequestNavigate += (sender, args) =>
                    {
                        Process.Start(new ProcessStartInfo(hyperlink.NavigateUri.ToString())
                        {
                            UseShellExecute = true
                        });
                    };

                    infoLines.Add(new LineBreak());
                    infoLines.Add(new Run("Download here: "));
                    infoLines.Add(hyperlink);
                }
            }

            InfoLines = new ObservableCollection<Inline>(infoLines);
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}