Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports DevExpress.DataAccess.Sql

Namespace CustomDataSourceWizard
	Public Class CustomClosingEventArgs
		Inherits EventArgs

		Public Sub New()
		End Sub
		Public Sub New(ByVal closeMode As CloseMode)
			Me.CloseMode = closeMode
		End Sub
		Public Sub New(ByVal ds As SqlDataSource, ByVal dataMember As String, ByVal closeMode As CloseMode)
			Me.DataSource = ds
			Me.DataMember = dataMember
			Me.CloseMode = closeMode
		End Sub
		Public Property DataSource() As SqlDataSource
		Public Property DataMember() As String
		Public Property CloseMode() As CloseMode
	End Class

End Namespace
