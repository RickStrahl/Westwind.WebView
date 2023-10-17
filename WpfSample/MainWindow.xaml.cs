using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowModel Model = new MainWindowModel();

        public MainWindow()
        {
            InitializeComponent();
            
            ThemeOverride.SetThemeWindowOverride(this, "Dark");
            DataContext = Model;
        }
        

        private void BtnEmoji_OnClick(object sender, RoutedEventArgs e)
        {
            Model.EmojiForm  = new EmojiWindow();
            Model.EmojiForm.Owner = this;
            Model.EmojiForm.SearchText = Model.EmojiSearchText;

            var result = Model.EmojiForm.ShowDialog();

            if (result == true)
            {
                MessageBox.Show(this, Model.EmojiForm.EmojiString, "Emoji Picker Selection", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }


        private void BtnSimple_OnClick(object sender, RoutedEventArgs e)
        {
            Model.BasicInteropForm = new BasicInterop();
            Model.BasicInteropForm.Owner = this;
            Model.BasicInteropForm.Show();
        }


    }




    public class MainWindowModel : INotifyPropertyChanged
    {
        public EmojiWindow EmojiForm { get; set; }
        
        public BasicInterop BasicInteropForm { get; set; }

        public string EmojiSearchText
        {
            get => _emojiSearchText;
            set
            {
                if (value == _emojiSearchText) return;
                _emojiSearchText = value;
                OnPropertyChanged();
            }
        }
        private string _emojiSearchText;


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
