using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.DataAccess.Sql;

namespace CustomDataSourceWizard {
    public class CustomClosingEventArgs : EventArgs{
        public CustomClosingEventArgs() {
        }
        public CustomClosingEventArgs(CloseMode closeMode) {
            this.CloseMode = closeMode;
        }
        public CustomClosingEventArgs(SqlDataSource ds, string dataMember, CloseMode closeMode) {
            this.DataSource = ds;
            this.DataMember = dataMember;
            this.CloseMode = closeMode;
        }
        public SqlDataSource DataSource {
            get;
            set;
        }
        public string DataMember {
            get;
            set;
        }
        public CloseMode CloseMode {
            get;
            set;
        }
    }
    
}
