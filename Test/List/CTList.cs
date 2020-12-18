using SPExtention;

namespace Test.List
{
    [InternalName("TestCT"), DisplayName("TestCT"), ContentTypeId("0x010019DE394A3F40422A87F60769C59E3CED")]
    public class CTListProps1 : ListPropsFromAttributes<CTListProps1> { }

    public class CTListProps2 : ListProps 
    {
        public override string InternalName => "TestCT";
        public override string DisplayName => "TestCT";
        public override string ContentTypeId => "0x010019DE394A3F40422A87F60769C59E3CED";
    }

    class CTList : SPListWrapper<CTList, CTListProps2, DefaultListItem> { }
}
