using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for BasicInterop.xaml
    /// </summary>
    public partial class BasicInterop : MetroWindow
    {
        public BasicInterop()
        {
            InitializeComponent();
            ThemeOverride.SetThemeWindowOverride(this, "Dark");

            Model = new BasicInteropModel();
            DataContext = Model;

            Loaded += BasicInterop_Loaded;
        }

        public BasicInteropModel Model  { get; set; }

        private void BasicInterop_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Top = Owner.Top + 65;
                Left = Owner.Left + 60;
            }
        }
    }

    #region Model Data

    public class BasicInteropModel : INotifyPropertyChanged
    {
        public Person Person { get; set;  } = new Person();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }


    public class Person : INotifyPropertyChanged
    {
        private string _firstname = "Billy";
        private string _lastname = "Bopp";
        private string _email = "bopp@beebopp.com";
        private string _company = "beebob.com";
        private Address _address = new Address();

        public string Firstname
        {
            get => _firstname;
            set
            {
                if (value == _firstname) return;
                _firstname = value;
                OnPropertyChanged();
            }
        }

        public string Lastname
        {
            get => _lastname;
            set
            {
                if (value == _lastname) return;
                _lastname = value;
                OnPropertyChanged();
            }
        }

        public string Company

        {
            get => _company;
            set
            {
                if (value == _company) return;
                _company = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (value == _email) return;
                _email = value;
                OnPropertyChanged();
            }
        }

        public Address Address
        {
            get => _address;
            set
            {
                if (Equals(value, _address)) return;
                _address = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class Address : INotifyPropertyChanged
    {
        private string _street = "123 Nowher Lane";
        private string _city = "Anytown";
        private string _state = "UT";
        private string _country = "USA";
        private string _postalCode = "12314";

        public string Street
        {
            get => _street;
            set
            {
                if (value == _street) return;
                _street = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (value == _city) return;
                _city = value;
                OnPropertyChanged();
            }
        }

        public string State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public string PostalCode

        {
            get => _postalCode;
            set
            {
                if (value == _postalCode) return;
                _postalCode = value;
                OnPropertyChanged();
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (value == _country) return;
                _country = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    #endregion
}
