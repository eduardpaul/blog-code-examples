using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;
using System.Diagnostics;
using System.IO;

namespace SPQueryTesting.ItemsAddWP
{
    [ToolboxItemAttribute(false)]
    public class ItemsAddWP : WebPart
    {
        protected override void CreateChildControls()
        {
            var web = SPContext.Current.Web;
            try
            {
                web.AllowUnsafeUpdates = true;

                var list = web.GetList("/Lists/Test4");

                var stopw = new Stopwatch();

                stopw.Start();
                for (int i = 0; i < 500; i++)
                {
                    var item = list.Items.Add();
                    item[SPBuiltInFieldId.Title] = string.Format("T-[{0}]", i);
                    item.Update();
                }
                stopw.Stop();

                using (StreamWriter wr = new StreamWriter(File.Open("C:\\ItemsAdd.txt", FileMode.Append)))
                {
                    wr.WriteLine(string.Format("{0};{1}", list.ItemCount, stopw.ElapsedMilliseconds));
                }
            }
            catch
            { }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }
    }
}
