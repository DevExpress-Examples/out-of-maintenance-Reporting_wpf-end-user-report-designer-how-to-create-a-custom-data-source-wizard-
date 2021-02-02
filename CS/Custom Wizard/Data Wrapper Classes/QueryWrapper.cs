using System;
using System.ComponentModel;
using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm.POCO;

namespace CustomDataSourceWizard {
    public class QueryWrapper : IEditableObject, IDataErrorInfo {
        private QueryWrapper Cache { get; set; }
        private object CurrentModel {
            get { return this; }
        }
        public virtual string Name { get; set; }
        public virtual string Sql { get; set; }
        internal DataSourceWrapper OwnerDataSource { get; set; }
        public static QueryWrapper Create() {
            return ViewModelSource.Create(() => new QueryWrapper());
        }
        protected QueryWrapper() {
        }
        public CustomSqlQuery CreateCustomSqlQuery() {
            return new CustomSqlQuery(Name, Sql);
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
            Cache = ViewModelSource.Create(() => new QueryWrapper());
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
            Cache = default(QueryWrapper);
        }

        string IDataErrorInfo.Error {
            get { return ((IDataErrorInfo)this)["Name"] + ((IDataErrorInfo)this)["Sql"]; }
        }
        string IDataErrorInfo.this[string columnName] {
            get {
                string errorMessage = string.Empty;
                switch(columnName) {
                    case "Name":
                        if(string.IsNullOrEmpty(Name))
                            errorMessage = "You cannot leave the Query Name empty.";
                        break;
                    case "Sql":
                        errorMessage = ValidateSql();
                        break;
                }
                return errorMessage;
            }
        }


        string ValidateSql() {
            if(string.IsNullOrEmpty(this.Sql)) {
                return "You cannot leave the Sql empty";
            }
            string sql = Sql;
            var testDS = new SqlDataSource {
                ConnectionName = this.OwnerDataSource.Connection.ConnectionName,
                ConnectionParameters = this.OwnerDataSource.Connection.ConnectionParameters
            };
            try {
                testDS.Queries.Add(new CustomSqlQuery(this.Name, sql));
                testDS.RebuildResultSchema();
            } catch(Exception e) {
                return e.Message;
            }
            return string.Empty;
        }
    }
}
