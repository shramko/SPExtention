using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using SPExtention;
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
            var testList = TestSpList.CreateOrUpdate(SPContext.Current.Web);
            lbl_Message.Text = testList.Title;

            var ctList = CTList.CreateOrUpdate(SPContext.Current.Web);
            var items = ctList.GetItems();

            ctList.AddItem(new DefaultListItem { 
                Title = "item-" + items.Count()
            });
        }

        protected void btn_Update_OnClick(object sender, EventArgs e)
        {
            var testList = TestSpList.GetExisting(SPContext.Current.Web);
            var items = testList.GetItems();

            foreach (var item in items)
            {
                lbl_Message.Text += item.LinkUrl + " | ";
            }
        }
    }
}
