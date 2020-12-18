using Microsoft.SharePoint;
using SPExtention;

namespace Test.List
{
    class TestSpListItem: BaseListItem
    {
        [DisplayName("Picture URL")]
        [FieldType(SPFieldType.Text)]
        [DefaultView]
        public string PictureUrl { get; private set; }

        [DisplayName("TEst Link URL")]
        [FieldType(SPFieldType.Text)]
        [DefaultView]
        public string LinkUrl { get; private set; }

        [DisplayName("Picture 123")]
        [FieldType(SPFieldType.Text)]
        [DefaultView]
        public string Picture { get; private set; }

        [DisplayName("Picture")]
        [FieldType(SPFieldType.Note)]
        [AdditionalFieldAttr("NumLines", "4")]
        public string Picture1 { get; private set; }

        [DisplayName("Tester")]
        [FieldType(SPFieldType.Integer)]
        [DefaultView]
        public string Tester { get; private set; }
    }

    [InternalName("PopRequestsList123"), DisplayName("Popular requests 123")]
    class TestSpListProps : ListPropsFromAttributes<TestSpListProps> { }

    class TestSpList : SPListWrapper<TestSpList, TestSpListProps, TestSpListItem> { }

}
