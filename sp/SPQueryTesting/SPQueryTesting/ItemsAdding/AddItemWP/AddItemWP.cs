using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System.Diagnostics;
using System.IO;

namespace SPQueryTesting.AddItemWP
{
    [ToolboxItemAttribute(false)]
    public class AddItemWP : WebPart
    {
        protected override void CreateChildControls()
        {
            var web = SPContext.Current.Web;
            try
            {
                web.AllowUnsafeUpdates = true;

                var list = web.GetList("/Lists/Test3");

                var stopw = new Stopwatch();

                stopw.Start();
                for (int i = 0; i < 500; i++)
                {
                    var item = list.AddItem();
                    item[SPBuiltInFieldId.Title] = string.Format("T-[{0}]", i);
                    item.Update();
                }
                stopw.Stop();

                using (StreamWriter wr = new StreamWriter(File.Open("C:\\AddItem.txt", FileMode.Append)))
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
