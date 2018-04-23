Imports System
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports DevExpress.DataAccess.Sql
Imports DevExpress.Mvvm.POCO

Namespace CustomDataSourceWizard
    Public Class DataSourceWrapper
        Implements IEditableObject, IDataErrorInfo

        Public Shared Function Create() As DataSourceWrapper
            Dim dsw As DataSourceWrapper = ViewModelSource.Create(Function() New DataSourceWrapper())
            dsw.Queries = New ObservableCollection(Of QueryWrapper)()
            Return dsw
        End Function
        Protected Sub New()
        End Sub
        Private Property Cache() As DataSourceWrapper
        Private ReadOnly Property CurrentModel() As Object
            Get
                Return Me
            End Get
        End Property
        Public Overridable Property Name() As String
        Public Overridable Property Connection() As ConnectionWrapper
        Public Overridable Property SelectedQuery() As QueryWrapper
        Public Property Queries() As ObservableCollection(Of QueryWrapper)
        Public Function CreateSqlDataSource() As SqlDataSource
            Dim dataSource As New SqlDataSource() With {.Name = Me.Name, .ConnectionName = Me.Connection.ConnectionName, .ConnectionParameters = Me.Connection.ConnectionParameters}
            If Queries IsNot Nothing Then
                For Each query As QueryWrapper In Queries
                    dataSource.Queries.Add(query.CreateCustomSqlQuery())
                Next query
            End If
            dataSource.RebuildResultSchema()
            Return dataSource
        End Function

        Public Sub DoCancelEdit()
            DirectCast(Me, IEditableObject).CancelEdit()
        End Sub
        Public Function CanCancelEdit() As Boolean
            Return True
        End Function
        Public Sub DoEndEdit()
            DirectCast(Me, IEditableObject).EndEdit()
        End Sub
        Public Overridable Function CanEndEdit() As Boolean
            Return True
        End Function
        #Region "IEditableObject Members"
        Private Sub IEditableObject_BeginEdit() Implements IEditableObject.BeginEdit
            Cache = ViewModelSource.Create(Function() New DataSourceWrapper())
            For Each info In CurrentModel.GetType().GetProperties()
                If (Not info.CanRead) OrElse (Not info.CanWrite) Then
                    Continue For
                End If
                Dim oldValue = info.GetValue(CurrentModel, Nothing)
                Cache.GetType().GetProperty(info.Name).SetValue(Cache, oldValue, Nothing)
            Next info
        End Sub
        Private Sub IEditableObject_CancelEdit() Implements IEditableObject.CancelEdit
            For Each info In CurrentModel.GetType().GetProperties()
                If (Not info.CanRead) OrElse (Not info.CanWrite) Then
                    Continue For
                End If
                Dim oldValue = info.GetValue(Cache, Nothing)
                CurrentModel.GetType().GetProperty(info.Name).SetValue(CurrentModel, oldValue, Nothing)
            Next info
        End Sub
        Private Sub IEditableObject_EndEdit() Implements IEditableObject.EndEdit
            Cache = Nothing
        End Sub
        #End Region
        Private Sub qw_EndEditing(ByVal sender As Object, ByVal e As CustomClosingEventArgs)
            If e.CloseMode = CloseMode.Cancel Then
                Return
            End If
            Dim qw As QueryWrapper = TryCast(sender, QueryWrapper)
            Queries.Add(qw)
            SelectedQuery = qw
        End Sub
        Friend Sub RemoveSelectedQuery()
            Queries.Remove(SelectedQuery)
        End Sub

        #Region "IDataErrorInfo Members"
        Private ReadOnly Property IDataErrorInfo_Error() As String Implements IDataErrorInfo.Error
            Get
                Return DirectCast(Me, IDataErrorInfo)("Name") & DirectCast(Me, IDataErrorInfo)("Connection")
            End Get
        End Property

        Public ReadOnly Property IDataErrorInfo_Item(ByVal columnName As String) As String Implements IDataErrorInfo.Item
            Get
                Dim errorMessage As String = String.Empty
                Select Case columnName
                    Case "Name"
                        If String.IsNullOrEmpty(Me.Name) Then
                            errorMessage = "You cannot leave the Data source name empty."
                        End If
                    Case "Connection"
                        errorMessage = ValidateConnection()
                End Select
                Return errorMessage
            End Get
        End Property
        #End Region
        Private Function ValidateConnection() As String
            If Connection Is Nothing OrElse String.IsNullOrEmpty(Connection.ConnectionName) OrElse Connection.ConnectionParameters Is Nothing Then
                Return "You cannot leave the Connection empty"
            End If
            Dim testDS = New SqlDataSource With {.ConnectionName = "testName", .ConnectionParameters = Connection.ConnectionParameters}
            Try
                testDS.Connection.Open()
            Catch e As Exception
                Return e.Message
            End Try
            Return String.Empty
        End Function
    End Class
End Namespace
