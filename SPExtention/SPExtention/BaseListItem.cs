using Microsoft.SharePoint;

namespace SPExtention
{
    public abstract class BaseListItem
    {
        [DisplayName("ID")]
        [FieldType(SPFieldType.Text)]
        [DefaultView]
        public string ID { get; set; }

        [DisplayName("Title")]
        [FieldType(SPFieldType.Text)]
        [DefaultView]
        public string Title { get; set; }

        [DisplayName("Author")]
        [FieldType(SPFieldType.Text)]
        [DefaultView]
        public string Author { get; set; }
    }

    public class DefaultListItem: BaseListItem { }
}
