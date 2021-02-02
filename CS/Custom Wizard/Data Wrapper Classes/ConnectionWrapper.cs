using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.Mvvm.POCO;

namespace CustomDataSourceWizard {

    public class ConnectionWrapper {
        protected ConnectionWrapper() {
        }
        public static ConnectionWrapper Create() {
            return ViewModelSource.Create(() => new ConnectionWrapper());
        }
        public virtual string ConnectionName {
            get;
            set;
        }
        public virtual DataConnectionParametersBase ConnectionParameters {
            get;
            set;
        }
    }
}
