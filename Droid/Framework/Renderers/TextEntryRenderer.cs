using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BodyReportMobile.Presenter.Framework.Controls.TextEntry), typeof(BodyReport.Droid.Framework.Renderers.TextEntryRenderer))]
namespace BodyReport.Droid.Framework.Renderers
{
    public class TextEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.InputType = Control.InputType | Android.Text.InputTypes.TextFlagNoSuggestions;
            }
        }
    }
}