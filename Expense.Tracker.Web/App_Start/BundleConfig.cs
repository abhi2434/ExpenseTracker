using System.Web;
using System.Web.Optimization;

namespace Expense.Tracker.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js")
                        .Include("~/Scripts/jquery.validate.js")
                        .Include("~/Scripts/jquery.ajaxprogress.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));
            bundles.Add(new ScriptBundle("~/bundles/apps")
                .Include("~/Scripts/app/AjaxCall.js")
                .Include("~/Scripts/app/app.bindings.js")
                .Include("~/Scripts/app/app.extenders.js")
                .Include("~/Scripts/kendo.web.min.js")
                .Include("~/Scripts/kendo.core.min.js")
                .Include("~/Scripts/kendo.upload.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/bootstrap-responsive.css",
                      "~/Content/bootstrap.icon-large.min.css",
                      "~/Content/kendo.common.min.css",
                      "~/Content/kendo.default.min.css",
                      "~/Content/kendo.bootstrap.min.css",
                      "~/Content/PagedList.css"));

            //FlatView
            //JQuery
            bundles.Add(new ScriptBundle("~/content/flat/js/jquery")
                .Include("~/Content/Flat/js/jquery.min.js")
                );
            bundles.Add(new ScriptBundle("~/content/flat/js/bootstrap")
                .Include("~/Content/Flat/js/bootstrap.js")
               );

            //Base theme Js
            bundles.Add(new ScriptBundle("~/content/flat/js")
                .Include("~/Content/Flat/js/eakroko.js")
                .Include("~/Content/Flat/js/application.js")
               );

            //Plgins
            bundles.Add(new ScriptBundle("~/content/flat/js/plugins")
              .Include("~/Content/Flat/js/plugins/bootbox/jquery.bootbox.js")
              .Include("~/Content/Flat/js/plugins/chosen/chosen.jquery.min.js")
              .Include("~/Content/Flat/js/plugins/ckeditor/ckeditor.js")
              .Include("~/Content/Flat/js/plugins/colorbox/jquery.colorbox-min.js")
              .Include("~/Content/Flat/js/plugins/colorpicker/bootstrap-colorpicker.js")
              .Include("~/Content/Flat/js/plugins/complexify/jquery.complexify.js")
              .Include("~/Content/Flat/js/plugins/datatable/jquery.dataTables.js")
              .Include("~/Content/Flat/js/plugins/datepicker/bootstrap-datepicker.js")
              .Include("~/Content/Flat/js/plugins/daterangepicker/daterangepicker.js")
              .Include("~/Content/Flat/js/plugins/easy-pie-chart/jquery.easy-pie-chart.min.js")
              .Include("~/Content/Flat/js/plugins/elfinder/elfinder.min.js")
              .Include("~/Content/Flat/js/plugins/fileupload/bootstrap-fileupload.min.js")
              .Include("~/Content/Flat/js/plugins/flot/jquery.flot.js")
              .Include("~/Content/Flat/js/plugins/form/jquery.form.min.js")
              .Include("~/Content/Flat/js/plugins/fullcalendar/fullcalendar.min.js")
              .Include("~/Content/Flat/js/plugins/gmap/gmap3.min.js")
              .Include("~/Content/Flat/js/plugins/gritter/jquery.gritter.min.js")
              .Include("~/Content/Flat/js/plugins/icheck/jquery.icheck.min.js")
              .Include("~/Content/Flat/js/plugins/imagesLoaded/jquery.imagesloaded.min.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/maskedinput", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/masonry", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/mockjax", "*.min.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/momentjs", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/multiselect", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/nicescroll", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/pageguide", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/placeholder", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/plupload", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/select2", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/slimscroll", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/sparklines", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/tagsinput", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/timepicker", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/touch-punch", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/touchwipe", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/validation", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/vmap", "jquery.vmap.min.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/wizard", "*.js")
              .IncludeDirectory("~/Content/Flat/js/plugins/xeditable", "bootstrap-editable.min.js")
              );
            bundles.Add(new ScriptBundle("~/content/flat/js/plugins/jquery-ui")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.core.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.widget.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.mouse.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.draggable.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.resizable.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.sortable.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.spinner.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.slider.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.selectable.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.position.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.droppable.min.js")
              .Include("~/Content/Flat/js/plugins/jquery-ui/jquery.ui.datepicker.min.js")
           );
            bundles.Add(new ScriptBundle("~/content/flat/js/plugins/jquery-dynatree")
           .Include("~/Content/Flat/js/plugins/dynatree/jquery.dynatree.js"));

            bundles.Add(new StyleBundle("~/content/flat/css")
                .IncludeDirectory("~/Content/Flat/css", "*.css", true));


            BundleTable.EnableOptimizations = true;
        }
    }

    public class MTBundleConfig
    {
        /// <summary>
        /// Registering default bundles
        /// </summary>
        /// <param name="bundles"></param>
        public static void RegisterBundles(BundleCollection bundles)
        {
       
            bundles.UseCdn = true;
            // Global CSS Bundle
            bundles.Add(new StyleBundle("~/bundles/appseconnect/global/css")
                    .Include("~/Content/themes30/global/plugins/font-awesome/css/font-awesome.css")
                    .Include("~/Content/themes30/global/plugins/simple-line-icons/simple-line-icons.min.css")
                    .Include("~/Content/themes30/global/plugins/bootstrap/css/bootstrap.min.css")
                    .Include("~/Content/themes30/global/plugins/uniform/css/uniform.default.css")
                    .Include("~/Content/themes30/global/plugins/bootstrap-switch/css/bootstrap-switch.min.css")
                    .Include("~/Content/themes30/global/css/components.min.css")
                    .Include("~/Content/themes30/global/css/plugins.min.css"));


            // Theme Layout 
            bundles.Add(new StyleBundle("~/bundles/layout/css")
                .Include("~/Content/themes30/layouts/layout/css/layout.min.css")
                .Include("~/Content/themes30/layouts/layout/css/themes/darkblue.min.css")
                .Include("~/Content/themes30/layouts/layout/css/custom.css")
                .Include("~/Content/themes30/global/plugins/bootstrap-daterangepicker/daterangepicker.min.css")
                .Include("~/Content/themes30/global/plugins/fullcalendar/fullcalendar.min.css")
                .Include("~/Content/themes30/global/plugins/jqvmap/jqvmap/jqvmap.css")
                .Include("~/Content/themes30/global/plugins/select2/css/select2.css")
                .Include("~/Content/themes30/global/plugins/ladda/ladda-themeless.min.css"));



            // JavaScript 

            bundles.Add(new ScriptBundle("~/bundles/global/js/jquery")
                .Include("~/Content/themes30/global/plugins/jquery.min.js")
                 .Include("~/Scripts/jquery.cookie.js")
                 .Include("~/Scripts/jquery.signalR-2.1.0.min.js")
                 .Include("~/Content/themes30/global/pages/scripts/ui-blockui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/global/js")
                .Include("~/Content/themes30/global/plugins/js.cookie.min.js")
                .Include("~/Content/themes30/global/plugins/bootstrap-hover-dropdown/bootstrap-hover-dropdown.min.js")
                .Include("~/Content/themes30/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js")
                .Include("~/Content/themes30/global/plugins/jquery.blockui.min.js")
                .Include("~/Content/themes30/global/plugins/uniform/jquery.uniform.min.js")
                .Include("~/Content/themes30/global/plugins/bootstrap-switch/js/bootstrap-switch.min.js")
                .Include("~/Content/themes30/global/scripts/datatable.js")
                .Include("~/Content/themes30/global/plugins/datatables/datatables.min.js")
                .Include("~/Content/themes30/global/plugins/datatables/plugins/bootstrap/datatables.bootstrap.js")
                .Include("~/Content/themes30/global/plugins/bootstrap-datepicker/js/bootstrap-datepicker.js")
                .Include("~/Content/themes30/global/plugins/select2/js/select2.min.js")
                .Include("~/Content/themes30/global/plugins/ladda/spin.min.js")
                .Include("~/Content/themes30/global/plugins/ladda/ladda.min.js")
                .Include("~/Content/themes30/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js")
                .Include("~/Content/themes30/global/plugins/bootbox/bootbox.min.js"));


            // Knockout
            bundles.Add(new ScriptBundle("~/bundles/knockout/js")
                .Include("~/Scripts/knockout-2.3.0.js")
                .Include("~/Scripts/knockout-sortable.js"));

            bundles.Add(new ScriptBundle("~/bundles/global/app/js")
                .Include("~/Content/themes30/global/scripts/app.js")
                .Include("~/Content/themes30/global/plugins/bootstrap-toastr/toastr.min.js")
                .Include("~/Content/themes30/layouts/layout/scripts/layout.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/app/loaders")
                .Include("~/Scripts/app/SignalRHub.js")
                .Include("~/Scripts/app/AjaxCall.js")
                .Include("~/Scripts/app/PortletLoader.js"));

           // BundleConfig.RegisterBundles(bundles);

            BundleTable.EnableOptimizations = false;
        }
    }
}
