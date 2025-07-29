using Microsoft.Maui.Controls;
using BRB5.Model;
using BRB6.Template;

namespace BRB6.Template
{
    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HeadTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var question = item as BRB5.Model.RaitingDocItem;
            if (question == null)
                return null;

            return question.IsHead
                ? HeadTemplate
                : ItemTemplate;
        }
    }
}
