using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace CustomDataSourceWizard {
    public class QueryWrapper : IEditableObject, IDataErrorInfo {        
        private QueryWrapper Cache {
            get;
            set;
        }
        private object CurrentModel {
            get {
                return this;
            }
        }
        public  virtual string Name {
            get;
            set;
        }
        public virtual string Sql {
            get;
            set;
        }
        internal DataSourceWrapper OwnerDataSource {
            get;
            set;
        }
        public static QueryWrapper Create() {
            return ViewModelSource.Create(() => new QueryWrapper());
        }
        protected QueryWrapper() {
        }
        public CustomSqlQuery CreateCustomSqlQuery() {
            return new CustomSqlQuery(this.Name, this.Sql);
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
            Cache = ViewModelSource.Create(() => new QueryWrapper());
            foreach(var info in CurrentModel.GetType().GetProperties()) {
                if(!info.CanRead || !info.CanWrite)
                    continue;
                var oldValue = info.GetValue(CurrentModel, null);
                Cache.GetType().GetProperty(info.Name).SetValue(Cache, oldValue, null);
            }            
        }
         [Command(isCommand: false)]
        public void CancelEdit() {
            foreach (var info in CurrentModel.GetType().GetProperties()){
                if (!info.CanRead || !info.CanWrite) continue;
                var oldValue = info.GetValue(Cache, null);
                CurrentModel.GetType().GetProperty(info.Name).SetValue(CurrentModel, oldValue, null);
            }
        }
         [Command(isCommand: false)]
        public void EndEdit() {
            Cache = default(QueryWrapper);
        }
        #endregion

        #region IDataErrorInfo Members
        public string Error {
            get {
                return this["Name"] + this["Sql"];
            }
        }
        public string this[string columnName] {
            get {
                String errorMessage = String.Empty;
                switch(columnName) {
                   case "Name":
                        if(String.IsNullOrEmpty(this.Name))
                            errorMessage = "You cannot leave the Query Name empty.";
                        break;
                   case "Sql":
                        errorMessage = ValidateSql();
                        break;
                }
                return errorMessage;
            }
        }

        private string ValidateSql() {
            if(String.IsNullOrEmpty(this.Sql)) {
                return "You cannot leave the Sql empty";
            }
            string sql = this.Sql;
            SqlDataSource testDS = new SqlDataSource();
            testDS.ConnectionName = this.OwnerDataSource.Connection.ConnectionName;
            testDS.ConnectionParameters = this.OwnerDataSource.Connection.ConnectionParameters;
            try {
                testDS.Queries.Add(new CustomSqlQuery(this.Name, sql));    
                testDS.RebuildResultSchema();
            } catch(Exception e) {
                return e.Message;
            }
            return string.Empty;
        }
        #endregion
    }
}
