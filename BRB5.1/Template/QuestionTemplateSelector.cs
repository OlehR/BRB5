using BRB5.Model;
using BRB6.Template;

namespace BRB6
{
    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HeadTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var q = item as BRB5.Model.RaitingDocItem;
            return q?.IsHead == true ? HeadTemplate : ItemTemplate;
        }
    }
    public interface IHeadTapHandler
    {
        void OnHeadTapped(BRB5.Model.RaitingDocItem head);
    }

}
