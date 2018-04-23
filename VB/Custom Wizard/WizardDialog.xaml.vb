Imports System
Imports System.ComponentModel
Imports System.Windows
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

        Private ReadOnly Property WizardDataModel() As WizardDataModel
            Get
                Return CType(DataContext, WizardDataModel)
            End Get
        End Property

        Protected Overridable Sub OnEndWizard(ByVal e As CustomClosingEventArgs)
            Dim handler As EventHandler(Of CustomClosingEventArgs) = EndWizardEvent
            If handler IsNot Nothing Then
                handler(Me, e)
            End If
        End Sub
        Private Sub DXWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            AddHandler WizardDataModel.EndEditing, AddressOf WizardDialog_EndEditing
            Dim editableObject As IEditableObject = WizardDataModel
            editableObject.BeginEdit()
        End Sub

        Private Sub WizardDialog_EndEditing(ByVal sender As Object, ByVal e As CustomClosingEventArgs)
            Me.Close()
            If e.CloseMode = CloseMode.Apply Then
                Dim dsw As DataSourceWrapper = WizardDataModel.SelectedDataSource
                Dim ds As SqlDataSource = WizardDataModel.CreateDataSource()
                Dim queryName As String = WizardDataModel.SelectedDataSource.SelectedQuery.Name
                OnEndWizard(New CustomClosingEventArgs(ds, queryName, e.CloseMode))
            Else
                OnEndWizard(New CustomClosingEventArgs(CloseMode.Cancel))
            End If
        End Sub
    End Class
End Namespace
