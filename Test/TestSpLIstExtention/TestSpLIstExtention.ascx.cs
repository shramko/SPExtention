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
            var list = tl.Create();
            lbl_Message.Text = list.Title;

            lbl_Message.Text += " " + CTList.Create(spWeb);
        }

        protected void btn_Update_OnClick(object sender, EventArgs e)
        {
            //TestSpList.UpdateFields(SPContext.Current.Web);
            //TestSpList tl = new TestSpList(SPContext.Current.Web);
            //SPList list = tl.GetSPListByInternalOrDisplayName();
            //if (list != null)
            //{
            //    var r = TestSpList.UpdateFields(SPContext.Current.Web);
            //    if(r!=null)
            //        lbl_Message.Text = (r as Exception).Message;
            //    else
            //        lbl_Message.Text = "Update successfull";
            //}

            CTList.UpdateFields(SPContext.Current.Web);
        }
    }
}
