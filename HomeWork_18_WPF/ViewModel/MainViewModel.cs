using HomeWork_18_WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using System.Windows.Input;
using System.Windows;
using HomeWork_18_WPF.Exceptions;
using HomeWork_15_WPF.Services;
using HomeWork_15_WPF;
using System.Data.Entity;
using System.Linq;

namespace HomeWork_18_WPF.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Названия департаментов банка
        /// </summary>
        public ObservableCollection<string> departments { get; set; } = new ObservableCollection<string>()
            {Const.personalName, Const.businessName, Const.VIPName};
        /// <summary>
        /// Выбранный клинт в списке
        /// </summary>
        public static Client SelectedClient { get; set; }
        /// <summary>
        /// выбранный департамент в списке
        /// </summary>
        public static string SelectedDep { get; set; }
        static bool isLoad = false;

        /// <summary>
        /// Id выбранного клиента
        /// </summary>
        public static int SelectedClientID { get; set; }
        /// <summary>
        /// Имя выбранного клиента
        /// </summary>
        public string SelectClientName { get; set; }
        /// <summary>
        /// Сумма на счёте выбранного клиента
        /// </summary>
        public int SelectClientMoney { get; set; }
        /// <summary>
        /// Тип клиента
        /// </summary>
        public string SelectClientType { get; set; }
        /// <summary>
        /// Вклад клиента
        /// </summary>
        public string SelectClientDeposit { get; set; }
        /// <summary>
        /// Ставка по вкладу
        /// </summary>
        public double? SelectClientInterestRate { get; set; }
        /// <summary>
        /// Дата открытия
        /// </summary>
        public string SelectClientDataBegin { get; set; }
        /// <summary>
        /// На срок в днях
        /// </summary>
        public int? SelectClientDays { get; set; }

        /// <summary>
        /// EF DbContext
        /// </summary>
        static BankModel context;
        /// <summary>
        /// Список клиентов для ListView 
        /// </summary>
        public static ObservableCollection<Client> clientsList { get; set; }

        public MainViewModel()
        {
            if (!isLoad)
            {
                context = new BankModel();
                context.Clients.Load();
                context.Departments.Load();
                clientsList = context.Clients.Local;
                isLoad = true;
            }
        }
        

        /// <summary>
        /// Команда "Выбор департамента в ListBox"
        /// </summary>
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    int id = 0;
                    string objStr = obj as string;
                    foreach (var item in context.Departments)
                    {
                        if (item.Name == objStr)
                        {
                            id = item.Id;
                            break;
                        }
                    }
                    var clients1 = context.Clients.Where(e => e.Department == id);
                    clientsList.Clear();
                    foreach (var item in clients1)
                    {
                        if (!clientsList.Contains(item))
                            clientsList.Add(item);
                    }
                });
                return a;
            }
        }

        /// <summary>
        /// Команда "Выбор клинта в ListView"
        /// </summary>
        public ICommand LVSelectedItemChangedCommand
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    Client client = obj as Client;
                    if (client != null)
                    {
                        SelectedClientID = client.Id;
                        SelectClientName = client.Name;
                        SelectClientMoney = client.Money;
                        switch ((int)client.Department)
                        {
                            case 1:
                                SelectClientType = "Физ. лицо";
                                break;
                            case 2:
                                SelectClientType = "Юр. лицо";
                                break;
                            case 3:
                                SelectClientType = "VIP";
                                break;
                        }
                        if (client.Deposit > 0)
                        {
                            switch ((int)client.Deposit)
                            {
                                case 1:
                                    SelectClientDeposit = "вклад без капитализации %";
                                    break;
                                case 2:
                                    SelectClientDeposit = "вклад с капитализацией %";
                                    break;
                            }
                            SelectClientInterestRate = client.Rate;
                            SelectClientDataBegin = ((DateTime)client.DateOpen).ToLongDateString();
                            SelectClientDays = client.Days;
                        }
                        else
                        {
                            SelectClientInterestRate = 0;
                            SelectClientDataBegin = "";
                            SelectClientDays = 0;
                            SelectClientDeposit = "";
                        }
                    }
                });
                return a;
            }
        }

        
        #region Открыть счёт
        /// <summary>
        /// Открыть счёт
        /// </summary>
        public ICommand AddAccount_Click
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    var displayRootRegistry = (Application.Current as App).displayRootRegistry;

                    var addAccountViewModel = new AddAccountViewModel();
                    displayRootRegistry.ShowModalPresentation(addAccountViewModel);
                });
                return a;
            }
        }
        /// <summary>
        /// Возвращяет Client из диалогового окна AddClient
        /// </summary>
        /// <param name="employee"></param>
        public static void ReturnAddClient(Client client)
        {
            BankModel contextLocal = new BankModel();
            contextLocal.Clients.Load();
            contextLocal.Clients.Add(client);
            contextLocal.SaveChanges();

            int id = 0;
            switch (SelectedDep)
            {
                case "Физ. лицо":
                    id = 1;
                    break;
                case "Юр. лицо":
                    id = 2;
                    break;
                case "VIP":
                    id = 3;
                    break;
            }
            IQueryable<Client> clients1 = null;
            if (SelectedDep != null)
                clients1 = context.Clients.Where(e => e.Department == id);
            else
                clients1 = context.Clients;
            clientsList.Clear();
            foreach (var item in clients1)
            {
                if (!clientsList.Contains(item))
                    clientsList.Add(item);
            }
            Messenger.Default.Send(new MessageParam(DateTime.Now, MessageType.AddAccount, $"Открыт счёт для '{client.Name}' на сумму '{client.Money}'"));
        }
        #endregion


        #region Закрыть счёт
        /// <summary>
        /// Закрыть счёт
        /// </summary>
        public ICommand CloseAccount_Click
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    try
                    {
                        if (SelectedClient == null)
                        {
                            throw new NoSelectClientException("Не выбран клиент");
                        }
                        else
                        {
                            if (MessageBox.Show($"Закрыть счёт для   '{SelectedClient.Name}'", "Закрыть счёт", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return;
                            string SelectedClientName = SelectedClient.Name;
                            int SelectedClientMoney = SelectedClient.Money;

                            BankModel contextLocal = new BankModel();
                            contextLocal.Clients.Load();
                            foreach(var client in contextLocal.Clients)
                            {
                                if (client.Id == SelectedClient.Id)
                                    SelectedClient = client;
                            }
                            contextLocal.Clients.Remove(SelectedClient);
                            contextLocal.SaveChanges();

                            int id = 0;
                            switch (SelectedDep)
                            {
                                case "Физ. лицо":
                                    id = 1;
                                    break;
                                case "Юр. лицо":
                                    id = 2;
                                    break;
                                case "VIP":
                                    id = 3;
                                    break;
                            }
                            IQueryable<Client> clients1 = null;
                            if (SelectedDep != null)
                                clients1 = context.Clients.Where(e => e.Department == id);
                            else
                                clients1 = context.Clients;
                            clientsList.Clear();
                            foreach (var item in clients1)
                            {
                                if (!clientsList.Contains(item))
                                    clientsList.Add(item);
                            }
                            Messenger.Default.Send(new MessageParam(DateTime.Now, MessageType.CloseAccount, $"Закрыт счёт для '{SelectedClientName}' на сумму '{SelectedClientMoney}'"));
                        }
                    }
                    catch (NoSelectClientException ex)
                    {
                        MessageBox.Show(ex.Message, "Закрыть счёт");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Закрыть счёт");
                    }
                });
                return a;
            }
        }
        #endregion


        #region Перевести на другой счёт
        /// <summary>
        /// Перевести на другой счёт
        /// </summary>
        public ICommand MoveMoney_Click
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    try
                    {
                        if (SelectedClient == null)
                        {
                            throw new NoSelectClientException("Не выбран клиент для перевода");
                        }
                        else
                        {
                            var displayRootRegistry = (Application.Current as App).displayRootRegistry;

                            var moveMoneyViewModel = new MoveMoneyViewModel();
                            displayRootRegistry.ShowModalPresentation(moveMoneyViewModel);
                        }                        
                    }
                    catch (NoSelectClientException ex)
                    {
                        MessageBox.Show(ex.Message, "Перевести на другой счёт");
                    }
                });
                return a;
            }
        }
        /// <summary>
        /// Возвращяет Client из диалогового окна MoveMoney
        /// </summary>
        /// <param name="employee"></param>
        public static void ReturnMoveMoney(Dictionary<Client, int> client)
        {
            try
            {
                int moveMoney;
                Client moveClient;
                foreach (KeyValuePair<Client, int> kvp in client)
                {
                    moveClient = kvp.Key;
                    moveMoney = kvp.Value;
                    if (SelectedClient.Money >= moveMoney)
                    {
                        int moveClientMoney = moveMoney;
                        string SelectedClientName = SelectedClient.Name;
                        string moveClientName = moveClient.Name;

                        BankModel contextLocal = new BankModel();
                        contextLocal.Clients.Load();
                        foreach (var clientL in contextLocal.Clients)
                        {
                            if (clientL.Id == SelectedClient.Id)
                                SelectedClient = clientL;
                            if (clientL.Id == moveClient.Id)
                                moveClient = clientL;
                        }
                        SelectedClient.Money -= moveMoney;
                        moveClient.Money += moveMoney;
                        contextLocal.SaveChanges();

                        int id = 0;
                        switch (SelectedDep)
                        {
                            case "Физ. лицо":
                                id = 1;
                                break;
                            case "Юр. лицо":
                                id = 2;
                                break;
                            case "VIP":
                                id = 3;
                                break;
                        }
                        IQueryable<Client> clients1 = null;
                        if (SelectedDep != null)
                            clients1 = context.Clients.Where(e => e.Department == id);
                        else
                            clients1 = context.Clients;
                        clientsList.Clear();
                        foreach (var item in clients1)
                        {
                            if (!clientsList.Contains(item))
                                clientsList.Add(item);
                        }
                        Messenger.Default.Send(new MessageParam(DateTime.Now, MessageType.MoveMoney, $"Переведена сумма '{moveClientMoney}' с счёта '{SelectedClientName}' на счёт '{moveClientName}'"));
                    }
                    else
                    {
                        MessageBox.Show($"На счёту клиента {SelectedClient} недостаточно средств", "Перевести на другой счёт");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Перевести на другой счёт");
            }
        }
        #endregion


        #region Открыть вклад без капитализации %
        /// <summary>
        /// Открыть вклад без капитализации %
        /// </summary>
        public ICommand AddDepositNoCapitalize_Click
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    try
                    {
                        if (SelectedClient == null)
                        {
                            throw new NoSelectClientException("Не выбран клиент");
                        }
                        else
                        {
                            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
                            var addDepositNoCapitalizeViewModel = new AddDepositNoCapitalizeViewModel();
                            Dictionary<BankDepartment, uint> bd = new Dictionary<BankDepartment, uint>();
                            switch (SelectedClient.Department)
                            {
                                case 2:
                                    bd.Add(BankDepartment.BusinessDepartment, 0);
                                    Messenger.Default.Send(bd);
                                    displayRootRegistry.ShowModalPresentation(addDepositNoCapitalizeViewModel);
                                    break;
                                case 1:
                                    bd.Add(BankDepartment.PersonalDepartment, 0);
                                    Messenger.Default.Send(bd);
                                    displayRootRegistry.ShowModalPresentation(addDepositNoCapitalizeViewModel);
                                    break;
                                case 3:
                                    bd.Add(BankDepartment.VIPDepartment, 0);
                                    Messenger.Default.Send(bd);
                                    displayRootRegistry.ShowModalPresentation(addDepositNoCapitalizeViewModel);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Открыть вклад без капитализации %");
                    }
                });
                return a;
            }
        }

        /// <summary>
        /// Возвращяет Deposit из окна AddDepositNoCapitalizeWindow
        /// </summary>
        /// <param name="deposit"></param>
        public static void ReturnAddDepositNoCapitalize(Model.DepositC deposit)
        {
            // обновленние данных для текущего content
            #region обновленние данных для текущего content
            SelectedClient.DateOpen = deposit.DateBegin;
            SelectedClient.Deposit = 1;
            SelectedClient.Days = deposit.Days;
            SelectedClient.Rate = deposit.InterestRate;
            #endregion

            BankModel contextLocal = new BankModel();
            contextLocal.Clients.Load();
            foreach (var clientL in contextLocal.Clients)
            {
                if (clientL.Id == SelectedClient.Id)
                    SelectedClient = clientL;
            }
            SelectedClient.DateOpen = deposit.DateBegin;
            SelectedClient.Deposit = 1;
            SelectedClient.Days = deposit.Days;
            SelectedClient.Rate = deposit.InterestRate;
            contextLocal.SaveChanges();
            Messenger.Default.Send(new MessageParam(DateTime.Now, MessageType.AddDepositNoCapitalize, $"Открыт вклад без капитализации % для '{SelectedClient.Name}'"));
        }
        #endregion


        #region Открыть вклад с капитализацией %
        /// <summary>
        /// Открыть вклад с капитализацией %
        /// </summary>
        public ICommand AddDepositCapitalize_Click
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    try
                    {
                        if (SelectedClient == null)
                        {
                            throw new NoSelectClientException("Не выбран клиент");
                        }
                        else
                        {
                            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
                            var addDepositCapitalizeViewModel = new AddDepositCapitalizeViewModel();
                            switch (SelectedClient.Department)
                            {
                                case 2:
                                    Messenger.Default.Send(BankDepartment.BusinessDepartment);
                                    displayRootRegistry.ShowModalPresentation(addDepositCapitalizeViewModel);
                                    break;
                                case 1:
                                    Messenger.Default.Send(BankDepartment.PersonalDepartment);
                                    displayRootRegistry.ShowModalPresentation(addDepositCapitalizeViewModel);
                                    break;
                                case 3:
                                    Messenger.Default.Send(BankDepartment.VIPDepartment);
                                    displayRootRegistry.ShowModalPresentation(addDepositCapitalizeViewModel);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Открыть вклад с капитализацией %");
                    }
                });
                return a;
            }
        }

        /// <summary>
        /// Возвращяет Deposit из окна AddDepositNoCapitalizeWindow
        /// </summary>
        /// <param name="deposit"></param>
        public static void ReturnAddDepositCapitalize(Model.DepositC deposit)
        {
            // обновленние данных для текущего content
            #region обновленние данных для текущего content
            SelectedClient.DateOpen = deposit.DateBegin;
            SelectedClient.Deposit = 2;
            SelectedClient.Days = deposit.Days;
            SelectedClient.Rate = deposit.InterestRate;
            #endregion

            BankModel contextLocal = new BankModel();
            contextLocal.Clients.Load();
            foreach (var clientL in contextLocal.Clients)
            {
                if (clientL.Id == SelectedClient.Id)
                    SelectedClient = clientL;
            }
            SelectedClient.DateOpen = deposit.DateBegin;
            SelectedClient.Deposit = 2;
            SelectedClient.Days = deposit.Days;
            SelectedClient.Rate = deposit.InterestRate;
            contextLocal.SaveChanges();
            Messenger.Default.Send(new MessageParam(DateTime.Now, MessageType.AddDepositCapitalize, $"Открыт вклад c капитализацией % для '{SelectedClient.Name}'"));
        }
        #endregion


        #region Показать окно с расчётом %
        /// <summary>
        /// Показать окно с расчётом %
        /// </summary>
        public ICommand RateView_Click
        {
            get
            {
                var a = new DelegateCommand((obj) =>
                {
                    try 
                    { 
                        if (SelectedClient == null)
                        {
                            throw new NoSelectClientException("Не выбран клиент");
                        }
                        else
                        { 
                            if (SelectedClient.Deposit > 0)
                            {
                                var displayRootRegistry = (Application.Current as App).displayRootRegistry;
                                var rateViewModel = new RateViewModel();
                                Dictionary<Client, short> client = new Dictionary<Client, short>();
                                client.Add(SelectedClient, 0);
                                Messenger.Default.Send(client);
                                displayRootRegistry.ShowModalPresentation(rateViewModel);
                                Messenger.Default.Send(new MessageParam(DateTime.Now, MessageType.RateView, $"Показано окно с расчётом % для '{SelectedClient.Name}'"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Перевести на другой счёт");
                    }
                });
                return a;
            }
        }
        #endregion


        #region Создать Log
        /// <summary>
        /// Создать Log
        /// </summary>
        public ICommand CreateLog_Click
        {
            get
            {
                return new DelegateCommand((obj) => SaveMessages.Save());               
            }
        }
        #endregion

        #region Загрузить Log
        /// <summary>
        /// Загрузить Log
        /// </summary>
        public ICommand LoadLog_Click
        {
            get
            {
                return new DelegateCommand(async (obj) => await SaveMessages.Load());                 
            }
        }
        #endregion

    }
}
