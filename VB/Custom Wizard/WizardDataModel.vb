Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Input
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.Sql
Imports DevExpress.Mvvm
Imports DevExpress.Mvvm.DataAnnotations
Imports DevExpress.Mvvm.POCO
Imports WpfReportDesignerDataSourceWizard

Namespace CustomDataSourceWizard
	Public Class WizardDataModel
		Implements IEditableObject

		<ServiceProperty(Key := "queryEditorService")>
		Protected Overridable ReadOnly Property EditQueryDialogService() As IDialogService
			Get
				Return Nothing
			End Get
		End Property
		<ServiceProperty(Key := "dsEditorService")>
		Protected Overridable ReadOnly Property EditDataSourceDialogService() As IDialogService
			Get
				Return Nothing
			End Get
		End Property
		Public Event EndEditing As EventHandler(Of CustomClosingEventArgs)
		Private Property Cache() As WizardDataModel
		Private ReadOnly Property CurrentModel() As Object
			Get
				Return Me
			End Get
		End Property
		Public Overridable Property SelectedDataSource() As DataSourceWrapper
		Public Overridable Property DataSources() As System.Collections.ObjectModel.ObservableCollection(Of DataSourceWrapper)
        <BindableProperty(False)>
        Public Property DataConnections() As List(Of ConnectionWrapper)
		Protected Sub New()
			InitTestData()
		End Sub
		Public Shared Function Create() As WizardDataModel
			Return ViewModelSource.Create(Function() New WizardDataModel())
		End Function
		Protected Overridable Sub OnEndEditing(ByVal e As CustomClosingEventArgs)
			Dim handler As EventHandler(Of CustomClosingEventArgs) = EndEditingEvent
			If handler IsNot Nothing Then
				handler(Me, e)
			End If
		End Sub
		Public Function CreateDataSource() As SqlDataSource
			If SelectedDataSource Is Nothing Then
				Return Nothing
			End If
			Return SelectedDataSource.CreateSqlDataSource()
		End Function

		#Region "Data Source Add / Edit / Remove"
		Public Sub InitDataSource()
			Dim dsw As DataSourceWrapper = DataSourceWrapper.Create()
			dsw.BeginEdit()
			Dim acceptChanges As Boolean = OpenDataSourceEditor(dsw)
			If acceptChanges Then
				dsw.DoEndEdit()
				Me.DataSources.Add(dsw)
				Me.SelectedDataSource = dsw
			Else
				dsw.DoCancelEdit()
			End If
		End Sub
		Public Sub EditDataSource()
			Dim dsw As DataSourceWrapper = Me.SelectedDataSource
			dsw.BeginEdit()
			Dim acceptChanges As Boolean = OpenDataSourceEditor(dsw)
			If acceptChanges Then
			   dsw.DoEndEdit()
			Else
				dsw.DoCancelEdit()
			End If
		End Sub
		Private Function OpenDataSourceEditor(ByVal dsw As DataSourceWrapper) As Boolean
			Dim okCommand As New UICommand() With {.Caption = "OK", .IsCancel = False, .IsDefault = True, .Command = New DelegateCommand(Of CancelEventArgs)(Sub(x)
				End Sub, Function(x) String.IsNullOrEmpty(dsw.Error))}
			Dim cancelCommand As New UICommand() With {.Id = MessageBoxResult.Cancel, .Caption = "Cancel", .IsCancel = True, .IsDefault = False}
			Dim result As UICommand = EditDataSourceDialogService.ShowDialog(dialogCommands:= New List(Of UICommand)() From {okCommand, cancelCommand}, title:= "Edit Data Source", viewModel:= New Object() { dsw, Me.DataConnections })
			Dim acceptChanges As Boolean = result Is okCommand
			Return acceptChanges
		End Function
		Public Function CanAddDataSource() As Boolean
			Return True
		End Function
		Public Function CanEditDataSource() As Boolean
			Return Me.SelectedDataSource IsNot Nothing
		End Function
		Public Sub RemoveSelectedDataSource()
			Me.DataSources.Remove(Me.SelectedDataSource)
		End Sub
		Public Function CanRemoveSelectedDataSource() As Boolean
			Return (Me.DataSources.Count > 0 AndAlso Me.SelectedDataSource IsNot Nothing)
		End Function
		#End Region
		#Region "Query Add / Edit / Remove"
		Public Sub AddQuery()
			Dim qw As QueryWrapper = QueryWrapper.Create()
			qw.OwnerDataSource = Me.SelectedDataSource
			qw.BeginEdit()
			Dim acceptChanges As Boolean = OpenQueryEditor(qw)
			If acceptChanges Then
				qw.DoEndEdit()
				Me.SelectedDataSource.Queries.Add(qw)
				Me.SelectedDataSource.SelectedQuery = qw
			Else
				qw.DoCancelEdit()
			End If
		End Sub
		Private Function OpenQueryEditor(ByVal qw As QueryWrapper) As Boolean
			Dim okCommand As New UICommand() With {.Caption = "OK", .IsCancel = False, .IsDefault = True, .Command = New DelegateCommand(Of CancelEventArgs)(Sub(x)
				End Sub, Function(x) String.IsNullOrEmpty(qw.Error))}
			Dim cancelCommand As New UICommand() With {.Id = MessageBoxResult.Cancel, .Caption = "Cancel", .IsCancel = True, .IsDefault = False}
			Dim result As UICommand = EditQueryDialogService.ShowDialog(dialogCommands:= New List(Of UICommand)() From {okCommand, cancelCommand}, title:="Edit Query", viewModel:=qw)

			Dim acceptChanges As Boolean = result Is okCommand
			Return acceptChanges
		End Function
		Public Function CanAddQuery() As Boolean
			Return (Me.SelectedDataSource IsNot Nothing AndAlso Me.SelectedDataSource.Queries IsNot Nothing)
		End Function
		Public Sub EditQuery()
			Dim qw As QueryWrapper = Me.SelectedDataSource.SelectedQuery
			qw.BeginEdit()
			Dim acceptChanges As Boolean = OpenQueryEditor(qw)
			If acceptChanges Then
				qw.DoEndEdit()
				Me.SelectedDataSource.SelectedQuery = qw
			Else
				qw.DoCancelEdit()
			End If
		End Sub
		Public Function CanEditQuery() As Boolean
			Return Me.SelectedDataSource IsNot Nothing AndAlso Me.SelectedDataSource.SelectedQuery IsNot Nothing
		End Function
		Public Sub RemoveQuery()
			Me.SelectedDataSource.RemoveSelectedQuery()
		End Sub
		Public Function CanRemoveQuery() As Boolean
			Return (Me.SelectedDataSource IsNot Nothing AndAlso Me.SelectedDataSource.SelectedQuery IsNot Nothing)
		End Function
		#End Region        
		#Region "Test data source initialization"
		Private Sub InitTestData()
			Me.DataConnections = New List(Of ConnectionWrapper)()
			Me.DataSources = New ObservableCollection(Of DataSourceWrapper)()

			CreateNorthwind()
			CreateDepartmentsWorks()
			CreateGSP()

			AddInvalidConnection()
		End Sub
		Private Sub CreateGSP()
			Dim connectionParameters As New Access97ConnectionParameters("|DataDirectory|\AppData\gsp.mdb", "", "")
			Dim cw As ConnectionWrapper = InitConnection("GSPConnection", connectionParameters)
			Dim dsw As DataSourceWrapper = DataSourceWrapper.Create()
			dsw.Name = "GSP"
			dsw.Connection = cw
			Me.DataSources.Add(dsw)
		End Sub
		Private Sub CreateDepartmentsWorks()
			Dim connectionParameters As New Access97ConnectionParameters("|DataDirectory|\AppData\Departments.mdb", "", "")
			Dim cw As ConnectionWrapper = InitConnection("DepartmentsConnection", connectionParameters)
			Dim dsw As DataSourceWrapper = DataSourceWrapper.Create()
			dsw.Name = "Departments"
			dsw.Connection = cw
			Dim qwSalesPerson As QueryWrapper = QueryWrapper.Create()
			qwSalesPerson.OwnerDataSource = dsw
			qwSalesPerson.Name = "Departments"
			qwSalesPerson.Sql = "select * from Departments"
			dsw.Queries.Add(qwSalesPerson)

			Me.DataSources.Add(dsw)
		End Sub
		Private Sub CreateNorthwind()
			Dim connectionParameters As DataConnectionParametersBase = New Access97ConnectionParameters("|DataDirectory|\AppData\nwind.mdb", "", "")
			Dim cw As ConnectionWrapper = InitConnection("NorthwindConnection", connectionParameters)
			Dim dsw As DataSourceWrapper = DataSourceWrapper.Create()
			dsw.Name = "Northwind"
			dsw.Connection = cw
			Dim qwCategories As QueryWrapper = QueryWrapper.Create()
			qwCategories.OwnerDataSource = dsw
			qwCategories.Name = "Categories"
			qwCategories.Sql = "select * from Categories"
			dsw.Queries.Add(qwCategories)
			Dim qwProducts As QueryWrapper = QueryWrapper.Create()
			qwProducts.OwnerDataSource = dsw
			qwProducts.Name = "Products"
			qwProducts.Sql = "select * from Products"
			dsw.Queries.Add(qwProducts)
			Me.DataSources.Add(dsw)
		End Sub
		Private Function InitConnection(ByVal connectionName As String, ByVal connectionParameters As DataConnectionParametersBase) As ConnectionWrapper
			Dim cw As ConnectionWrapper = ConnectionWrapper.Create()
			cw.ConnectionName = connectionName
			cw.ConnectionParameters = connectionParameters
			Me.DataConnections.Add(cw)
			Return cw
		End Function
		Private Sub AddInvalidConnection()
			Dim cw As ConnectionWrapper = InitConnection("InvalidConnection", New CustomStringConnectionParameters("invalidConnection"))
		End Sub
		#End Region
		#Region "View Model Edit & Cancel methods"
		Public Sub DoCancelEdit()
			Me.CancelEdit()
			OnEndEditing(New CustomClosingEventArgs(CloseMode.Cancel))
		End Sub
		Public Function CanDoCancelEdit() As Boolean
			Return True
		End Function
		Public Sub DoEndEdit()
			Me.EndEdit()
			OnEndEditing(New CustomClosingEventArgs(CloseMode.Apply))
		End Sub
		Public Overridable Function CanDoEndEdit() As Boolean
			Return (Me.SelectedDataSource IsNot Nothing AndAlso Me.SelectedDataSource.SelectedQuery IsNot Nothing)
		End Function
		#End Region
		#Region "IEditableObject Members"
        <Command(False)>
        Public Sub BeginEdit() Implements IEditableObject.BeginEdit
            Cache = ViewModelSource.Create(Function() New WizardDataModel())
            For Each info In CurrentModel.GetType().GetProperties()
                If (Not info.CanRead) OrElse (Not info.CanWrite) Then
                    Continue For
                End If
                Dim oldValue = info.GetValue(CurrentModel, Nothing)
                Cache.GetType().GetProperty(info.Name).SetValue(Cache, oldValue, Nothing)
            Next info
        End Sub
        <Command(False)>
        Public Sub CancelEdit() Implements IEditableObject.CancelEdit
            For Each info In CurrentModel.GetType().GetProperties()
                If (Not info.CanRead) OrElse (Not info.CanWrite) Then
                    Continue For
                End If
                Dim oldValue = info.GetValue(Cache, Nothing)
                CurrentModel.GetType().GetProperty(info.Name).SetValue(CurrentModel, oldValue, Nothing)
            Next info
        End Sub
        <Command(False)>
        Public Sub EndEdit() Implements IEditableObject.EndEdit
            Cache = Nothing
        End Sub
		#End Region
	End Class
End Namespace

