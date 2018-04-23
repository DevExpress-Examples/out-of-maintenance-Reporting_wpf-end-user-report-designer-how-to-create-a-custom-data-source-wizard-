using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;

namespace CustomDataSourceWizard {
    /// <summary>
    /// Interaction logic for WizardDialog.xaml
    /// </summary>
    public partial class WizardDialog : DevExpress.Xpf.Core.DXWindow {
        public event EventHandler<CustomClosingEventArgs> EndWizard;        
        public WizardDialog() {
            InitializeComponent();
        }
        protected virtual void OnEndWizard(CustomClosingEventArgs e) {
            EventHandler<CustomClosingEventArgs> handler = EndWizard;
            if(handler != null) {
                handler(this, e);
            }
        }
        private void DXWindow_Loaded(object sender, RoutedEventArgs e) {
            (this.DataContext as WizardDataModel).EndEditing += WizardDialog_EndEditing;
            (this.DataContext as WizardDataModel).BeginEdit();
        }

        void WizardDialog_EndEditing(object sender, CustomClosingEventArgs e) {
            this.Close();
            if(e.CloseMode == CloseMode.Apply) {
                DataSourceWrapper dsw = (this.DataContext as WizardDataModel).SelectedDataSource;
                SqlDataSource ds = (this.DataContext as WizardDataModel).CreateDataSource();
                string queryName = (this.DataContext as WizardDataModel).SelectedDataSource.SelectedQuery.Name;
                OnEndWizard(new CustomClosingEventArgs(ds, queryName, e.CloseMode));
            } else {
                OnEndWizard(new CustomClosingEventArgs(CloseMode.Cancel));
            }
            
        }
    }
}
