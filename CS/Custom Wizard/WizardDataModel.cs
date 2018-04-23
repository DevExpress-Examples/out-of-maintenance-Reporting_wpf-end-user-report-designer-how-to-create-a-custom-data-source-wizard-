using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace CustomDataSourceWizard {
    public class WizardDataModel : IEditableObject {
        [ServiceProperty(Key = "queryEditorService")]
        protected virtual IDialogService EditQueryDialogService {
            get { return null; }
        }
        [ServiceProperty(Key = "dsEditorService")]
        protected virtual IDialogService EditDataSourceDialogService {
            get { return null; }
        }
        public event EventHandler<CustomClosingEventArgs> EndEditing;
        private WizardDataModel Cache { get; set; }
        private object CurrentModel {
            get { return this; }
        }
        public virtual DataSourceWrapper SelectedDataSource { get; set; }
        public virtual ObservableCollection<DataSourceWrapper> DataSources { get; set; }
        [BindableProperty(false)]
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
            IEditableObject editableObject = dsw;
            editableObject.BeginEdit();
            bool acceptChanges = OpenDataSourceEditor(dsw);
            if(acceptChanges) {
                dsw.DoEndEdit();
                DataSources.Add(dsw);
                SelectedDataSource = dsw;
            } else {
                dsw.DoCancelEdit();
            }
        }
        public void EditDataSource() {
            DataSourceWrapper dsw = SelectedDataSource;
            IEditableObject editableObject = dsw;
            editableObject.BeginEdit();
            bool acceptChanges = OpenDataSourceEditor(dsw);
            if(acceptChanges) {
               dsw.DoEndEdit();
            } else {
                dsw.DoCancelEdit();
            }
        }
        private bool OpenDataSourceEditor(DataSourceWrapper dsw) {
            IDataErrorInfo dataErrorInfo = dsw;
            UICommand okCommand = new UICommand {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
                    delegate(CancelEventArgs x) { },
                    delegate(CancelEventArgs x) { return string.IsNullOrEmpty(dataErrorInfo.Error); }
                )
            };
            UICommand cancelCommand = new UICommand {
                Id = MessageBoxResult.Cancel,
                Caption = "Cancel",
                IsCancel = true,
                IsDefault = false
            };
            UICommand result = EditDataSourceDialogService.ShowDialog(
                dialogCommands: new List<UICommand> { okCommand, cancelCommand },
                title: "Edit Data Source",
                viewModel: new object[] { dsw, DataConnections });
            bool acceptChanges = result == okCommand;
            return acceptChanges;
        }
        public bool CanAddDataSource() {
            return true;
        }
        public bool CanEditDataSource() {
            return SelectedDataSource != null;
        }
        public void RemoveSelectedDataSource() {
            DataSources.Remove(SelectedDataSource);
        }
        public bool CanRemoveSelectedDataSource() {
            return DataSources.Count > 0 && SelectedDataSource != null;
        }
        #endregion
        #region Query Add / Edit / Remove
        public void AddQuery() {
            var qw = QueryWrapper.Create();
            IEditableObject editableObject = qw;
            qw.OwnerDataSource = SelectedDataSource;
            editableObject.BeginEdit();
            bool acceptChanges = OpenQueryEditor(qw);
            if(acceptChanges) {
                qw.DoEndEdit();
                SelectedDataSource.Queries.Add(qw);
                SelectedDataSource.SelectedQuery = qw;
            } else {
                qw.DoCancelEdit();
            }
        }
        bool OpenQueryEditor(QueryWrapper qw) {
            var okCommand = new UICommand {
                Caption = "OK",
                IsCancel = false,
                IsDefault = true,
                Command = new DelegateCommand<CancelEventArgs>(
                    delegate(CancelEventArgs x) { },
                    delegate(CancelEventArgs x) { return string.IsNullOrEmpty(((IDataErrorInfo)qw).Error); })
            };
            UICommand cancelCommand = new UICommand() {
                Id = MessageBoxResult.Cancel,
                Caption = "Cancel",
                IsCancel = true,
                IsDefault = false
            };
            UICommand result = EditQueryDialogService.ShowDialog(
                dialogCommands: new List<UICommand> { okCommand, cancelCommand },
                title: "Edit Query",
                viewModel: qw);

            bool acceptChanges = result == okCommand;
            return acceptChanges;
        }
        public bool CanAddQuery() {
            return SelectedDataSource != null && SelectedDataSource.Queries != null;
        }
        public void EditQuery() {
            QueryWrapper qw = SelectedDataSource.SelectedQuery;
            IEditableObject editableObject = qw;
            editableObject.BeginEdit();
            bool acceptChanges = OpenQueryEditor(qw);
            if(acceptChanges) {
                qw.DoEndEdit();
                SelectedDataSource.SelectedQuery = qw;
            } else {
                qw.DoCancelEdit();
            }
        }
        public bool CanEditQuery() {
            return SelectedDataSource != null && SelectedDataSource.SelectedQuery != null;
        }
        public void RemoveQuery() {
            this.SelectedDataSource.RemoveSelectedQuery();
        }
        public bool CanRemoveQuery() {
            return SelectedDataSource != null && SelectedDataSource.SelectedQuery != null;
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
            ((IEditableObject)this).CancelEdit();
            OnEndEditing(new CustomClosingEventArgs(CloseMode.Cancel));
        }
        public bool CanDoCancelEdit() {
            return true;
        }
        public void DoEndEdit() {
            ((IEditableObject)this).EndEdit();
            OnEndEditing(new CustomClosingEventArgs(CloseMode.Apply));
        }
        public virtual bool CanDoEndEdit() {
            return SelectedDataSource != null && SelectedDataSource.SelectedQuery != null;
        }
        #endregion
        #region IEditableObject Members
        void IEditableObject.BeginEdit() {
            Cache = ViewModelSource.Create(() => new WizardDataModel());
            foreach(var info in CurrentModel.GetType().GetProperties()) {
                if(!info.CanRead || !info.CanWrite)
                    continue;
                var oldValue = info.GetValue(CurrentModel, null);
                Cache.GetType().GetProperty(info.Name).SetValue(Cache, oldValue, null);
            }
        }

        void IEditableObject.CancelEdit() {
            foreach(var info in CurrentModel.GetType().GetProperties()) {
                if(!info.CanRead || !info.CanWrite)
                    continue;
                var oldValue = info.GetValue(Cache, null);
                CurrentModel.GetType().GetProperty(info.Name).SetValue(CurrentModel, oldValue, null);
            }
        }

        void IEditableObject.EndEdit() {
            Cache = default(WizardDataModel);
        }
        #endregion
    }
}

