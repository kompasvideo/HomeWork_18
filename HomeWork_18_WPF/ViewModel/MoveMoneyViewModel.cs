using HomeWork_18_WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HomeWork_18_WPF.ViewModel
{
    class MoveMoneyViewModel
    {
        /// <summary>
        /// Список клиентов для ListView 
        /// </summary>
        public static ObservableCollection<Client> clientsList { get; set; }
        /// <summary>
        /// Выбранный клинт в списке
        /// </summary>
        public Client SelectedClient { get; set; }
        /// <summary>
        /// Сумма перевода
        /// </summary>
        public int MoneyMove { get; set; }
        /// <summary>
        /// EF DbContext
        /// </summary>
        static BankModel context;
        static bool isLoad = false;


        public MoveMoneyViewModel()
        {
            if (!isLoad)
            {
                context = new BankModel();
                context.Clients.Load();
                clientsList = context.Clients.Local;
                isLoad = true;
            }
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
                    if (SelectedClient != null)
                    {
                        Dictionary<Client, int> client = new Dictionary<Client, int>();
                        client.Add(SelectedClient, MoneyMove);
                        Messenger.Default.Send(client);
                        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window.Title == "Перевести между счетами")
                            {
                                window.Close();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не выбран счёт для перевода", "Перевести на другой счёт");
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
                        if (window.Title == "Перевести между счетами")
                        {
                            window.Close();
                        }
                    }
                });
            }
        }
    }
}
