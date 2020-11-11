using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using SPExtention;

namespace Test.List
{
    [InternalName("PopRequestsList123"), DisplayName("Popular requests 123")]
    public class TestSpList : SPListExtention<TestSpList>
    {
        public TestSpList(SPWeb web)
            : base(web)
        {
        }

        public TestSpList()
        {

        }

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

    [InternalName("TestCT"), DisplayName("TestCT"), ContentTypeId("0x010019DE394A3F40422A87F60769C59E3CED")]
    public class CTList : SPListExtention<CTList>
    {
        public CTList(SPWeb web) : base(web) { }
        public CTList()
        {

        }
    }
}
