using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using Newtonsoft.Json;
using Reprocessor.Helpers;
using Reprocessor.Models;

namespace Reprocessor
{
    internal class MainWindowViewModel : ViewModelBase
    {
        SdeInteraction _sde = new SdeInteraction();
        PriceCache prices = new PriceCache();

        private readonly int theForge = 10000002;

        private string _cheapestAvailable;
        private string _reprocessTotalSell;
        private EveItem _selectedItem;
        private string _userInput;

        [JsonIgnore]
        public EveItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public string CheapestAvailable
        {
            get { return _cheapestAvailable; }
            set
            {
                if (value == _cheapestAvailable) return;
                _cheapestAvailable = value;
                OnPropertyChanged();
            }
        }

        public string ReprocessTotalSell
        {
            get { return _reprocessTotalSell; }
            set
            {
                if (value == _reprocessTotalSell) return;
                _reprocessTotalSell = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<EveItem> EveItems {  get; set; }

        public string UserInput
        {
            get { return _userInput; }
            set
            {
                if (value == _userInput) return;
                _userInput = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand => new RelayCommand(x =>
        {
            if (EveItems == null)
            {
                EveItems = new ObservableCollection<EveItem>();
            }

            EveItems.Clear();
            foreach (var item in _sde.GetItems(UserInput))
            {
                EveItems.Add(item);
            }
            
        }, x => UserInput != null);

        public ICommand ReprocessCommand => new RelayCommand(HandleReprocess,x => SelectedItem != null);

        private async void HandleReprocess(object parameter)
        {
            var item = parameter as EveItem;
            item.UnitCost = await GetReprocessItemCost(item);
            var data = await GetReprocessData(item);
            var meltedPrice = await Task.Run(() => GetReprocessTotal(data));

            item.ReprocessCost = meltedPrice;
        }

        public ICommand EvaluateCommand => new RelayCommand(x => EvaluateAll(), x => EveItems.Any());

        private void EvaluateAll()
        {
            foreach (var eveItem in EveItems)
            {
                HandleReprocess(eveItem);
            }
        }

        public async Task<string> GetReprocessItemCost(EveItem item)
        {
            var price = "No items for sale";

            await Task.Run(() =>
            {
                try
                {
                    var typeId = item.TypeId;
                    if (string.IsNullOrEmpty(typeId))
                    {
                        //return;
                    }

                    var reprocessItemId = int.Parse(typeId);

                    var portion = _sde.GetPortionSize(reprocessItemId);

                    Debug.WriteLine("Reprocessing: " + item.TypeName);
                    Debug.WriteLine("Needed units: " + portion);

                    prices.GetPrices(reprocessItemId);
                    price = (portion * prices.Cache[reprocessItemId]).ToString("C");
                }
                catch (NoMarketOrdersException e)
                {
                    Console.WriteLine(item.TypeId);
                }
            });

            return price;
        }

        public async Task<List<Tuple<int, int>>> GetReprocessData(EveItem item)
        {
            var reprocessedData = new List<Tuple<int, int>>();
            await Task.Run(() =>
            {
                var reprocessData = _sde.GetReprocessDetails(int.Parse(item.TypeId));

                //Todo: Migrate from using a datatable
                foreach (DataRow data in reprocessData.Rows)
                {
                    var reprocessedMaterialId = data["materialTypeId"].ToString();
                    var quantity = int.Parse(data["quantity"].ToString());
                    var typeName = _sde.GetItemNameFromTypeId(reprocessedMaterialId);

                    //We have 50% refining efficiency
                    if (quantity % 2 != 0)
                        quantity--;

                    quantity = quantity / 2;

                    if (quantity == 0)
                    {
                        continue;
                    }

                    Debug.WriteLine(typeName + " x " + quantity);

                    reprocessedData.Add(new Tuple<int, int>(int.Parse(reprocessedMaterialId), quantity));
                }

                if (reprocessedData.Any() == false)
                {
                    item.ReprocessCost = "Unable to find reprocess information";
                }

            });
            
            return reprocessedData;
        }

        public string GetReprocessTotal(IEnumerable<Tuple<int,int>> reprocessedData)
        {
            var itemIdsForPriceCheck = reprocessedData.Select(itemId => itemId.Item1).ToList();
            prices.GetPrices(itemIdsForPriceCheck);


            var reprocessedTotal = 0d;
            Debug.WriteLine("Prices:");
            foreach (var rerocessItem in reprocessedData)
            {
                var priceInformation = prices.Cache.First(x => x.Key == rerocessItem.Item1);
                var actualQuantity = rerocessItem.Item2;
                var cheapestBuy = priceInformation.Value;

                var itemReprodValue = actualQuantity*cheapestBuy;
                var humanReadableItem = _sde.GetItemNameFromTypeId(rerocessItem.Item1.ToString());
                Debug.WriteLine($"{humanReadableItem} x {actualQuantity} at {cheapestBuy} = value of: {itemReprodValue:C}");
                reprocessedTotal = reprocessedTotal + (float) itemReprodValue;
            }

            return reprocessedTotal.ToString("C");
        }
    }
}