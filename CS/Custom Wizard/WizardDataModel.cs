using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using WpfReportDesignerDataSourceWizard;

namespace CustomDataSourceWizard {
    public class WizardDataModel : IEditableObject {
        [ServiceProperty(Key = "queryEditorService")]
        protected virtual IDialogService EditQueryDialogService {
            get {
                return null;
            }
        }
        [ServiceProperty(Key = "dsEditorService")]
        protected virtual IDialogService EditDataSourceDialogService {
            get {
                return null;
            }
        }
        public event EventHandler<CustomClosingEventArgs> EndEditing;       
        private WizardDataModel Cache {
            get;
            set;
        }
        private object CurrentModel {
            get {
                return this;
            }
        }
        public virtual DataSourceWrapper SelectedDataSource {
            get;
            set;
        }
        public virtual System.Collections.ObjectModel.ObservableCollection<DataSourceWrapper> DataSources { get; set; }
        [BindableProperty(isBindable:false)]
        public List<ConnectionWrapper> DataConnections { get; set; }
        protected WizardDataModel() {
            InitTestData();
        }
        public static WizardDataModel Create() {
            return ViewModelSource.Create(() => new WizardDataModel());
        }
        protected virtual void OnEndEditing(CustomClosingEventArgs e) {
            EventHandler<CustomClosingEventArgs> handler = EndEditing;
            if(handler != null) {
                handler(this, e);
            }
        }
        public SqlDataSource CreateDataSource() {
            if(SelectedDataSource == null) {
                return null;
            }
            return SelectedDataSource.CreateSqlDataSource();
        }

