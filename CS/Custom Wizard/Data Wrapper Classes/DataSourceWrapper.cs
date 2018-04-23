using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
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
        private DataSourceWrapper Cache {
            get;
            set;
        }
        private object CurrentModel {
            get {
                return this;
            }
        }
        public virtual string Name { get; set; }
        public virtual ConnectionWrapper Connection { get; set; }
        public virtual QueryWrapper SelectedQuery {
            get;
            set;
        }
        public ObservableCollection<QueryWrapper> Queries {get;set;}
        public SqlDataSource CreateSqlDataSource() {
            SqlDataSource dataSource = new SqlDataSource() {
                Name = this.Name,
                ConnectionName = this.Connection.ConnectionName,
                ConnectionParameters =  this.Connection.ConnectionParameters
            };
            if(Queries!= null) {
                foreach(QueryWrapper query in Queries) {
                    dataSource.Queries.Add(query.CreateCustomSqlQuery());
                }
            }
            dataSource.RebuildResultSchema();
            return dataSource;
        }

        public void DoCancelEdit() {
            this.CancelEdit();
        }
        public bool CanCancelEdit() {
            return true;
        }
        public void DoEndEdit() {
            this.EndEdit();
        }
        public virtual bool CanEndEdit() {
            return true;
        }
        #region IEditableObject Members
         [Command(isCommand: false)]
        public void BeginEdit() {
            Cache = ViewModelSource.Create(() => new DataSourceWrapper());
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
            Cache = default(DataSourceWrapper);
        }
        #endregion
        void qw_EndEditing(object sender, CustomClosingEventArgs e) {
            if(e.CloseMode == CloseMode.Cancel)
                return;
            QueryWrapper qw = sender as QueryWrapper;
            this.Queries.Add(qw);
            this.SelectedQuery = qw;
        }
        internal void RemoveSelectedQuery() {
            this.Queries.Remove(this.SelectedQuery);
        }

        #region IDataErrorInfo Members

        public string Error {
            get {
                return this["Name"] + this["Connection"];                
            }
        }
        public string this[string columnName] {
            get {
                String errorMessage = String.Empty;
                switch(columnName) {
                    case "Name":
                        if(String.IsNullOrEmpty(this.Name))
                            errorMessage = "You cannot leave the Data source name empty.";
                        break;
                    case "Connection":
                        errorMessage = ValidateConnection();
                        break;
                    
                }
               return errorMessage;
            }
        }
        private string ValidateConnection() {
            if(this.Connection == null || String.IsNullOrEmpty(this.Connection.ConnectionName) || this.Connection.ConnectionParameters == null)
                return "You cannot leave the Connection empty";
            SqlDataSource testDS = new SqlDataSource();
            testDS.ConnectionName = "testName";
            ConnectionWrapper cw = (ConnectionWrapper)this.Connection;
            testDS.ConnectionParameters = cw.ConnectionParameters;
            try {
                testDS.Connection.Open();
            } catch(Exception e) {
                return e.Message;
            }
            return string.Empty;
        }
        #endregion
    }
    
}
