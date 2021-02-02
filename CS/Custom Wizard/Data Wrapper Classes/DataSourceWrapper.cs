using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm.POCO;

namespace CustomDataSourceWizard {
    public class DataSourceWrapper : IEditableObject, IDataErrorInfo {
        public static DataSourceWrapper Create() {
            DataSourceWrapper dsw = ViewModelSource.Create(() => new DataSourceWrapper());
            dsw.Queries = new ObservableCollection<QueryWrapper>();
            return dsw;
        }
        protected DataSourceWrapper() {
        }
        private DataSourceWrapper Cache { get; set; }
        private object CurrentModel {
            get { return this; }
        }
        public virtual string Name { get; set; }
        public virtual ConnectionWrapper Connection { get; set; }
        public virtual QueryWrapper SelectedQuery { get; set; }
        public ObservableCollection<QueryWrapper> Queries { get; set; }
        public SqlDataSource CreateSqlDataSource() {
            SqlDataSource dataSource = new SqlDataSource() {
                Name = this.Name,
                ConnectionName = this.Connection.ConnectionName,
                ConnectionParameters = this.Connection.ConnectionParameters
            };
            if(Queries != null) {
                foreach(QueryWrapper query in Queries) {
                    dataSource.Queries.Add(query.CreateCustomSqlQuery());
                }
            }
            dataSource.RebuildResultSchema();
            return dataSource;
        }

        public void DoCancelEdit() {
            ((IEditableObject)this).CancelEdit();
        }
        public bool CanCancelEdit() {
            return true;
        }
        public void DoEndEdit() {
            ((IEditableObject)this).EndEdit();
        }
        public virtual bool CanEndEdit() {
            return true;
        }
        void IEditableObject.BeginEdit() {
            Cache = ViewModelSource.Create(() => new DataSourceWrapper());
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
            Cache = default(DataSourceWrapper);
        }
        void qw_EndEditing(object sender, CustomClosingEventArgs e) {
            if(e.CloseMode == CloseMode.Cancel)
                return;
            QueryWrapper qw = sender as QueryWrapper;
            Queries.Add(qw);
            SelectedQuery = qw;
        }
        internal void RemoveSelectedQuery() {
            Queries.Remove(SelectedQuery);
        }

        string IDataErrorInfo.Error {
            get { return ((IDataErrorInfo)this)["Name"] + ((IDataErrorInfo)this)["Connection"]; }
        }

        string IDataErrorInfo.this[string columnName] {
            get {
                string errorMessage = string.Empty;
                switch(columnName) {
                    case "Name":
                        if(string.IsNullOrEmpty(this.Name))
                            errorMessage = "You cannot leave the Data source name empty.";
                        break;
                    case "Connection":
                        errorMessage = ValidateConnection();
                        break;
                }
                return errorMessage;
            }
        }
        string ValidateConnection() {
            if(Connection == null || string.IsNullOrEmpty(Connection.ConnectionName) || Connection.ConnectionParameters == null)
                return "You cannot leave the Connection empty";
            var testDS = new SqlDataSource {
                ConnectionName = "testName",
                ConnectionParameters = Connection.ConnectionParameters
            };
            try {
                testDS.Connection.Open();
            } catch(Exception e) {
                return e.Message;
            }
            return string.Empty;
        }
    }
}
