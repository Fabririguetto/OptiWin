using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OptiWin.Core.Tweaks
{
    public enum TweakCategory
    {
        Rendimiento,
        Privacidad,
        Sistema,
        Red
    }

    public class TweakModel : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private bool _isBusy;

        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DetailedExplanation { get; set; } = "";
        public string RegistryOrServiceDetails { get; set; } = "";
        public TweakCategory Category { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public Func<bool>? CheckAction { get; set; }
        public Action<bool>? ApplyAction { get; set; }

        public void RefreshStatus()
        {
            if (CheckAction != null)
            {
                _isEnabled = CheckAction();
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
