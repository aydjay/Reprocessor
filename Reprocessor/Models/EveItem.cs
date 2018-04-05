using System.ComponentModel;
using System.Runtime.CompilerServices;
using Reprocessor.Annotations;

namespace Reprocessor.Models
{
    public class EveItem : INotifyPropertyChanged
    {
        private string _typeId;
        private string _typeName;
        private string _unitCost;
        private string _reprocessCost;

        public string TypeId
        {
            get { return _typeId; }
            set { _typeId = value; OnPropertyChanged();}
        }

        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; OnPropertyChanged();}
        }

        public string UnitCost
        {
            get { return _unitCost; }
            set { _unitCost = value; OnPropertyChanged();}
        }

        public string ReprocessCost
        {
            get { return _reprocessCost; }
            set { _reprocessCost = value; OnPropertyChanged();}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}