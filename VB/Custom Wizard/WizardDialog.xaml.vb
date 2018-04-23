Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.Sql

Namespace CustomDataSourceWizard
	''' <summary>
	''' Interaction logic for WizardDialog.xaml
	''' </summary>
	Partial Public Class WizardDialog
		Inherits DevExpress.Xpf.Core.DXWindow

		Public Event EndWizard As EventHandler(Of CustomClosingEventArgs)
		Public Sub New()
			InitializeComponent()
		End Sub
		Protected Overridable Sub OnEndWizard(ByVal e As CustomClosingEventArgs)
			Dim handler As EventHandler(Of CustomClosingEventArgs) = EndWizardEvent
			If handler IsNot Nothing Then
				handler(Me, e)
			End If
		End Sub
		Private Sub DXWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			AddHandler TryCast(Me.DataContext, WizardDataModel).EndEditing, AddressOf WizardDialog_EndEditing
			TryCast(Me.DataContext, WizardDataModel).BeginEdit()
		End Sub

		Private Sub WizardDialog_EndEditing(ByVal sender As Object, ByVal e As CustomClosingEventArgs)
			Me.Close()
			If e.CloseMode = CloseMode.Apply Then
				Dim dsw As DataSourceWrapper = (TryCast(Me.DataContext, WizardDataModel)).SelectedDataSource
				Dim ds As SqlDataSource = (TryCast(Me.DataContext, WizardDataModel)).CreateDataSource()
				Dim queryName As String = (TryCast(Me.DataContext, WizardDataModel)).SelectedDataSource.SelectedQuery.Name
				OnEndWizard(New CustomClosingEventArgs(ds, queryName, e.CloseMode))
			Else
				OnEndWizard(New CustomClosingEventArgs(CloseMode.Cancel))
			End If

		End Sub
	End Class
End Namespace