        #region Data Source Add / Edit / Remove
        public void InitDataSource() {
            DataSourceWrapper dsw = DataSourceWrapper.Create();            
            dsw.BeginEdit();
            bool acceptChanges = OpenDataSourceEditor(dsw);
            if(acceptChanges) {
                dsw.DoEndEdit();
                this.DataSources.Add(dsw);
                this.SelectedDataSource = dsw;
            } else {
                dsw.DoCancelEdit();
            }
        }
        public void EditDataSource() {
            DataSourceWrapper dsw = this.SelectedDataSource;
            dsw.BeginEdit();
            bool acceptChanges = OpenDataSourceEditor(dsw);
            if(acceptChanges) {
               dsw.DoEndEdit();                
            } else {
                dsw.DoCancelEdit();
            }
        }
        private bool OpenDataSourceEditor(DataSourceWrapper dsw) {
            UICommand okCommand = new UICommand() {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(x => {
                }, x => {
                    return string.IsNullOrEmpty(dsw.Error);
                }),
            };
            UICommand cancelCommand = new UICommand() {
                Id = MessageBoxResult.Cancel,
                Caption = "Cancel",
                IsCancel = true,
                IsDefault = false,
            };
            UICommand result = EditDataSourceDialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand },
                title: "Edit Data Source",
                viewModel: new object[] { dsw, this.DataConnections });
            bool acceptChanges = result == okCommand;
            return acceptChanges;
        }
        public bool CanAddDataSource() {
            return true;
        }       
        public bool CanEditDataSource() {
            return this.SelectedDataSource != null;
        }
        public void RemoveSelectedDataSource() {
            this.DataSources.Remove(this.SelectedDataSource);
        }
        public bool CanRemoveSelectedDataSource() {
            return (this.DataSources.Count > 0 && this.SelectedDataSource != null);
        }
        #endregion
        #region Query Add / Edit / Remove
        public void AddQuery() {
            QueryWrapper qw = QueryWrapper.Create();
            qw.OwnerDataSource = this.SelectedDataSource;
            qw.BeginEdit();
            bool acceptChanges = OpenQueryEditor(qw);
            if(acceptChanges) {
                qw.DoEndEdit();
                this.SelectedDataSource.Queries.Add(qw);
                this.SelectedDataSource.SelectedQuery = qw;
            } else {
                qw.DoCancelEdit();
            }
        }
        private bool OpenQueryEditor(QueryWrapper qw) {
            UICommand okCommand = new UICommand() {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(x => {
                }, x => {
                    return string.IsNullOrEmpty(qw.Error);
                }),
            };
            UICommand cancelCommand = new UICommand() {
                Id = MessageBoxResult.Cancel,
                Caption = "Cancel",
                IsCancel = true,
                IsDefault = false,
            };
            UICommand  result = EditQueryDialogService.ShowDialog(
                dialogCommands: new List<UICommand>() { okCommand, cancelCommand }, 
                title:"Edit Query", 
                viewModel:qw);

            bool acceptChanges = result == okCommand;
            return acceptChanges;
        }
        public bool CanAddQuery() {
            return (this.SelectedDataSource!=null && this.SelectedDataSource.Queries != null);
        }
        public void EditQuery() {
            QueryWrapper qw = this.SelectedDataSource.SelectedQuery;
            qw.BeginEdit();
            bool acceptChanges = OpenQueryEditor(qw);
            if(acceptChanges) {
                qw.DoEndEdit();
                this.SelectedDataSource.SelectedQuery = qw;
            } else {
                qw.DoCancelEdit();
            }
        }
        public bool CanEditQuery() {
            return this.SelectedDataSource != null && this.SelectedDataSource.SelectedQuery != null;
        }
        public void RemoveQuery() {
            this.SelectedDataSource.RemoveSelectedQuery();
        }
        public bool CanRemoveQuery() {
            return (this.SelectedDataSource != null && this.SelectedDataSource.SelectedQuery != null);
        }
        #endregion        
        #region Test data source initialization
        private void InitTestData() {
            this.DataConnections = new List<ConnectionWrapper>();
            this.DataSources = new ObservableCollection<DataSourceWrapper>();

            CreateNorthwind();
            CreateDepartmentsWorks();
            CreateGSP();

            AddInvalidConnection();
        }
        private void CreateGSP() {
            Access97ConnectionParameters connectionParameters = new Access97ConnectionParameters(@"|DataDirectory|\AppData\gsp.mdb", "", "");
            ConnectionWrapper cw = InitConnection("GSPConnection", connectionParameters);
            DataSourceWrapper dsw = DataSourceWrapper.Create();
            dsw.Name = "GSP";
            dsw.Connection = cw;
            this.DataSources.Add(dsw);
        }
        private void CreateDepartmentsWorks() {
            Access97ConnectionParameters connectionParameters = new Access97ConnectionParameters(@"|DataDirectory|\AppData\Departments.mdb", "", "");
            ConnectionWrapper cw = InitConnection("DepartmentsConnection", connectionParameters);
            DataSourceWrapper dsw = DataSourceWrapper.Create();
            dsw.Name = "Departments";
            dsw.Connection = cw;
            QueryWrapper qwSalesPerson = QueryWrapper.Create();
            qwSalesPerson.OwnerDataSource = dsw;
            qwSalesPerson.Name = "Departments";
            qwSalesPerson.Sql = "select * from Departments";
            dsw.Queries.Add(qwSalesPerson);

            this.DataSources.Add(dsw);
        }
        private void CreateNorthwind() {
            DataConnectionParametersBase connectionParameters = new Access97ConnectionParameters(@"|DataDirectory|\AppData\nwind.mdb", "", "");
            ConnectionWrapper cw = InitConnection("NorthwindConnection", connectionParameters);
            DataSourceWrapper dsw = DataSourceWrapper.Create();
            dsw.Name = "Northwind";
            dsw.Connection = cw;
            QueryWrapper qwCategories = QueryWrapper.Create();
            qwCategories.OwnerDataSource = dsw;
            qwCategories.Name = "Categories";
            qwCategories.Sql = "select * from Categories";
            dsw.Queries.Add(qwCategories);
            QueryWrapper qwProducts = QueryWrapper.Create();
            qwProducts.OwnerDataSource = dsw;
            qwProducts.Name = "Products";
            qwProducts.Sql = "select * from Products";
            dsw.Queries.Add(qwProducts);
            this.DataSources.Add(dsw);
        }
        private ConnectionWrapper InitConnection(string connectionName, DataConnectionParametersBase connectionParameters) {
            ConnectionWrapper cw = ConnectionWrapper.Create();
            cw.ConnectionName = connectionName;
            cw.ConnectionParameters = connectionParameters;
            this.DataConnections.Add(cw);
            return cw;
        }
        private void AddInvalidConnection() {
            ConnectionWrapper cw = InitConnection("InvalidConnection", new CustomStringConnectionParameters("invalidConnection"));
        }        
        #endregion
        #region View Model Edit & Cancel methods
        public void DoCancelEdit() {
            this.CancelEdit();
            OnEndEditing(new CustomClosingEventArgs(CloseMode.Cancel));
        }
        public bool CanDoCancelEdit() {
            return true;
        }
        public void DoEndEdit() {
            this.EndEdit();
            OnEndEditing(new CustomClosingEventArgs(CloseMode.Apply));
        }
        public virtual bool CanDoEndEdit() {
            return (this.SelectedDataSource != null && this.SelectedDataSource.SelectedQuery != null);
        }
        #endregion
        #region IEditableObject Members
        [Command(isCommand: false)]
        public void BeginEdit() {
            Cache = ViewModelSource.Create(() => new WizardDataModel());
            foreach(var info in CurrentModel.GetType().GetProperties()) {
                if(!info.CanRead || !info.CanWrite)
                    continue;
                var oldValue = info.GetValue(CurrentModel, null);
                Cache.GetType().GetProperty(info.Name).SetValue(Cache, oldValue, null);
            }
        }
         [Command(isCommand: false)]
        public void CancelEdit() {
            foreach(var info in CurrentModel.GetType().GetProperties()) {
                if(!info.CanRead || !info.CanWrite)
                    continue;
                var oldValue = info.GetValue(Cache, null);
                CurrentModel.GetType().GetProperty(info.Name).SetValue(CurrentModel, oldValue, null);
            }
        }
         [Command(isCommand: false)]
        public void EndEdit() {
            Cache = default(WizardDataModel);
        }
        #endregion
    }
}

