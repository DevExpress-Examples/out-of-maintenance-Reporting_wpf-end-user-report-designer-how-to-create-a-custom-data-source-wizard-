using System;
using System.ComponentModel;
using System.Windows;
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

        WizardDataModel WizardDataModel {
            get { return (WizardDataModel)DataContext; }
        }

        protected virtual void OnEndWizard(CustomClosingEventArgs e) {
            EventHandler<CustomClosingEventArgs> handler = EndWizard;
            if(handler != null) {
                handler(this, e);
            }
        }
        private void DXWindow_Loaded(object sender, RoutedEventArgs e) {
            WizardDataModel.EndEditing += WizardDialog_EndEditing;
            IEditableObject editableObject = WizardDataModel;
            editableObject.BeginEdit();
        }

        void WizardDialog_EndEditing(object sender, CustomClosingEventArgs e) {
            this.Close();
            if(e.CloseMode == CloseMode.Apply) {
                DataSourceWrapper dsw = WizardDataModel.SelectedDataSource;
                SqlDataSource ds = WizardDataModel.CreateDataSource();
                string queryName = WizardDataModel.SelectedDataSource.SelectedQuery.Name;
                OnEndWizard(new CustomClosingEventArgs(ds, queryName, e.CloseMode));
            } else {
                OnEndWizard(new CustomClosingEventArgs(CloseMode.Cancel));
            }
        }
    }
}
