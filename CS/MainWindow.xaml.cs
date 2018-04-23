using System.Windows;
using System.Windows.Input;
using CustomDataSourceWizard;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Native;
using DevExpress.XtraReports.UI;

namespace WpfReportDesignerDataSourceWizard {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DevExpress.Xpf.Core.DXWindow {
        public static DependencyProperty RunDataSourceWizardCommandProperty;

        static MainWindow() {
            DependencyPropertyRegistrator<MainWindow>.New()
                .Register(x => x.RunDataSourceWizardCommand, out RunDataSourceWizardCommandProperty, null);
        }

        public ICommand RunDataSourceWizardCommand {
            get { return (ICommand)GetValue(RunDataSourceWizardCommandProperty); }
            set { SetValue(RunDataSourceWizardCommandProperty, value); }
        }

        public MainWindow() {
            RunDataSourceWizardCommand = new DelegateCommand(RunDataSourceWizard);
            InitializeComponent();
        }

        void RunDataSourceWizard() {
            var wizard = new WizardDialog();
            wizard.EndWizard += wizard_EndWizard;
            wizard.ShowDialog();
        }

        void wizard_EndWizard(object sender, CustomClosingEventArgs e) {
            if(e.CloseMode == CloseMode.Cancel)
                return;
            XtraReport report = CreateReport(e.DataSource, e.DataMember);
            reportDesigner.NewDocument(report);
        }

        static XtraReport CreateReport(SqlDataSource ds, string dataMember) {
            var report = new XtraReport {
                DataSource = ds,
                DataMember = dataMember
            };
            return report;
        }
    }
}
