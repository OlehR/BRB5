using BRB5.Model;
using BRB5;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BL
{
    public partial class BL
    {

        public ObservableCollection<DocWaresEx> GetDataPCP(IEnumerable<DocWares> tempInfo, DocVM Doc, int ShelfType)
        {
            int i = 0;
            foreach (var item in tempInfo)
            {
                item.TypeDoc = 13;
                item.OrderDoc = i++;
            }

            db.ReplaceDocWaresSample(tempInfo.Select(el => new DocWaresSample(el)));

            return new ObservableCollection<DocWaresEx>(db.GetDocWares(Doc, 1, eTypeOrder.Name, ShelfType));

        }
    }
}
