using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CustomDataSourceWizard;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Commands;
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.Xpf.Reports.UserDesigner.Native;
using DevExpress.XtraReports.UI;

namespace WpfReportDesignerDataSourceWizard {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DevExpress.Xpf.Core.DXWindow {
        static ReportDesigner reportDesigner = new ReportDesigner();
        static WizardDialog wizard;
        public static DelegateCommand RunDataSourceWizardCommand {
            get {
                return new DelegateCommand(RunDataSourceWizard);
            }
        }
        public MainWindow() {
            InitializeComponent();
            this.Content = reportDesigner;
        }
        static void RunDataSourceWizard() {
            wizard = new WizardDialog();
            wizard.EndWizard += wizard_EndWizard;
            wizard.ShowDialog();
        }
        static void wizard_EndWizard(object sender, CustomClosingEventArgs e) {
            if(e.CloseMode == CloseMode.Cancel)
                return;
            XtraReport report = CreateReport(e.DataSource, e.DataMember);
            reportDesigner.OpenDocument(report);
        }
        private static XtraReport CreateReport(SqlDataSource ds, string dataMember) {
            XtraReport report = new XtraReport();
            report.DataSource = ds;
            report.DataMember = dataMember;
            return report;
        }
    }
}
