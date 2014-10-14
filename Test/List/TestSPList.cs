using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using SPExtention;

namespace Test.List
{
    [InternalName("PopRequestsList"), DisplayName("Popular requests"), ContentType("")]
    public class TestSpList : SPListExtention<TestSpList>
    {
        public TestSpList(SPWeb spWeb)
            : base(spWeb)
        {
        }

        [DisplayName("Picture URL")]
        [FieldType(SPFieldType.Text)]
        public string PictureUrl { get; private set; }

        [DisplayName("TEst Link URL")]
        [FieldType(SPFieldType.Text)]
        public string LinkUrl { get; private set; }

        [DisplayName("Picture 123")]
        [FieldType(SPFieldType.Text)]
        public string Picture { get; private set; }
        
        [DisplayName("Picture")]
        [FieldType(SPFieldType.Text)]
        public string Picture1 { get; private set; }
    }

    public class TSPList : SPListExtention<TSPList>
    {
        [DisplayName("Picture"),Required]
        [FieldType(SPFieldType.Text)]
        string Picture1 { get; set; }
    }
}
