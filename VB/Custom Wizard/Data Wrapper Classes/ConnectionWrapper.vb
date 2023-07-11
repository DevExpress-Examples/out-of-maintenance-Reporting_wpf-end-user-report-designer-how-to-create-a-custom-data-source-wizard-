Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.Mvvm.POCO

Namespace CustomDataSourceWizard

	Public Class ConnectionWrapper
		Protected Sub New()
		End Sub
		Public Shared Function Create() As ConnectionWrapper
			Return ViewModelSource.Create(Function() New ConnectionWrapper())
		End Function
		Public Overridable Property ConnectionName() As String
		Public Overridable Property ConnectionParameters() As DataConnectionParametersBase
	End Class
End Namespace
