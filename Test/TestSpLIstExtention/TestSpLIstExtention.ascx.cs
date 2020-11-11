using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Test.List;

namespace Test.TestSpLIstExtention
{
    [ToolboxItemAttribute(false)]
    public partial class TestSpLIstExtention : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public TestSpLIstExtention()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btn_Create_OnClick(object sender, EventArgs e)
        {
            var spWeb = SPContext.Current.Web;
            TestSpList tl = new TestSpList(spWeb);
            var list = tl.CreateOrUpdate();
            lbl_Message.Text = list.Title;

            CTList.CreateOrUpdate(spWeb);
        }

        protected void btn_Update_OnClick(object sender, EventArgs e)
        {
            var r = TestSpList.GetAllListItems(SPContext.Current.Web);

            foreach(var x in r)
            {
                lbl_Message.Text += x.LinkUrl + " | ";
            }
        }
    }
}
