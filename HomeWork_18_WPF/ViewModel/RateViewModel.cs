﻿using System.Collections.Generic;
using DevExpress.Mvvm;
using System.Windows.Input;
using HomeWork_18_WPF.Model;

namespace HomeWork_18_WPF.ViewModel
{
    class RateViewModel : ViewModelBase
    {
        public static string[] MoneyRate { get; set; }

        public RateViewModel()
        {
        }
        /// <summary>
        /// Принимает параметр типа Client
        /// </summary>
        /// <param name="client"></param>
        public static void SetClient(Dictionary<HomeWork_18_WPF.Model.Client, short> client)
        {
            foreach (KeyValuePair<HomeWork_18_WPF.Model.Client, short> kvp in client)
            {
                HomeWork_18_WPF.Model.Client l_client = kvp.Key;
                //MoneyRate = l_client.DepositClient.GetSumRate(l_client.Money);
                MoneyRate = l_client.GetSumRateExt();
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
                    foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window.Title == "Расчёт %")
                        {
                            window.Close();
                        }
                    }
                });
            }
        }
    }
}
