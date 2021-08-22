using DevExpress.Mvvm;
using System.Windows.Input;
using HomeWork_18_WPF.Model;
using System.Windows;

namespace HomeWork_18_WPF.ViewModel
{
    class AddAccountViewModel : ViewModelBase
    {
        /// <summary>
        /// Выбранный департамент в списке
        /// </summary>
        public static string SelectedDep { get; set; }
        /// <summary>
        /// Имя клиента
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Сумма на счёте
        /// </summary>
        public int Money { get; set; }
        public AddAccountViewModel()
        {
        }
        public AddAccountViewModel(string Name, int Money)
        {
            this.Name = Name;
            this.Money = Money;
        }
        /// <summary>
        /// Нажата кнопка "Ок"
        /// </summary>
        public ICommand bOK_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (SelectedDep == null)
                    {
                        MessageBox.Show("Не выбран тип счёта", "Ошибка");
                    }
                    else
                    {
                        Client client;
                        switch (SelectedDep)
                        {
                            case Const.personalName:
                                client = new Client();
                                client.Name = Name;
                                client.Money = Money;
                                client.Department = 1;
                                Messenger.Default.Send(client);
                                break;
                            case Const.businessName:
                                client = new Client();
                                client.Name = Name;
                                client.Money = Money;
                                client.Department = 2;
                                Messenger.Default.Send(client);
                                break;
                            case Const.VIPName:
                                client = new Client();
                                client.Name = Name;
                                client.Money = Money;
                                client.Department = 3;
                                Messenger.Default.Send(client);
                                break;
                        }
                        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window.Title == "Открыть счёт")
                            {
                                window.Close();
                            }
                        }
                    }
                });
            }
        }
        /// <summary>
        /// Нажата кнопка "Отмена"
        /// </summary>
        public ICommand bCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window.Title == "Открыть счёт")
                        {
                            window.Close();
                        }
                    }
                });
            }
        }
    }
}
