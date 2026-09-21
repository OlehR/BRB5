using BRB5.Model;
using BRB6.Template;

namespace BRB6
{
    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HeadTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate ChecklistTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var q = item as BRB5.Model.RaitingDocItem;
            if (q?.IsHead == true) return HeadTemplate;
            return q?.IsTimed == true ? ChecklistTemplate : ItemTemplate;
        }
    }
    public interface IHeadTapHandler
    {
        void OnHeadTapped(BRB5.Model.RaitingDocItem head);
    }
    public interface IRatingButtonHandler
    {
        void OnRatingButtonClicked(object sender, BRB5.Model.RaitingDocItem item);
    }
    public interface IChecklistHandler
    {
        void OnChecklistChanged(BRB5.Model.RaitingDocItem item);
    }
}
